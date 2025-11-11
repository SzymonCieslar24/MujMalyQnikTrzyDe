using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StartMenuUI : MonoBehaviour
{
    [Header("Panel startowy (Canvas/Panel)")]
    public GameObject panel;

    [Header("UI")]
    public TMP_Dropdown dropdownTMP;
    public Button startButton;

    [Header("Powi¹zania")]
    [Tooltip("Referencja do komponentu MicrophoneInput, który ma u¿ywaæ wybranego urz¹dzenia.")]
    public MicrophoneInput micInput;

    [Header("Pauza gry na starcie")]
    public bool pauseOnStart = true;

    private List<string> _options = new List<string>();
    private const string PlayerPrefsKey = "mic_device";

    private void Awake()
    {
        if (pauseOnStart) Time.timeScale = 0f;
        if (panel != null) panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        if (startButton == null)
        {
            startButton = GetComponentInChildren<Button>(true);
            if (startButton == null)
                Debug.LogError("StartMenuUI: startButton nie jest przypiêty i nie znaleziono go w dzieciach.");
        }
        if (dropdownTMP == null)
        {
            dropdownTMP = GetComponentInChildren<TMP_Dropdown>(true);
            if (dropdownTMP == null)
                Debug.LogError("StartMenuUI: dropdownTMP nie jest przypiêty i nie znaleziono go w dzieciach.");
        }
        if (micInput == null)
        {
            micInput = FindObjectOfType<MicrophoneInput>();
            if (micInput == null)
                Debug.LogWarning("StartMenuUI: Nie znaleziono MicrophoneInput w scenie.");
        }

        PopulateDevices();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }

        Debug.Log("StartMenuUI: gotowe, czekam na klik 'Graj'.");
    }

    private void PopulateDevices()
    {
        var devices = Microphone.devices;
        _options = new List<string>(devices);

        if (dropdownTMP != null)
        {
            dropdownTMP.ClearOptions();
            dropdownTMP.AddOptions(_options);
            dropdownTMP.onValueChanged.RemoveAllListeners();
            dropdownTMP.onValueChanged.AddListener(OnDropdownChanged);

            if (_options.Count > 0)
            {
                // Ustaw z PlayerPrefs (jeœli istnieje), w przeciwnym razie pierwsze urz¹dzenie
                string saved = PlayerPrefs.GetString(PlayerPrefsKey, _options[0]);
                int idx = Mathf.Max(0, _options.IndexOf(saved));
                dropdownTMP.value = idx;
                dropdownTMP.RefreshShownValue();

                ApplySelectedDevice(idx);
            }
            else
            {
                Debug.LogWarning("StartMenuUI: Nie wykryto ¿adnych mikrofonów.");
            }
        }
    }

    private void OnDropdownChanged(int index)
    {
        ApplySelectedDevice(index);
    }

    private void ApplySelectedDevice(int index)
    {
        if (_options == null || _options.Count == 0) return;
        index = Mathf.Clamp(index, 0, _options.Count - 1);
        string dev = _options[index];

        // Zapisz wybór
        PlayerPrefs.SetString(PlayerPrefsKey, dev);
        PlayerPrefs.Save();

        // Przeka¿ do MicrophoneInput
        if (micInput != null)
        {
            micInput.SetDevice(dev);
        }
        else
        {
            Debug.LogWarning($"StartMenuUI: Brak referencji do MicrophoneInput. Wybrane urz¹dzenie: {dev}");
        }
    }

    private void OnStartClicked()
    {
        Debug.Log("StartMenuUI: klikniêto 'Graj'.");

        if (panel) panel.SetActive(false);
        if (pauseOnStart) Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
