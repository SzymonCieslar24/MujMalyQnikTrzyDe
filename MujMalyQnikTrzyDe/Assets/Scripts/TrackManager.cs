using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public static TrackManager Instance { get; private set; }

    public List<ObstacleBase> obstacles = new();

    private int currentIndex = 0;
    private int score = 0;

    public int ClearedSuccess { get; private set; } = 0;
    public int ClearedTotal { get; private set; } = 0;

    private bool isRunning = false;
    private float runStartTime = 0f;
    private float lastRunTime = 0f;

    private bool trackActivated = false;

    public float finalRunTime = 0f;

    [Header("UI zawodów (opcjonalne, zostanie znalezione automatycznie)")]
    [SerializeField] private EndMenuUI competitionEndUI;

    [Header("UI zawodów (opcjonalne, zostanie znalezione automatycznie)")]
    [SerializeField] private CompetitionUI competitionUI;

    public float RunElapsed =>
        isRunning ? (Time.time - runStartTime) : lastRunTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (competitionEndUI == null)
            competitionEndUI = FindObjectOfType<EndMenuUI>(true);

        if (competitionUI == null)
            competitionUI = FindObjectOfType<CompetitionUI>(true);

        ArmTrackForTrigger();
    }

    public void ArmTrackForTrigger()
    {
        trackActivated = false;
        isRunning = false;
        runStartTime = 0f;
        lastRunTime = 0f;

        if (obstacles != null)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i] != null)
                    obstacles[i].SetActiveState(false);
            }
        }
    }

    public void ActivateTrackAndStart(GameObject activator = null)
    {
        if (trackActivated) return;
        trackActivated = true;

        ActivateOnlyCurrent();

        if (obstacles.Count > 0 && !isRunning)
            StartRun();
    }

    public void RegisterObstacle(ObstacleBase obstacle)
    {
        if (!obstacles.Contains(obstacle))
            obstacles.Add(obstacle);

        if (!trackActivated)
        {
            if (obstacle != null)
                obstacle.SetActiveState(false);
            return;
        }

        ActivateOnlyCurrent();

        if (!isRunning && obstacles.Count > 0)
            StartRun();
    }

    public bool IsCurrentObstacle(ObstacleBase obstacle)
    {
        return obstacles.Count > 0 &&
               obstacles[Mathf.Clamp(currentIndex, 0, obstacles.Count - 1)] == obstacle;
    }

    public ObstacleBase GetCurrentObstacle()
    {
        if (obstacles == null || obstacles.Count == 0) return null;
        currentIndex = Mathf.Clamp(currentIndex, 0, obstacles.Count - 1);
        return obstacles[currentIndex];
    }

    public void AddScore(int value)
    {
        score += value;
        Debug.Log($"Punkty: {score}");
    }

    public void NotifyObstacleCompleted(bool success)
    {
        ClearedTotal++;
        if (success) ClearedSuccess++;

        // Jeœli ostatnia przeszkoda zosta³a ukoñczona
        if (currentIndex >= obstacles.Count - 1)
        {
            StopRun();  // Zatrzymanie biegu
            finalRunTime = RunElapsed;  // Zapisz czas zakoñczenia

            if (competitionUI != null)
                competitionUI.Hide();

            if (competitionEndUI != null)
            {
                Debug.Log("Showing competition end UI");
                competitionEndUI.Show();  // Wyœwietlenie UI po zakoñczeniu
            }
            else
            {
                Debug.LogError("competitionEndUI is not assigned in TrackManager!");
            }
        }
    }

    public void AdvanceToNextObstacle()
    {
        var current = GetCurrentObstacle();
        if (current != null) current.SetActiveState(false);

        currentIndex = Mathf.Min(currentIndex + 1, Mathf.Max(obstacles.Count - 1, 0));

        ActivateOnlyCurrent();

        if (!isRunning && obstacles.Count > 0) StartRun();
    }

    public int GetCurrentIndex() => currentIndex;
    public int GetScore() => score;

    private void ActivateOnlyCurrent()
    {
        if (obstacles == null || obstacles.Count == 0) return;

        if (!trackActivated) return;

        for (int i = 0; i < obstacles.Count; i++)
        {
            bool shouldBeActive = (i == Mathf.Clamp(currentIndex, 0, obstacles.Count - 1));
            if (obstacles[i] != null)
                obstacles[i].SetActiveState(shouldBeActive);
        }
    }

    public void ClearObstaclesList()
    {
        obstacles?.Clear();
        currentIndex = 0;
    }


    public void StartRun()
    {
        isRunning = true;
        runStartTime = Time.time;
        lastRunTime = 0f;
    }

    public void StopRun()
    {
        if (!isRunning) return;

        isRunning = false;
        lastRunTime = Time.time - runStartTime;
    }

    public void ResetRun()
    {
        score = 0;
        ClearedSuccess = 0;
        ClearedTotal = 0;
        currentIndex = 0;
        isRunning = false;
        runStartTime = 0f;
        lastRunTime = 0f;
        finalRunTime = 0f;

        ArmTrackForTrigger();
    }
}
