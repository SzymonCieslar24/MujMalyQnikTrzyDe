using UnityEngine;

/// <summary>
/// Skrypt odpowiedzialny za losowe rozmieszczanie prefab�w na okre�lonych pozycjach.
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    /// <summary>
    /// Lista prefab�w, kt�re mog� by� zrespawnowane.
    /// </summary>
    [Tooltip("Lista mo�liwych prefab�w do zrespawnowania.")]
    public GameObject[] prefabs;

    /// <summary>
    /// Lista pozycji, na kt�rych mog� pojawi� si� obiekty.
    /// </summary>
    [Tooltip("Pozycje, na kt�rych mog� zosta� rozmieszczone obiekty.")]
    public Transform[] spawnPoints;

    /// <summary>
    /// Flaga okre�laj�ca, czy spawn ma si� wykona� przy starcie gry.
    /// </summary>
    [Tooltip("Czy obiekty maj� by� spawn'owane automatycznie po uruchomieniu sceny?")]
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnObjects();
    }

    /// <summary>
    /// Funkcja odpowiedzialna za losowe rozmieszczenie prefab�w na okre�lonych pozycjach.
    /// </summary>
    public void SpawnObjects()
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Brak prefab�w lub punkt�w spawn'owania!");
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
