using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlalomGate : MonoBehaviour
{
    public enum GateSide { Left, Right }

    public int Order = 0;

    public GateSide Side = GateSide.Left;

    private SlalomObstacle parent;
    private bool alreadyTriggered;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    public void AttachParent(SlalomObstacle slalom)
    {
        parent = slalom;
    }

    public void ResetGate()
    {
        alreadyTriggered = false;
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;
        if (parent == null) return;

        alreadyTriggered = true;
        parent.ReportGateEntered(this, other);
    }
}
