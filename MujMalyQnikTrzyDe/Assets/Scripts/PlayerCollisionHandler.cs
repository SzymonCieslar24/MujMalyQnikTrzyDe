using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Sprawd�, czy uderzyli�my w przeszkod�
        JumpObstacle obstacle = hit.collider.GetComponentInParent<JumpObstacle>();
        if (obstacle != null)
        {
            obstacle.ReportCollision();
        }
    }
}
