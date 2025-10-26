using UnityEngine;
using TMPro;

public class CompetitionUI : MonoBehaviour
{
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI GoalText;

    public string timeFormat = "{0:00}:{1:00}.{2:00}";

    public string scoreFormat = "{0}";

    public string clearedFormat = "{0}/{1}";

    void Update()
    {
        var tm = TrackManager.Instance;
        if (tm == null) return;

        float t = tm.RunElapsed;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        int centis = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);

        if (TimeText)
            TimeText.text = string.Format(timeFormat, minutes, seconds, centis);

        if (ScoreText)
            ScoreText.text = string.Format(scoreFormat, tm.GetScore());

        int total = tm.obstacles != null ? tm.obstacles.Count : 0;
        int completed = tm.ClearedTotal;
        if (GoalText)
            GoalText.text = string.Format(clearedFormat, completed, total);
    }
}

