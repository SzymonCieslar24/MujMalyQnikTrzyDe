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
        Debug.Log($"Gracz uderzy� w przeszkod� {name}");
    }


    //private void OnBecameInvisible()
    //{
    //    // Je�li gracz min�� przeszkod� (kamera ju� jej nie widzi)
    //    if (!jumpedOver && !collided)
    //    {
    //        Debug.Log($"Gracz omin�� przeszkod�: {gameObject.name}");
    //    }
    //}
}

