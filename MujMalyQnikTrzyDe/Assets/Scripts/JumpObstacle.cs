using UnityEngine;

public class JumpObstacle : ObstacleBase
{
    private GameObject player;

    private bool jumpedOver = false;

    private bool collided = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            Debug.LogWarning("Brak obiektu z tagiem 'Player' w scenie!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jumpedOver = true;
            Debug.Log($"Gracz przeskoczyl poprawnie przeszkode: {gameObject.name}");
        }
    }

    public void ReportCollision()
    {
        collided = true;
        Debug.Log($"Gracz uderzy³ w przeszkodê {name}");
    }


    //private void OnBecameInvisible()
    //{
    //    // Jeœli gracz min¹³ przeszkodê (kamera ju¿ jej nie widzi)
    //    if (!jumpedOver && !collided)
    //    {
    //        Debug.Log($"Gracz omin¹³ przeszkodê: {gameObject.name}");
    //    }
    //}
}

