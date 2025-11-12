using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SpamManager : MonoBehaviour
{
    [Header("UI")]
    public Slider Meter;                   
    public TMP_Text HudText;               
    public TMP_Text PromptText;            
    public TMP_Text ResultText;
    public Button BackButton;
    public string BackSceneName = "Padok";

    [Header("Parametry bazowe")]
    public int StartLevel = 1;            
    public float BaseGain = 0.1f;         
    public float BaseDecay = 0.15f;        
    public float BasePenalty = 0.10f;      
    public float BaseTimeLimit = 5.0f;

    [Header("Skalowanie trudności na poziom")]
    public float GainFactorPerLevel = 0.90f;   
    public float DecayFactorPerLevel = 1.1f;  
    public float PenaltyFactorPerLevel = 1.05f;
    public float TimeFactorPerLevel = 0.95f;   

    [Header("Efekty wizualne")]
    public Color MeterBase = new Color(0.6f, 0.6f, 0.6f);
    public Color MeterGood = new Color(0.2f, 0.85f, 0.3f);
    public Color MeterBad = new Color(1.0f, 0.3f, 0.3f);

    // ——— wewnętrzne ———
    private float TargetFill = 1;
    private bool _startedInput = false;
    private int _level;
    private float _gain, _decay, _penalty, _timeLimit;
    private float _timeLeft;
    private float _meter;
    private int _lastDir = 0; 
    private bool _inRound = false;
    private Image _meterFill;

    public void ResetLevel()
    {
        _level = 1;
        StartRound();
    }

    private void Awake()
    {
        if (BackButton) BackButton.onClick.AddListener(() => SceneManager.LoadScene(BackSceneName));
        if (Meter != null)
        {
            var fill = Meter.fillRect ? Meter.fillRect.GetComponent<Image>() : null;
            _meterFill = fill;
        }
        if (PromptText) PromptText.text = "Spamuj naprzemiennie: A ⇆ D  lub  ← ⇆ →";
        if (ResultText) ResultText.text = "Spamuj naprzemiennie: A ⇆ D  lub  ← ⇆ →";
    }

    private void Start()
    {
        _level = Mathf.Max(1, StartLevel);
        StartRound();
    }

    private void StartRound()
    {
        // skalowanie trudności na podstawie poziomu
        int L = _level - 1;
        _gain = Mathf.Max(0.03f, BaseGain * Mathf.Pow(GainFactorPerLevel, L));
        _decay = BaseDecay * Mathf.Pow(DecayFactorPerLevel, L);
        _penalty = BasePenalty * Mathf.Pow(PenaltyFactorPerLevel, L);
        _timeLimit = Mathf.Max(2.5f, BaseTimeLimit * Mathf.Pow(TimeFactorPerLevel, L));

        _startedInput = false;  
        _timeLeft = _timeLimit;

        _meter = 0f;
        _timeLeft = _timeLimit;
        _lastDir = 0;
        _startedInput = false;
        _inRound = true;

        UpdateHUD();
        UpdateMeter(0f, MeterBase);
        if (ResultText) ResultText.text = "";
    }

    private void Update()
    {
        if (!_inRound) return;

        // jednorazowy odczyt wejścia w tej klatce
        int inputDir = ReadDirection();

        // start licznika czasu dopiero po pierwszym kliknięciu
        if (!_startedInput && inputDir != 0)
            _startedInput = true;

        // --- PRZED STARTEM: czas stoi, brak opadania, możesz już nabijać pasek ---
        if (!_startedInput)
        {
            HandleInput(inputDir);
            UpdateMeter(_meter, MeterBase);
            UpdateHUD();
            return;
        }

        // --- FAZA GRY: działa czas i opadanie paska ---
        // opadanie w czasie
        _meter = Mathf.Max(0f, _meter - _decay * Time.deltaTime);
        UpdateMeter(_meter, MeterBase);

        // wejście gracza (nagroda/kara za prawidłową/nieprawidłową naprzemienność)
        HandleInput(inputDir);

        // czas
        _timeLeft = Mathf.Max(0f, _timeLeft - Time.deltaTime);
        UpdateHUD();

        // warunki końca rundy
        if (_meter >= TargetFill)
        {
            RoundWin();
        }
        else if (_timeLeft <= 0f)
        {
            RoundLose();
        }
    }

    private void RoundWin()
    {
        _inRound = false;
        _startedInput = false; 
        if (ResultText) ResultText.text = "Sukces!";
        StartCoroutine(FlashMeter(MeterGood, 0.5f));
        _level++;
        Invoke(nameof(StartRound), 0.7f);
    }

    private void RoundLose()
    {
        _inRound = false;
        _startedInput = false;
        if (ResultText) ResultText.text = "Porażka :(";
        StartCoroutine(FlashMeter(MeterBad, 0.5f));
        Invoke(nameof(StartRound), 0.8f);
    }

    private int ReadDirection()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return 0;
        bool left = kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame;
        bool right = kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;
#else
    bool left  = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    bool right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
#endif
        if (left == right) return 0;
        return right ? +1 : -1;
    }


    private void UpdateMeter(float value, Color c)
    {
        if (Meter) Meter.value = value;
        if (_meterFill != null)
        {
            c.a = 1f;
            _meterFill.color = c;
        }
    }

    private System.Collections.IEnumerator FlashMeter(Color c, float t)
    {
        if (_meterFill != null)
        {
            var before = _meterFill.color;
            _meterFill.color = c;
            yield return new WaitForSeconds(t);
            _meterFill.color = before;
        }
    }

    private void UpdateHUD()
    {
        if (!HudText) return;
        HudText.text = $"Poziom: {_level}\nCzas: {_timeLeft:0.0}s";
    }

    private void HandleInput(int inputDir)
    {
        if (inputDir != 0)
        {
            if (inputDir != _lastDir)
            {
                _lastDir = inputDir;
                _meter = Mathf.Min(1f, _meter + _gain);
                UpdateMeter(_meter, MeterGood);
            }
            else
            {
                _meter = Mathf.Max(0f, _meter - _penalty);
                UpdateMeter(_meter, MeterBad);
            }
        }
    }

}
