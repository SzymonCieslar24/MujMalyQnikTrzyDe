using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndMenuUI : MonoBehaviour
{
    [Header("Root panel turniejowy (tylko ten chowamy)")]
    [SerializeField] private GameObject canvasRoot;

    [Header("Referencje TMP")]
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI ScoreText;

    public Button startButton;

    [Header("Formaty")]
    public string timeFormat = "{0:00}:{1:00}.{2:00}";
    public string scoreFormat = "{0}";

    [Header("Zachowanie")]
    [Tooltip("Czy ukryæ panel turniejowy przy starcie gry.")]
    [SerializeField] private bool hideOnAwake = true;

    private void Awake()
    {
        if (canvasRoot == null) canvasRoot = gameObject;

        if (hideOnAwake && canvasRoot.activeSelf)
            canvasRoot.SetActive(false);

        if (startButton != null)
        {
            startButton.onClick.AddListener(BackToPadok);
        }
    }

    public void Show()
    {
        Debug.Log($"Is UI active before: {canvasRoot.activeSelf}");
        if (canvasRoot != null && !canvasRoot.activeSelf)
        {
            canvasRoot.SetActive(true);
        }
        Debug.Log($"Is UI active after: {canvasRoot.activeSelf}");
    }

    public void Hide()
    {
        if (canvasRoot != null && canvasRoot.activeSelf)
            canvasRoot.SetActive(false);
    }

    private void HideAndRestart()
    {
        Hide();
        TrackManager.Instance.ResetRun();
    }

    private void BackToPadok()
    {
        // Przejœcie do sceny "Padok"
        SceneManager.LoadScene("Padok");
    }

    private void Update()
    {
        if (canvasRoot == null || !canvasRoot.activeInHierarchy) return;

        var tm = TrackManager.Instance;
        if (tm == null) return;

        // Jeœli wyœcig siê zakoñczy³, u¿yj zapisanego czasu
        float t = tm.finalRunTime > 0f ? tm.finalRunTime : tm.RunElapsed;

        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int centis = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);

        if (TimeText) TimeText.text = string.Format(timeFormat, minutes, seconds, centis);
        if (ScoreText) ScoreText.text = string.Format(scoreFormat, tm.GetScore());
    }
}