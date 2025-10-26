using UnityEngine;

public class JumpObstacle : ObstacleBase
{
    private GameObject player;
    private bool collided = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        RegisterSelf();
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogWarning("Brak obiektu z tagiem 'Player' w scenie!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        CompleteSuccess();
        Debug.Log($"Gracz przeskoczy³ poprawnie przeszkodê: {name}");
    }

    public void ReportCollision()
    {
        if (!isActive) return;
        if (collided) return;

        collided = true;
        CompleteFail();
        Debug.Log($"Gracz uderzy³ w przeszkodê: {name}");
    }
}


