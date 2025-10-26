using UnityEngine;

public class ObstaclesSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    public Transform[] spawnPoints;

    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnObjects();
    }

    public void SpawnObjects()
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Brak prefabów lub punktów spawn'owania!");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];

            Instantiate(randomPrefab, point.position, point.rotation);
        }
    }
}
