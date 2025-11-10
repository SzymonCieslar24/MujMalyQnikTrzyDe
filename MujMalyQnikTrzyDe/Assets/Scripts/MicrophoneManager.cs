using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance { get; private set; }

    [Tooltip("Czêstotliwoœæ próbkowania dla podgl¹du.")]
    public int sampleRate = 48000;

    [Tooltip("Czy w³¹czyæ ods³uch mikrofonu w g³oœnikach podczas podgl¹du.")]
    public bool monitorToSpeakers = false;

    public string SelectedDevice { get; private set; }

    private AudioSource _audioSource;
    private bool _previewRunning = false;

    //private MicrophoneInput micInput;

    private const string PREF_KEY = "SelectedMicrophone";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!TryGetComponent(out _audioSource))
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.mute = !monitorToSpeakers;

        Debug.Log($"MicrophoneManager Awake on {gameObject.name}");

        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            string savedDevice = PlayerPrefs.GetString(PREF_KEY);
            if (System.Array.Exists(Microphone.devices, d => d == savedDevice))
            {
                SelectedDevice = savedDevice;
            }
        }
    }

    public string[] GetDevices() => Microphone.devices;

    public string GetSelectedDevice() => SelectedDevice;

    public void SetSelectedDevice(string deviceName)
    {
        SelectedDevice = deviceName;

        PlayerPrefs.SetString(PREF_KEY, deviceName);
        PlayerPrefs.Save();

        Debug.Log($"Zapisano wybrany mikrofon: {deviceName}");
    }

    public void StartPreview()
    {
        if (string.IsNullOrEmpty(SelectedDevice))
        {
            var devs = GetDevices();
            if (devs.Length > 0)
            {
                SelectedDevice = devs[0];
                Debug.Log($"Ustawiono domyœlny mikrofon: {SelectedDevice}");
            }
            else
            {
                Debug.LogWarning("MicrophoneManager: Brak urz¹dzeñ mikrofonowych.");
                return;
            }
        }

        if (_previewRunning) return;

        var clip = Microphone.Start(SelectedDevice, true, 1, sampleRate);

        int safety = 0;
        while (Microphone.GetPosition(SelectedDevice) <= 0 && safety++ < 1000) { }

        _audioSource.clip = clip;
        _audioSource.Play();
        _previewRunning = true;

        Debug.Log($"Podgl¹d mikrofonu uruchomiony ({SelectedDevice}).");
    }

    public void StopPreview()
    {
        if (!_previewRunning) return;

        _audioSource.Stop();
        if (Microphone.IsRecording(SelectedDevice))
            Microphone.End(SelectedDevice);

        _audioSource.clip = null;
        _previewRunning = false;
    }

    public float GetLevelRms()
    {
        if (!_previewRunning || _audioSource.clip == null) return 0f;

        float[] data = new float[256];
        _audioSource.GetOutputData(data, 0);
        float sum = 0f;
        for (int i = 0; i < data.Length; i++)
            sum += data[i] * data[i];
        return Mathf.Sqrt(sum / data.Length);
    }

    public void CommitSelection()
    {
        //micInput = FindAnyObjectByType<MicrophoneInput>();
        //micInput.StartMic(SelectedDevice);

        StopPreview();
        Debug.Log($"Wybrany mikrofon zatwierdzony: {SelectedDevice}");
    }
}
