using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    public float pitch;
    public float minFreq = 60f;
    public float maxFreq = 2000f;

    public ThirdPersonController player;
    private AudioSource audioSource;
    private const int sampleSize = 2048;
    private float[] audioData = new float[sampleSize];
    private float[] spectrum = new float[sampleSize];

    public string MicrophoneDevice = "Mikrofon (Arctis Nova 7)";

    public float noiseThreshold = 0.005f;
    public float loudThreshold = 0.1f;

    // Timeout na wykrywanie wysokich tonów
    public float pitchCooldown = 0.1f; // sekundy
    private float nextPitchTime = 0f;

    // Cooldown na g³oœny dŸwiêk (¿eby nie spamowaæ wspiêciem)
    public float loudCooldown = 1.0f; // sekundy
    private float nextLoudTime = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = true;

        if (Microphone.devices.Length == 0)
        {
            Debug.Log("Brak urz¹dzeñ mikrofonowych.");
            enabled = false;
            return;
        }
        else
        {
            Debug.Log("=== DOSTÊPNE MIKROFONY ===");
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"[{i}] {Microphone.devices[i]}");
            }
        }

        audioSource.clip = Microphone.Start(MicrophoneDevice, true, 1, 48000);

        while (Microphone.GetPosition(MicrophoneDevice) <= 0) { }

        audioSource.Play();
    }

    void Update()
    {
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

        // Wysoki ton -> nudge (z istniej¹cego kodu) + cooldown
        if (pitch > 1000f && Time.time >= nextPitchTime)
        {
            Debug.Log("Wysoki ton!");
            if (player != null) player.TriggerPitchNudge(player.GetDistance());
            nextPitchTime = Time.time + pitchCooldown;
        }
    }

    float CalculateRMS()
    {
        float sum = 0f;
        for (int i = 0; i < audioData.Length; i++)
        {
            sum += audioData[i] * audioData[i];
        }
        return Mathf.Sqrt(sum / audioData.Length);
    }

    float DetectPitch()
    {
        int peakIndex = 0;
        float maxCorr = 0f;

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

        if (peakIndex == 0)
            return 0f;

        float freq = AudioSettings.outputSampleRate / peakIndex;

        if (freq < minFreq || freq > maxFreq)
            return 0f;

        return freq;
    }
}
