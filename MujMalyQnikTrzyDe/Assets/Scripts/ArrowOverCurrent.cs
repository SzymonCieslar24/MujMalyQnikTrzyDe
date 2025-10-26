using UnityEngine;

public class ArrowOverCurrent : MonoBehaviour
{
    public float heightOffset = 2.0f;

    public Vector3 extraOffset = Vector3.zero;

    public float followSmooth = 10f;

    public bool hideWhenNone = true;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();
    }

    void Update()
    {
        var tm = TrackManager.Instance;
        var current = tm != null ? tm.GetCurrentObstacle() : null;

        if (current == null)
        {
            if (hideWhenNone && gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Vector3 targetPos = current.transform.position + Vector3.up * heightOffset + extraOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);

        if (cam != null)
        {
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }
}
