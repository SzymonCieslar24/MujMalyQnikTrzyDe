using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/**
 * @class PianoTileMenager
 * @brief Minigra w stylu "Piano Tiles" na 5 kolumn (Q,W,E,R,T).
 */
public class PianoTileMenager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text LevelText;
    public TMP_Text TimeText;
    public TMP_Text StatsText;
    public Button ResetButton;
    public Button BackButton;
    public string BackSceneName = "Padok";

    [Header("Pole gry")]
    public RectTransform GameArea;
    public RectTransform HitZone;
    public RectTransform[] Columns = new RectTransform[5];   ///< Kolumny Q,W,E,R,T
    public GameObject TilePrefab;

    [Header("Parametry gry")]
    public float BaseSpeed = 400f;            ///< px/s
    public float SpeedFactorPerLevel = 1.15f; ///< mnożnik prędkości spadania na poziom
    public float MinSpawnDelay = 0.25f;       ///< bazowe min opóźnienie spawnu
    public float MaxSpawnDelay = 0.60f;       ///< bazowe max opóźnienie spawnu

    [Header("Progresja poziomów (czas + próg)")]
    public int StartLevel = 1;
    public float BaseMinTime = 12f;           ///< poziom 1: min czasu
    public float BaseMaxTime = 20f;           ///< poziom 1: max czasu
    public float TimeIncreasePerLevel = 1f;   ///< +1s min/max na poziom
    public float BaseTargetAccuracy = 0.60f;  ///< poziom 1: 60%
    public float AccuracyIncreasePerLevel = 0.05f; ///< +5% na poziom
    public int AccuracyCapLevel = 7;          ///< od tego poziomu próg = 90% stały
    public float AccuracyCap = 0.90f;         ///< maks. 90%

    [Header("Przyspieszanie spawnu od poziomu X")]
    public int SpawnAccelStartLevel = 8;      ///< od 8 poziomu przyspieszamy spawn
    public float SpawnDelayFactor = 0.90f;    ///< ×0.90 opóźnień / poziom (10% szybciej)

    [Header("Kolorowanie kolumn po rundzie (opcjonalnie)")]
    public Image[] ColumnHighlights = new Image[5]; // półprzezroczyste obrazy na kolumnach
    public Color SuccessColumnColor = new Color(0.2f, 0.85f, 0.3f, 1f);
    public Color FailColumnColor = new Color(1.0f, 0.2f, 0.2f, 1f);
    public Color NeutralColumnColor = new Color(1f, 1f, 1f, 1f); // delikatna biel

    // --- wewnętrzne ---
    private int _level = 1;
    private float _speed;
    private float _roundTime;
    private float _timeLeft;
    private float _requiredAccuracy;          // próg (0..1) dla bieżącego poziomu
    private bool _roundRunning = false;       // runda wystartowała (po pierwszym kliku)?
    private bool _spawning = false;           // czy jeszcze generujemy kafle?
    private bool _finishing = false;          // czas minął – czekamy aż znikną kafle

    private int _hitCount = 0;                // trafione
    private int _totalTiles = 0;              // wygenerowane

    private readonly List<FallingTile> _activeTiles = new List<FallingTile>();

    // limit 3 kafli w strefie trafienia jednocześnie
    private int _zoneSlots = 0;

    // efektywne opóźnienia spawnu (po przyspieszaniu od lvl 8)
    private float _effMinSpawnDelay, _effMaxSpawnDelay;

    // auto-porazka po N pominięciach z rzędu
    private int _missStreak = 0;
    public int AutoFailMissStreak = 10;
    private bool _autoFail = false;

    // -------------------- API dla FallingTile (limit strefy, statystyki) --------------------

    public bool TryAcquireZoneSlot(FallingTile t)
    {
        if (_zoneSlots >= 3) return false;
        _zoneSlots++;
        return true;
    }

    public void ReleaseZoneSlot(FallingTile t)
    {
        _zoneSlots = Mathf.Max(0, _zoneSlots - 1);
    }

    /// <summary> Zlicza „miss” (kafel opuścił strefę bez trafienia) – liczy do serii porażek. </summary>
    public void RegisterMissStatOnly()
    {
        _missStreak++;
        if (_missStreak >= AutoFailMissStreak && !_autoFail)
        {
            // wymuś przejście w FINISHING i auto-fail
            _autoFail = true;
            _spawning = false;
            _finishing = true;
            // jeśli nie ma już kafli – domknij od razu
            if (_activeTiles.Count == 0)
                EndRound();
        }
    }

    public void RegisterHit(FallingTile tile)
    {
        _hitCount++;
        _missStreak = 0; // reset serii porażek
        RemoveTile(tile);
    }

    public void RegisterMiss(FallingTile tile)
    {
        RemoveTile(tile);
    }

    private void RemoveTile(FallingTile tile)
    {
        _activeTiles.Remove(tile);
        if (tile != null) Destroy(tile.gameObject);

        // jeśli jesteśmy w fazie kończenia i to był ostatni kafel – domknij rundę
        if (_finishing && _activeTiles.Count == 0)
        {
            EndRound();
        }
    }

    // ------------------------------------ Lifecycle ------------------------------------

    private void Start()
    {
        if (BackButton) BackButton.onClick.AddListener(() => SceneManager.LoadScene(BackSceneName));
        if (ResetButton) ResetButton.onClick.AddListener(ResetLevel);

        _level = Mathf.Max(1, StartLevel);
        StartRound();
    }

    private void StartRound()
    {
        // prędkość spadania z poziomu
        _speed = BaseSpeed * Mathf.Pow(SpeedFactorPerLevel, _level - 1);
        SetColumnHighlights(NeutralColumnColor);

        // czas rundy z progresją poziomu
        float minTime = BaseMinTime + (_level - 1) * TimeIncreasePerLevel;
        float maxTime = BaseMaxTime + (_level - 1) * TimeIncreasePerLevel;
        _roundTime = Random.Range(minTime, maxTime);
        _timeLeft = _roundTime;

        // próg trafień: dokładnie 90% od AccuracyCapLevel w górę
        if (_level >= AccuracyCapLevel)
            _requiredAccuracy = AccuracyCap;                 // dokładnie 0.90
        else
            _requiredAccuracy = BaseTargetAccuracy + (_level - 1) * AccuracyIncreasePerLevel;

        // opóźnienia spawnu (przyspieszanie od lvl 8)
        int accelSteps = Mathf.Max(0, _level - SpawnAccelStartLevel + 1); // lvl8→1, lvl9→2...
        float spawnFactor = Mathf.Pow(SpawnDelayFactor, accelSteps);
        _effMinSpawnDelay = MinSpawnDelay * spawnFactor;
        _effMaxSpawnDelay = MaxSpawnDelay * spawnFactor;

        _hitCount = 0;
        _totalTiles = 0;
        _missStreak = 0;
        _autoFail = false;

        ClearActiveTiles();
        UpdateHUD();

        _roundRunning = false; // start dopiero po 1 kliku
        _spawning = false;
        _finishing = false;
    }

    private void Update()
    {
        // czekamy na pierwszy klawisz
        if (!_roundRunning)
        {
            if (PianoInput.AnyKeyPressed())
            {
                _roundRunning = true;
                _spawning = true;
                StartCoroutine(SpawnLoop());
            }
            return;
        }

        // input Q/W/E/R/T (działa także w fazie finishing)
        int colPressed = PianoInput.GetPressedColumn();
        if (colPressed >= 0)
        {
            FallingTile candidate = null;
            float bestY = float.PositiveInfinity;
            for (int i = 0; i < _activeTiles.Count; i++)
            {
                var t = _activeTiles[i];
                if (t == null || t.ColumnIndex != colPressed) continue;
                float y = t.GetComponent<RectTransform>().anchoredPosition.y;
                if (y < bestY) { bestY = y; candidate = t; }
            }
            if (candidate != null) candidate.TryHit();
        }

        // czas i przełączanie stanów
        if (!_finishing)
        {
            _timeLeft -= Time.deltaTime;

            // ostatnie 2 sekundy – przestań generować nowe kafle
            if (_timeLeft <= 2f) _spawning = false;

            // po wybiciu czasu – nie kończ od razu; wejdź w finishing
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _finishing = true;
            }
        }

        UpdateHUD();

        // jeśli kończymy i na ekranie nie ma kafli – domknij rundę
        if (_finishing && _activeTiles.Count == 0)
        {
            EndRound();
        }
    }

    // ------------------------------------ Spawn ------------------------------------

    private IEnumerator SpawnLoop()
    {
        while (_roundRunning && _spawning)
        {
            if (!_spawning) yield break;
            if (_timeLeft <= 2f) yield break; // bezpieczeństwo

            SpawnTile();

            float delay = Random.Range(_effMinSpawnDelay, _effMaxSpawnDelay);
            while (delay > 0f && _roundRunning && _spawning)
            {
                delay -= Time.deltaTime;
                if (_timeLeft <= 2f) yield break;
                yield return null;
            }
        }
    }

    private void SpawnTile()
    {
        int col = Random.Range(0, Columns.Length);
        var parent = Columns[col];

        var tileObj = Instantiate(TilePrefab, parent);
        var tile = tileObj.GetComponent<FallingTile>();
        tile.Init(this, _speed, HitZone, col);

        // start nad górną krawędzią GameArea
        var rt = tile.GetComponent<RectTransform>();
        float startY = GameArea.rect.height * 0.5f + rt.rect.height;
        rt.anchoredPosition = new Vector2(0f, startY);

        _activeTiles.Add(tile);
        _totalTiles++;
    }

    // ------------------------------------ Koniec rundy / HUD / Reset ------------------------------------

    private void EndRound()
    {
        _roundRunning = false;
        _spawning = false;

        float acc = (_totalTiles > 0) ? (float)_hitCount / _totalTiles : 0f;

        bool success = !_autoFail && (acc >= _requiredAccuracy);

        if (success)
        {
            StatsText.text = $"SUKCES ({acc * 100f:0}%)  • Cel: {Mathf.RoundToInt(_requiredAccuracy * 100f)}%";
            _level++; // sukces → poziom w górę
            SetColumnHighlights(SuccessColumnColor);
        }
        else
        {
            StatsText.text = $"PORAŻKA ({acc * 100f:0}%)  • Cel: {Mathf.RoundToInt(_requiredAccuracy * 100f)}%";
            SetColumnHighlights(FailColumnColor);
            // porażka: zostajemy na tym samym poziomie
        }

        StopAllCoroutines();
        Invoke(nameof(StartRound), 2f);
    }

    private void ClearActiveTiles()
    {
        foreach (var t in _activeTiles) if (t) Destroy(t.gameObject);
        _activeTiles.Clear();
        _zoneSlots = 0;
    }

    private void ResetLevel()
    {
        _level = 1;
        StartRound();
    }

    private void UpdateHUD()
    {
        if (LevelText) LevelText.text = $"Poziom: {_level}\nCel: {Mathf.RoundToInt(_requiredAccuracy * 100f)}%";
        if (TimeText) TimeText.text = $"Czas: {_timeLeft:0.0}s";
        if (StatsText) StatsText.text = $"Trafione:\n{_hitCount} / {_totalTiles}  ({(_totalTiles > 0 ? (float)_hitCount / _totalTiles * 100f : 0):0}%)";
    }

    private void SetColumnHighlights(Color c)
    {
        if (ColumnHighlights == null) return;
        for (int i = 0; i < ColumnHighlights.Length; i++)
        {
            if (ColumnHighlights[i] == null) continue;
            ColumnHighlights[i].color = c;
        }
    }
}
