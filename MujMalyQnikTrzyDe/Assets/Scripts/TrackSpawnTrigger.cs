using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrackSpawnTrigger : MonoBehaviour
{
    [Tooltip("Tag gracza, który ma aktywowaæ tor.")]
    public string playerTag = "Player";

    [Tooltip("Czy przy ka¿dym wejœciu czyœcimy i generujemy nowy tor.")]
    public bool resetOnEveryEnter = true;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");

        if (!other.CompareTag(playerTag)) return;

        // 1) zatrzymaj ewentualny poprzedni bieg
        if (TrackManager.Instance != null)
            TrackManager.Instance.StopRun();

        // 2) usuñ stare przeszkody i wyczyœæ listê w managerze
        if (ObstaclesSpawner.Instance != null)
        {
            if (resetOnEveryEnter)
            {
                ObstaclesSpawner.Instance.ClearSpawned();
                if (TrackManager.Instance != null)
                    TrackManager.Instance.ClearObstaclesList();
            }

            // 3) zespawnuj nowe
            ObstaclesSpawner.Instance.SpawnObjects();
        }
        else
        {
            Debug.LogWarning("TrackSpawnTrigger: Brak ObstaclesSpawner.Instance w scenie.");
        }

        // 4) wyzeruj wynik/czas/liczniki i uzbrój bieg od pocz¹tku
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.ResetRun();                  // zeruje score/time/index
            TrackManager.Instance.ActivateTrackAndStart(other.gameObject); // start nad 1. przeszkod¹
        }
        else
        {
            Debug.LogWarning("TrackSpawnTrigger: Brak TrackManager.Instance w scenie.");
        }

        // Uwaga: nie wy³¹czamy triggera — ma dzia³aæ ponownie przy nastêpnym wejœciu
    }
}
