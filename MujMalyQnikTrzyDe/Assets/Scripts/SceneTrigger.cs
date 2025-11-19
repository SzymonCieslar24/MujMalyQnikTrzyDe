using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    public string sceneName;  // Nazwa sceny, do której ma przejœæ

    // Metoda wykrywaj¹ca wejœcie postaci w trigger
    private void OnTriggerEnter(Collider other)
    {
        // Sprawdzamy, czy obiekt, który wchodzi w trigger, ma tag "Player"
        if (other.CompareTag("Player"))
        {
            // Wypisujemy nazwê sceny do konsoli
            Debug.Log("Wejœcie w trigger! Scena: " + sceneName);

            // Je¿eli tak, ³adujemy odpowiedni¹ scenê
            SceneManager.LoadScene(sceneName);
        }
    }
}
