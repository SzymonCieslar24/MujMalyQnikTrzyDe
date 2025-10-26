using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        JumpObstacle obstacle = hit.collider.GetComponentInParent<JumpObstacle>();
        if (obstacle != null)
        {
            obstacle.ReportCollision();
        }
    }
}
