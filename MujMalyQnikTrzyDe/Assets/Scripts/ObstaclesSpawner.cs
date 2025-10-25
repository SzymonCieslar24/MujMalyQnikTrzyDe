using UnityEngine;

/// <summary>
/// Skrypt odpowiedzialny za losowe rozmieszczanie prefabów na okreœlonych pozycjach.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    /// <summary>
    /// Lista prefabów, które mog¹ byæ zrespawnowane.
    /// </summary>
    [Tooltip("Lista mo¿liwych prefabów do zrespawnowania.")]
    public GameObject[] prefabs;

    /// <summary>
    /// Lista pozycji, na których mog¹ pojawiæ siê obiekty.
    /// </summary>
    [Tooltip("Pozycje, na których mog¹ zostaæ rozmieszczone obiekty.")]
    public Transform[] spawnPoints;

    /// <summary>
    /// Flaga okreœlaj¹ca, czy spawn ma siê wykonaæ przy starcie gry.
    /// </summary>
    [Tooltip("Czy obiekty maj¹ byæ spawn'owane automatycznie po uruchomieniu sceny?")]
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnObjects();
    }

    /// <summary>
    /// Funkcja odpowiedzialna za losowe rozmieszczenie prefabów na okreœlonych pozycjach.
    /// </summary>
    public void SpawnObjects()
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Brak prefabów lub punktów spawn'owania!");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            // Losowy prefab z listy
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];

            // Tworzenie instancji w danym punkcie
            Instantiate(randomPrefab, point.position, point.rotation);
        }
    }
}
