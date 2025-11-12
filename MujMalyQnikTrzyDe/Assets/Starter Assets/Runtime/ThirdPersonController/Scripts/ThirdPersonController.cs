using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static Codice.Client.Common.EventTracking.TrackFeatureUseEvent.Features.DesktopGUI.Filters;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.5f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Voice Nudge")]
        public float PitchNudgeDuration = 0.2f;
        [Tooltip("Długość pchnięcia w metrach (forward)")]
        public float NudgeDistance = 2f;

        [Tooltip("Koszt jednorazowego Nudge w punktach staminy")]
        public float NudgeCost = 3f;

        private float _nudgeRemaining = 0f;
        private Vector3 _nudgeDir = Vector3.zero;

        [Header("Pitch Nudge Hop")]
        [Tooltip("Czy Nudge dodaje lekki podskok (hop)")]
        public bool NudgeHop = true;

        [Tooltip("Wysokość lekkiego podskoku w metrach")]
        public float NudgeHopHeight = 0.2f; // ~lekki hop   

        [Header("Punish (kara za głośny dźwięk)")]
        public float PunishDuration = 5f;   // czas kary domyślnie
        private float _punishTime = -1f;    // <0 = brak kary

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        // --- STAMINA: Ustawienia i stan ---
        [Header("Stamina")]
        [Tooltip("Maksymalna ilość staminy")]
        public float MaxStamina = 100f;

        [Tooltip("Zużycie staminy na sekundę podczas sprintu")]
        public float SprintDrainPerSecond = 3f;

        [Tooltip("Koszt jednorazowy skoku")]
        public float JumpCost = 30f;

        [Tooltip("Regeneracja staminy na sekundę podczas chodzenia (nie sprintu)")]
        public float RegenWhileWalkingPerSecond = 5f;

        [Tooltip("Regeneracja staminy na sekundę podczas stania (brak ruchu)")]
        public float RegenWhileIdlePerSecond = 15f;

        [Tooltip("Minimalna stamina wymagana, by zacząć/utrzymać sprint")]
        public float MinStaminaToSprint = 5f;

        [Tooltip("Minimalna stamina wymagana, by wykonać skok")]
        public float MinStaminaToJump = 10f;

        private float _stamina;          // aktualna stamina
        private bool _isSprinting;       // czy faktycznie sprintujemy w tej klatce
        private bool _isMoving;          // czy jest wejście ruchu (niezerowe)

        // --- REARING: odchylenie konia przy głośnym dźwięku (bez korutyn) ---
        [Header("Rearing (głośny dźwięk)")]
        [Tooltip("Czas trwania wspięcia (sekundy)")]
        public float RearDuration = 0.8f;

        [Tooltip("Maksymalny kąt odchyłu do tyłu w stopniach (oś X, dodatnie wartości = do tyłu)")]
        public float RearMaxAngle = 35f;

        [Tooltip("Prędkość lekkiego cofania podczas wspięcia (m/s)")]
        public float RearBackwardSpeed = 0.6f;

        // stan rearing
        private float _rearTime = -1f;        // <0 = brak rearing, inaczej czas trwania od startu
        private float _currentRearTiltX = 0f; // aktualny kąt odchyłu (X)

        [Header("Audio (reakcje konia)")]
        [Tooltip("Dźwięk rżenia / wystraszenia konia podczas rearingu")]
        public AudioClip ScaredAudioClip;

        [Range(0, 1)]
        public float ScaredAudioVolume = 1.0f;

        [Tooltip("Dźwięk wyskoku")]
        public AudioClip JumpAudioClip;

        [Range(0, 1)]
        public float JumpAudioVolume = 1.0f;

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            _stamina = MaxStamina;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // najpierw zaktualizuj stan wspięcia (określa tilt X i blokady)
            UpdateRear();

            if (_punishTime >= 0f)
            {
                _punishTime -= Time.deltaTime;
            }

            JumpAndGravity();
            GroundedCheck();
            Move();
            UpdateStamina();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        public void TriggerPunish(float duration)
        {
            _punishTime = duration;
        }

        public void TriggerPitchNudge(float distance)
        {
            if (_mainCamera == null) return;

            // Kierunek = forward kamery rzutowany na płaszczyznę XZ
            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude < 0.0001f)
                camForward = transform.forward; // awaryjnie

            _stamina = Mathf.Max(0f, _stamina - NudgeCost);

            _nudgeDir = camForward.normalized;
            _nudgeRemaining = distance;

            // >>> DODANE: lekki podskok podczas Nudge
            // warunki: włączone NudgeHop, brak kary/punish, nie w trakcie rearing,
            // najlepiej gdy stoimy na ziemi (żeby nie wzmacniać już trwającego skoku)
            if (NudgeHop && _punishTime < 0f && _rearTime < 0f && Grounded)
            {
                // v = sqrt(h * -2 * g)  (Gravity jest ujemne)
                float hopVelocity = Mathf.Sqrt(Mathf.Max(0.0001f, NudgeHopHeight) * -2f * Gravity);

                // nie nadpisuj większej prędkości w górę (np. pełny skok)
                if (_verticalVelocity < hopVelocity)
                {
                    _verticalVelocity = hopVelocity;
                }
            }
        }

        // --- PUBLICZNE: wyzwolenie wspięcia (rearing) ---
        public void TriggerRear()
        {
            // jeśli już trwa, nie restartuj w kółko
            if (_rearTime < 0f)
            {
                _rearTime = 0f;

                if (ScaredAudioClip != null)
                {
                    AudioSource.PlayClipAtPoint(ScaredAudioClip, transform.position, ScaredAudioVolume);
                }
            }
        }

        // --- AKTUALIZACJA STANU REARING ---
        private void UpdateRear()
        {
            if (_rearTime < 0f)
            {
                _currentRearTiltX = 0f;
                return;
            }

            _rearTime += Time.deltaTime;
            float t = Mathf.Clamp01(_rearTime / Mathf.Max(0.0001f, RearDuration));

            // gładkie wejście/wyjście: sin(pi * t) -> 0..1..0
            float envelope = Mathf.Sin(t * Mathf.PI);

            // odchył do tyłu (oś X): dodatnie w górę/tył – jeśli chcesz odwrócić, zmień znak
            _currentRearTiltX = -envelope * RearMaxAngle;

            if (t >= 1f)
            {
                _rearTime = -1f;
                _currentRearTiltX = 0f;
            }
        }

        public void Move()
        {
            // Brak wejścia ruchu w tej wersji kontrolera
            _isMoving = false;
            _isSprinting = false;

            // Zawsze ustawiamy rotację: patrz w stronę kamery + przechył X (rearing)
            float cameraYaw = _mainCamera != null ? _mainCamera.transform.eulerAngles.y : transform.eulerAngles.y;
            float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, cameraYaw, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(_currentRearTiltX, rotationY, 0.0f);

            // Zaczynamy tylko od składowej grawitacji
            Vector3 move = new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

            // --- KARA: pełna blokada ruchu poziomego, ale przechył jest widoczny ---
            if (_punishTime >= 0f)
            {
                _speed = 0f;
                _animationBlend = 0f;

                // Bez cofania i bez nudge podczas kary
                _controller.Move(move);

                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                    _animator.SetFloat(_animIDMotionSpeed, 0f);
                }
                return;
            }

            // Poza karą: brak chodzenia/sprintu, ale płynnie wygaszamy blend
            _speed = 0f;
            _animationBlend = Mathf.Lerp(_animationBlend, 0f, Time.deltaTime * SpeedChangeRate);

            // Cofanie podczas wspięcia (rearing) – delikatne, zależne od obwiedni
            if (_rearTime >= 0f)
            {
                float t = Mathf.Clamp01(_rearTime / Mathf.Max(0.01f, RearDuration));
                float envelope = Mathf.Sin(t * Mathf.PI);
                move += -transform.forward * (RearBackwardSpeed * envelope * Time.deltaTime);
            }

            // Pchnięcie po wysokim tonie (nudge)
            if (_nudgeRemaining > 0f && _nudgeDir.sqrMagnitude > 1e-6f)
            {
                float nudgeSpeed = NudgeDistance / Mathf.Max(0.01f, PitchNudgeDuration);
                float step = Mathf.Min(_nudgeRemaining, nudgeSpeed * Time.deltaTime);
                move += _nudgeDir * step;
                _nudgeRemaining -= step;
            }

            _controller.Move(move);

            // Animator
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
        }

        private void JumpAndGravity()
        {
            if (_punishTime >= 0f)
            {
                _input.jump = false;
                return;
            }

            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // W czasie rearing nie pozwalaj na skok
                bool isRearing = _rearTime >= 0f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f && !isRearing)
                {
                    if (_stamina >= MinStaminaToJump && _stamina >= JumpCost)
                    {
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                        if (JumpAudioClip != null)
                        {
                            AudioSource.PlayClipAtPoint(JumpAudioClip, transform.position, JumpAudioVolume);
                        }

                        _stamina = Mathf.Max(0f, _stamina - JumpCost);

                        TriggerPitchNudge(5f);
                        _jumpTimeoutDelta = JumpTimeout;
                        _input.jump = false;

                        if (_hasAnimator)
                        {
                            _animator.SetBool(_animIDJump, true);
                        }
                    }
                    else
                    {
                        _input.jump = false;
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        // --- STAMINA: logika zużycia i regeneracji ---
        private void UpdateStamina()
        {
            float delta = 0f;

            if (_isSprinting)
            {
                delta -= SprintDrainPerSecond * Time.deltaTime;
            }
            else
            {
                if (_isMoving)
                    delta += RegenWhileWalkingPerSecond * Time.deltaTime;
                else
                    delta += RegenWhileIdlePerSecond * Time.deltaTime;
            }

            _stamina = Mathf.Clamp(_stamina + delta, 0f, MaxStamina);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public float GetStamina01() => MaxStamina <= 0.01f ? 0f : _stamina / MaxStamina;
        public float GetStamina() => _stamina;
        public float GetMaxStamina() => MaxStamina;
        public float GetDistance() => NudgeDistance;
        public float GetJumpHeight() => JumpHeight;
        public float GetRegenSpeed() => RegenWhileWalkingPerSecond;
        public void SetStamina(float value) => _stamina = Mathf.Clamp(value, 0f, MaxStamina);
    }
}
