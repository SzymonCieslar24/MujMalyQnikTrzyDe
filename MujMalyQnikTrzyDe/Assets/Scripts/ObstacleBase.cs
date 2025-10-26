using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    [Tooltip("Liczba punktów za poprawne zaliczenie.")]
    public int points = 10;

    protected bool completed = false;
    protected bool isActive = false;

    private Collider[] _colliders;
    private Behaviour[] _optionalBehaviours;

    protected virtual void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(includeInactive: true);
        _optionalBehaviours = GetComponentsInChildren<Behaviour>(includeInactive: true);
    }

    protected void RegisterSelf()
    {
        if (TrackManager.Instance != null)
            TrackManager.Instance.RegisterObstacle(this);
    }

    public virtual void SetActiveState(bool active)
    {
        isActive = active;

        if (_colliders != null)
        {
            foreach (var c in _colliders)
                if (c != null) c.enabled = active;
        }

        if (_optionalBehaviours != null)
        {
            foreach (var b in _optionalBehaviours)
            {
                if (b == this) continue;
                if (b is Renderer) continue;
                b.enabled = active;
            }
        }
    }

    protected void CompleteSuccess()
    {
        if (!CanComplete()) return;

        completed = true;
        TrackManager.Instance.AddScore(points);
        TrackManager.Instance.NotifyObstacleCompleted(true);
        Debug.Log($"Zaliczone: +{points} za {name}");

        TrackManager.Instance.AdvanceToNextObstacle();
    }

    protected void CompleteFail()
    {
        if (!CanComplete()) return;

        completed = true;
        TrackManager.Instance.NotifyObstacleCompleted(false);
        Debug.Log($"Niezaliczone: {name}");

        TrackManager.Instance.AdvanceToNextObstacle();
    }

    private bool CanComplete()
    {
        if (completed) return false;
        if (TrackManager.Instance == null) return false;
        if (!TrackManager.Instance.IsCurrentObstacle(this)) return false;
        return true;
    }
}
