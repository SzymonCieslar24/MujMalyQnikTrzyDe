using UnityEngine;
using UnityEngine.UI;

/**
 * @class FallingTile
 * @brief Pojedynczy spadający kafel: ruch, wejście/wyjście ze strefy trafienia, trafienie/miss.
 */
public class FallingTile : MonoBehaviour
{
    public Image Img;                   ///< Kolor kafelka (na start czarny)
    public Color HitColor = new Color(0.2f, 0.85f, 0.3f);
    public Color MissColor = new Color(1.0f, 0.3f, 0.3f);

    // runtime
    private PianoTileMenager _mgr;
    private RectTransform _rt;
    private RectTransform _hitZone;
    private float _speed;
    private bool _wasHit = false;
    private bool _countedMiss = false;

    // kontrola „klikowalności” i limitu 3 w strefie
    private bool _isInZone = false;     // czy aktualnie geometrycznie przecina strefę
    private bool _isClickable = false;  // czy liczy się jako jeden z „max 3” w strefie

    public int ColumnIndex { get; private set; } // kto spawnował, nieobowiązkowe

    public void Init(PianoTileMenager mgr, float speed, RectTransform hitZone, int columnIdx = -1)
    {
        _mgr = mgr;
        _speed = speed;
        _hitZone = hitZone;
        ColumnIndex = columnIdx;
        _rt = GetComponent<RectTransform>();
        if (!Img) Img = GetComponent<Image>();
        if (Img) { var c = Color.black; c.a = 1f; Img.color = c; }
    }

    private void Update()
    {
        // spadanie
        _rt.anchoredPosition += Vector2.down * (_speed * Time.deltaTime);

        // ✅ kasuj dopiero gdy kafel wyszedł DOŁEM poza GameArea
        var a = GetWorldRect(_rt);
        var area = GetWorldRect(_mgr.GameArea);

        // jeśli górna krawędź kafla jest poniżej dolnej krawędzi pola gry → poza ekranem dołem
        if (a.yMax < area.yMin)
        {
            if (_isInZone && !_wasHit && !_countedMiss)
            {
                CountMiss(); // policz miss, jeśli był w strefie i nie trafiony
            }
            _mgr.RegisterMiss(this); // sprzątnij kafel
            return;
        }
        // UWAGA: jeśli kafel jest nad polem (a.yMin > area.yMax) – NIC nie rób, nie kasujemy


        // wykrywanie wejścia/wyjścia w strefę trafienia
        bool nowInZone = RectOverlaps(_rt, _hitZone);
        if (nowInZone && !_isInZone)
        {
            // ENTER
            _isInZone = true;
            if (_mgr.TryAcquireZoneSlot(this))
            {
                _isClickable = true;
                // opcjonalnie: lekkie przyciemnienie/rozjaśnienie
            }
            else
            {
                // brak slotu (więcej niż 3) → nieklikany; jeśli opuści strefę, policzymy miss
                _isClickable = false;
            }
        }
        else if (!nowInZone && _isInZone)
        {
            // EXIT
            _isInZone = false;
            if (_isClickable && !_wasHit && !_countedMiss)
            {
                // był klikowalny, ale gracz nie trafił → miss + czerwony
                CountMiss();
            }
            if (_isClickable)
            {
                _mgr.ReleaseZoneSlot(this);
                _isClickable = false;
            }
        }
    }

    /// <summary> Próba trafienia – wywołuje manager, gdy naciśnięto klawisz kolumny. </summary>
    public bool TryHit()
    {
        if (_wasHit || !_isClickable) return false;

        _wasHit = true;
        if (Img) Img.color = HitColor;

        // zwolnij slot w strefie, żeby kolejny kafel mógł być klikalny
        if (_isClickable)
        {
            _mgr.ReleaseZoneSlot(this);
            _isClickable = false;
        }

        _mgr.RegisterHit(this);
        return true;
    }

    private void CountMiss()
    {
        _countedMiss = true;
        if (Img) Img.color = MissColor; // zostaje czerwony do końca spadania
        _mgr.RegisterMissStatOnly();    // tylko statystykę, kafel zniszczy manager gdy wyjdzie z GameArea
    }

    // prosta kolizja prostokątów w lokalnej przestrzeni GameArea
    private static bool RectOverlaps(RectTransform a, RectTransform b)
    {
        var aWorld = GetWorldRect(a);
        var bWorld = GetWorldRect(b);
        return aWorld.Overlaps(bWorld);
    }

    private static Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // min x/y, max x/y z 4 rogów
        float minX = corners[0].x, minY = corners[0].y, maxX = corners[2].x, maxY = corners[2].y;
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}
