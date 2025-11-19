using StarterAssets;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    [Header("Pitch / zakres")]
    public float pitch;
    public float minFreq = 60f;
    public float maxFreq = 2000f;

    [Header("Progi g³oœnoœci")]
    public float noiseThreshold = 0.005f;
    public float loudThreshold = 0.1f;

    [Header("Cooldowny")]
    [Tooltip("Minimalny odstêp miêdzy wyzwoleniami reakcji na wysoki ton")]
    public float pitchCooldown = 0.1f; // s
    [Tooltip("Minimalny odstêp miêdzy karami za zbyt g³oœny dŸwiêk")]
    public float loudCooldown = 1.0f; // s

    [Header("Powi¹zania")]
    public ThirdPersonController player;

    [Header("Urz¹dzenie mikrofonowe (tylko do podgl¹du)")]
    [SerializeField] private string MicrophoneDevice = "";

    private AudioSource audioSource;
    private const int sampleSize = 2048;
    private readonly float[] audioData = new float[sampleSize];
    private readonly float[] spectrum = new float[sampleSize];

    private float nextPitchTime = 0f;
    private float nextLoudTime = 0f;
    private Coroutine initRoutine;
    private int sampleRate;

    private const string PlayerPrefsKey = "mic_device";

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        sampleRate = AudioSettings.outputSampleRate;
    }

    private void Start()
    {

        PlayerPrefs.DeleteKey(PlayerPrefsKey);

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("MicrophoneInput: Brak urz¹dzeñ mikrofonowych. Wy³¹czam komponent.");
            enabled = false;
            return;
        }

        // Spróbuj wczytaæ ostatni wybór (zapisany przez StartMenuUI)
        if (string.IsNullOrEmpty(MicrophoneDevice))
        {
            string saved = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (!string.IsNullOrEmpty(saved))
            {
                SetDevice(saved);
            }
            else
            {
                // Fallback: pierwsze urz¹dzenie
                SetDevice(Microphone.devices[0]);
            }
        }
        else
        {
            // Jeœli ktoœ ustawi³ w Inspectorze — u¿yj
            SetDevice(MicrophoneDevice);
        }
    }

    /// <summary>
    /// Publiczna metoda ustawiaj¹ca urz¹dzenie mikrofonowe i restartuj¹ca nas³uch.
    /// </summary>
    public void SetDevice(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName))
        {
            Debug.LogWarning("MicrophoneInput: Pusta nazwa urz¹dzenia — ignorujê.");
            return;
        }

        // Sprawdzamy, czy urz¹dzenie istnieje na liœcie dostêpnych mikrofonów
        bool deviceFound = false;
        foreach (var device in Microphone.devices)
        {
            if (device == deviceName)
            {
                deviceFound = true;
                break;
            }
        }

        if (!deviceFound)
        {
            Debug.LogError($"MicrophoneInput: Urz¹dzenie '{deviceName}' nie zosta³o znalezione. U¿ywam pierwszego dostêpnego mikrofonu.");
            deviceName = Microphone.devices.Length > 0 ? Microphone.devices[0] : "";
        }

        // Je¿eli to ju¿ to samo urz¹dzenie i dzia³a – nic nie rób
        if (!string.IsNullOrEmpty(MicrophoneDevice) && MicrophoneDevice == deviceName && audioSource.clip != null && Microphone.IsRecording(MicrophoneDevice))
            return;

        MicrophoneDevice = deviceName;

        // Zatrzymaj ewentualny poprzedni nas³uch
        StopListening();

        // Rozpocznij inicjalizacjê asynchronicznie (bez blokowania g³ównego w¹tku)
        if (initRoutine != null) StopCoroutine(initRoutine);
        initRoutine = StartCoroutine(InitMicrophoneCoroutine(MicrophoneDevice));
    }


    private IEnumerator InitMicrophoneCoroutine(string device)
    {
        if (string.IsNullOrEmpty(device))
            yield break;

        // Bezpieczna inicjalizacja
        int lengthSec = 1; // krótka pêtla – do analizy wystarczy
        audioSource.clip = Microphone.Start(device, true, lengthSec, sampleRate);

        // Czekaj a¿ mikrofon zacznie zwracaæ próbki
        float startTime = Time.unscaledTime;
        const float timeout = 2f; // sekundy
        while (Microphone.GetPosition(device) <= 0)
        {
            if (Time.unscaledTime - startTime > timeout)
            {
                Debug.LogWarning($"MicrophoneInput: Timeout czekania na urz¹dzenie '{device}'.");
                yield break;
            }
            yield return null; // odpuœæ jedn¹ klatkê
        }

        audioSource.Play();
        Debug.Log($"MicrophoneInput: S³ucham z urz¹dzenia: {device} @ {sampleRate} Hz");
    }

    private void StopListening()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            if (audioSource.clip != null)
            {
                // Zatrzymaj aktualny mikrofon (jeœli jakiœ by³)
                try
                {
                    if (!string.IsNullOrEmpty(MicrophoneDevice) && Microphone.IsRecording(MicrophoneDevice))
                        Microphone.End(MicrophoneDevice);
                }
                catch { /* nie szkodzi */ }

                audioSource.clip = null;
            }
        }
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void OnDestroy()
    {
        StopListening();
    }

    private void Update()
    {
        if (audioSource == null || audioSource.clip == null) return;

        audioSource.GetOutputData(audioData, 0);
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hanning);

        float rmsValue = CalculateRMS();

        if (rmsValue < noiseThreshold)
            return;

        // --- REARING na g³oœny dŸwiêk ---
        if (rmsValue > loudThreshold && Time.time >= nextLoudTime)
        {
            Debug.Log("Zbyt g³oœny dŸwiêk! Rearing + PUNISH!");
            if (player != null)
            {
                player.TriggerRear();
                player.TriggerPunish(player.PunishDuration);
            }
            nextLoudTime = Time.time + loudCooldown;
        }

        pitch = DetectPitch();

        // Wysoki ton -> nudge + cooldown
        if (pitch > 1000f && Time.time >= nextPitchTime)
        {
            Debug.Log("Wysoki ton!");
            if (player != null) player.TriggerPitchNudge(player.GetDistance());
            nextPitchTime = Time.time + pitchCooldown;
        }
    }

    private float CalculateRMS()
    {
        float sum = 0f;
        for (int i = 0; i < audioData.Length; i++)
            sum += audioData[i] * audioData[i];
        return Mathf.Sqrt(sum / audioData.Length);
    }

    private float DetectPitch()
    {
        int peakIndex = 0;
        float maxCorr = 0f;

        // Prosta autokorelacja
        for (int lag = 20; lag < sampleSize / 2; lag++)
        {
            float corr = 0f;
            for (int i = 0; i < sampleSize / 2; i++)
                corr += audioData[i] * audioData[i + lag];

            if (corr > maxCorr)
            {
                maxCorr = corr;
                peakIndex = lag;
            }
        }

        if (peakIndex == 0) return 0f;

        float freq = sampleRate / (float)peakIndex;
        if (freq < minFreq || freq > maxFreq) return 0f;

        return freq;
    }
}
