using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generator losowej trasy z dostêpnych przeszkód.
/// Losuje 10 unikalnych przeszkód spoœród oznaczonych tagiem "Obstacle".
/// </summary>
public class RandomTrackGenerator : MonoBehaviour
{
    [Tooltip("Iloœæ przeszkód w trasie.")]
    public int obstacleCount = 10;

    [Tooltip("Odstêp miêdzy przeszkodami (po osi Z).")]
    public float distanceBetween = 10f;

    [Tooltip("Punkt pocz¹tkowy generacji trasy.")]
    public Transform startPoint;

    [Tooltip("Czy wylosowane przeszkody maj¹ byæ instancjowane (tworzone kopie prefabów).")]
    public bool instantiateCopies = true;

    private List<GameObject> allObstacles = new();
    private List<GameObject> track = new();

    void Start()
    {
        GenerateTrack();
    }

    /// <summary>
    /// G³ówna funkcja generuj¹ca losow¹ trasê.
    /// </summary>
    public void GenerateTrack()
    {
        // ZnajdŸ wszystkie przeszkody w scenie (po tagu)
        allObstacles.Clear();
        allObstacles.AddRange(GameObject.FindGameObjectsWithTag("Obstacle"));

        if (allObstacles.Count == 0)
        {
            Debug.LogWarning("Brak przeszkód w scenie z tagiem 'Obstacle'.");
            return;
        }

        // Losowe wylosowanie unikalnych przeszkód
        List<GameObject> shuffled = new List<GameObject>(allObstacles);
        ShuffleList(shuffled);

        int count = Mathf.Min(obstacleCount, shuffled.Count);
        track = shuffled.GetRange(0, count);

        // Ustawienie trasy
        Vector3 spawnPosition = startPoint != null ? startPoint.position : Vector3.zero;

        for (int i = 0; i < track.Count; i++)
        {
            GameObject obstacle = track[i];

            if (instantiateCopies)
            {
                GameObject clone = Instantiate(obstacle, spawnPosition, Quaternion.identity);
                clone.name = $"{obstacle.name}_Clone_{i + 1}";
            }
            else
            {
                obstacle.transform.position = spawnPosition;
            }

            spawnPosition += Vector3.forward * distanceBetween;
        }

        Debug.Log($"Wygenerowano trasê z {count} przeszkód.");
    }

    /// <summary>
    /// Prosty algorytm tasowania listy (Fisher–Yates).
    /// </summary>
    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
