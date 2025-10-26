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
    }

    private void Start()
    {
        ActivateOnlyCurrent();
        if (obstacles.Count > 0 && !isRunning) StartRun();
    }

    public void RegisterObstacle(ObstacleBase obstacle)
    {
        if (!obstacles.Contains(obstacle))
            obstacles.Add(obstacle);

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
        Debug.Log($"Wynik: {score}");
    }

    public void NotifyObstacleCompleted(bool success)
    {
        ClearedTotal++;
        if (success) ClearedSuccess++;

        if (currentIndex >= obstacles.Count - 1)
        {
            StopRun();
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
        if (obstacles == null) return;

        for (int i = 0; i < obstacles.Count; i++)
        {
            bool shouldBeActive = (i == Mathf.Clamp(currentIndex, 0, obstacles.Count - 1));
            if (obstacles[i] != null)
                obstacles[i].SetActiveState(shouldBeActive);
        }
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

        ActivateOnlyCurrent();
    }
}
