using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generator losowej trasy z dost�pnych przeszk�d.
/// Losuje 10 unikalnych przeszk�d spo�r�d oznaczonych tagiem "Obstacle".
/// </summary>
public class RandomTrackGenerator : MonoBehaviour
{
    [Tooltip("Ilo�� przeszk�d w trasie.")]
    public int obstacleCount = 10;

    [Tooltip("Odst�p mi�dzy przeszkodami (po osi Z).")]
    public float distanceBetween = 10f;

    [Tooltip("Punkt pocz�tkowy generacji trasy.")]
    public Transform startPoint;

    [Tooltip("Czy wylosowane przeszkody maj� by� instancjowane (tworzone kopie prefab�w).")]
    public bool instantiateCopies = true;

    private List<GameObject> allObstacles = new();
    private List<GameObject> track = new();

    void Start()
    {
        GenerateTrack();
    }

    /// <summary>
    /// G��wna funkcja generuj�ca losow� tras�.
    /// </summary>
    public void GenerateTrack()
    {
        // Znajd� wszystkie przeszkody w scenie (po tagu)
        allObstacles.Clear();
        allObstacles.AddRange(GameObject.FindGameObjectsWithTag("Obstacle"));

        if (allObstacles.Count == 0)
        {
            Debug.LogWarning("Brak przeszk�d w scenie z tagiem 'Obstacle'.");
            return;
        }

        // Losowe wylosowanie unikalnych przeszk�d
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

        Debug.Log($"Wygenerowano tras� z {count} przeszk�d.");
    }

    /// <summary>
    /// Prosty algorytm tasowania listy (Fisher�Yates).
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
