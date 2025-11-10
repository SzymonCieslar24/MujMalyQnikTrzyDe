using UnityEngine;

public class ArrowOverCurrent : MonoBehaviour
{
    [Header("Pozycjonowanie")]
    public float heightOffset = 2.0f;
    public Vector3 extraOffset = Vector3.zero;
    public float followSmooth = 10f;

    [Header("Widocznoœæ")]
    public bool hideWhenNone = true;

    [Tooltip("Opcjonalnie: obiekt zawieraj¹cy grafikê strza³ki (child). " +
             "Jeœli puste – u¿yje obiektu bie¿¹cego.")]
    public GameObject visualRoot;

    private Camera cam;

    void Awake()
    {
        // Jeœli nie przypisano, u¿yj bie¿¹cego obiektu jako „wizualnego”
        if (visualRoot == null) visualRoot = gameObject;
    }

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();

        // Upewnij siê, ¿e komponent pozostaje aktywny – NIE wy³¹czamy ca³ego GameObject!
        // Na starcie ukryj tylko wizual (jeœli nie ma przeszkód).
        SetVisible(false);
    }

    void Update()
    {
        var tm = TrackManager.Instance;
        var current = tm != null ? tm.GetCurrentObstacle() : null;

        if (current == null)
        {
            if (hideWhenNone)
                SetVisible(false);   // Ukryj grafikê, ale nie wy³¹czaj ca³ego GO!
            return;
        }

        // Jest przeszkoda – poka¿ grafikê
        SetVisible(true);

        // Œledzenie pozycji
        Vector3 targetPos = current.transform.position + Vector3.up * heightOffset + extraOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);

        // Obrót w stronê kamery (poziomo)
        if (cam != null)
        {
            Vector3 dir = transform.position - cam.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot == null) return;

        // Jeœli visualRoot to ten sam obiekt co skrypt – nie wy³¹czamy ca³ego GO!
        // Zamiast tego w³¹cz/wy³¹cz renderery.
        var renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers) r.enabled = visible;

        var canvases = visualRoot.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases) c.enabled = visible;
    }
}
