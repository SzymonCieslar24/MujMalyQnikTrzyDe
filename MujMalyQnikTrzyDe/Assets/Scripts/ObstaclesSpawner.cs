using System.Collections.Generic;
using UnityEngine;

public class ObstaclesSpawner : MonoBehaviour
{
    public static ObstaclesSpawner Instance { get; private set; }

    public GameObject[] prefabs;
    public Transform[] spawnPoints;

    // trzymamy referencje, aby móc je ³atwo usun¹æ
    private readonly List<GameObject> spawned = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ClearSpawned()
    {
        // usuñ wszystkie poprzednie przeszkody
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();
    }

    public void SpawnObjects()
    {
        if (prefabs == null || prefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Brak prefabów lub punktów spawn'owania!");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) continue;

            var go = Instantiate(prefab, point.position, point.rotation, transform); // parent dla porz¹dku
            spawned.Add(go);
        }

        Debug.Log($"ObstaclesSpawner: wygenerowano {spawnPoints.Length} obiektów.");
    }

    public void Respawn()
    {
        ClearSpawned();
        SpawnObjects();
    }
}
