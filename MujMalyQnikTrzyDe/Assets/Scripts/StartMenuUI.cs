using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class StartMenuUI : MonoBehaviour
{
    [Header("Panel startowy (Canvas/Panel)")]
    public GameObject panel;

    [Header("UI")]
    public TMP_Dropdown dropdownTMP;
    public Button startButton;

    [Header("Pauza gry na starcie")]
    public bool pauseOnStart = true;

    private MicrophoneManager audioMgr;

    private void Awake()
    {
        audioMgr = MicrophoneManager.Instance;
        if (audioMgr == null)
        {
            // utwórz poprawnie – AudioSource zostanie dodany dziêki [RequireComponent]
            var go = new GameObject("MicrophoneManager");
            audioMgr = go.AddComponent<MicrophoneManager>();
        }

        if (pauseOnStart) Time.timeScale = 0f;
        if (panel != null) panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        // Auto-znajdŸ kontrolki jeœli nie przypiête
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

        PopulateDevices();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }

        audioMgr.StartPreview();

        Debug.Log("StartMenuUI: gotowe, czekam na klik 'Graj'.");
    }

    private void PopulateDevices()
    {
        var devices = audioMgr.GetDevices();
        var options = new List<string>(devices);

        if (dropdownTMP != null)
        {
            dropdownTMP.ClearOptions();
            dropdownTMP.AddOptions(options);
            dropdownTMP.onValueChanged.RemoveAllListeners();
            dropdownTMP.onValueChanged.AddListener(i => OnDeviceSelected(options, i));

            if (options.Count > 0)
            {
                dropdownTMP.value = 0;
                OnDeviceSelected(options, 0);
            }
            else
            {
                Debug.LogWarning("StartMenuUI: Nie wykryto ¿adnych mikrofonów.");
            }
        }
    }

    private void OnDeviceSelected(List<string> options, int index)
    {
        if (options == null || options.Count == 0) return;
        string dev = options[Mathf.Clamp(index, 0, options.Count - 1)];
        audioMgr.SetSelectedDevice(dev);

        audioMgr.StopPreview();
        audioMgr.StartPreview();
    }

    private void OnStartClicked()
    {
        Debug.Log("StartMenuUI: klikniêto 'Graj'.");

        audioMgr.CommitSelection();

        if (panel) panel.SetActive(false);
        if (pauseOnStart) Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
