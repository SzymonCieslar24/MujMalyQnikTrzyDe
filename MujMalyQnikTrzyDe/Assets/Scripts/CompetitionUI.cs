using UnityEngine;
using TMPro;

public class CompetitionUI : MonoBehaviour
{
    [Header("Root panel turniejowy (tylko ten chowamy)")]
    [SerializeField] private GameObject tournamentRoot; // <- ustaw tu obiekt Panel/Container dla UI zawodów

    [Header("Referencje TMP")]
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI GoalText;

    [Header("Formaty")]
    public string timeFormat = "{0:00}:{1:00}.{2:00}";
    public string scoreFormat = "{0}";
    public string clearedFormat = "{0}/{1}";

    [Header("Zachowanie")]
    [Tooltip("Czy ukryæ panel turniejowy przy starcie gry.")]
    [SerializeField] private bool hideOnAwake = true;

    private void Awake()
    {
        // Jeœli nie podano, przyjmij ¿e skrypt siedzi na root'cie panelu turniejowego
        if (tournamentRoot == null) tournamentRoot = gameObject;

        // Ukryj tylko panel turniejowy
        if (hideOnAwake && tournamentRoot.activeSelf)
            tournamentRoot.SetActive(false);
    }

    /// Pokazuje panel turniejowy
    public void Show()
    {
        if (tournamentRoot != null && !tournamentRoot.activeSelf)
            tournamentRoot.SetActive(true);
    }

    /// Ukrywa panel turniejowy
    public void Hide()
    {
        if (tournamentRoot != null && tournamentRoot.activeSelf)
            tournamentRoot.SetActive(false);
    }

    private void Update()
    {
        // Jeœli panel jest ukryty, nie aktualizuj tekstów
        if (tournamentRoot == null || !tournamentRoot.activeInHierarchy) return;

        var tm = TrackManager.Instance;
        if (tm == null) return;

        float t = tm.RunElapsed;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int centis = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);

        if (TimeText) TimeText.text = string.Format(timeFormat, minutes, seconds, centis);
        if (ScoreText) ScoreText.text = string.Format(scoreFormat, tm.GetScore());

        int total = tm.obstacles != null ? tm.obstacles.Count : 0;
        int completed = tm.ClearedTotal;
        if (GoalText) GoalText.text = string.Format(clearedFormat, completed, total);
    }
}
