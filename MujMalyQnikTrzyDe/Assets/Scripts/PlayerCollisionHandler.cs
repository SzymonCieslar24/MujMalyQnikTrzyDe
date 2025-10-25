using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // SprawdŸ, czy uderzyliœmy w przeszkodê
        JumpObstacle obstacle = hit.collider.GetComponentInParent<JumpObstacle>();
        if (obstacle != null)
        {
            obstacle.ReportCollision();
        }
    }
}
