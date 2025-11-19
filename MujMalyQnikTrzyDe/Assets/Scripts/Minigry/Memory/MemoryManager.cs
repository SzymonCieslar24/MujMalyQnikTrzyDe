using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class MemoryManager : MonoBehaviour
{
    public List<TileButton> Tiles = new List<TileButton>();
    public Color PlayColor = new Color(0.2f, 0.5f, 1f);
    public Color BaseColor = new Color(0.75f, 0.75f, 0.75f);
    public Color ShowColor = new Color(0.3f, 0.7f, 1f);
    public Color DisabledColor = new Color(0.55f, 0.55f, 0.55f);
    public Color ErrorColor = new Color(1f, 0.25f, 0.25f);
    public Color SuccessColor = new Color(0.2f, 0.9f, 0.3f);
    public float AllFlashTime = 0.6f;
    public float ErrorFlashTime = 0.5f;
    public float AfterErrorDelay = 0.25f;
    public float AfterSuccessDelay = 0.25f;
    public float ShowDelay = 0.6f;
    public float StepFlashTime = 0.35f;
    public float BetweenSteps = 0.25f;
    public int Level = 1;
    public int Lives = 3;
    public Text StatusText;

    public TMP_Text HudText;
    public Button BackButton;
    public string BackSceneName = "Padok";


    private List<int> _sequence = new List<int>();
    private int _inputPos = 0;
    private bool _isPlaying = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;  // Myszka nie jest zablokowana
        Cursor.visible = true;

        NewGame();

        if (BackButton != null)
            BackButton.onClick.AddListener(() => SceneManager.LoadScene(BackSceneName));

        UpdateHUD();
    }

    public void NewGame()
    {
        _sequence.Clear();
        Level = Mathf.Max(1, Level);
        Lives = Mathf.Max(1, Lives);
        UpdateHUD();
        GenerateNextStep();
        PlayCurrentSequence();
    }

    private void GenerateNextStep()
    {
        _sequence.Add(Random.Range(0, Tiles.Count));
    }

    public void PlayCurrentSequence()
    {
        StopAllCoroutines();
        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        _isPlaying = true;
        _inputPos = 0;

        // wy³¹cz przyciski i ustaw kolor nieaktywny
        foreach (var t in Tiles)
        {
            t.SetInteractable(false);
            t.SetColor(DisabledColor);
        }

        yield return new WaitForSeconds(ShowDelay);

        // pokaz sekwencji: b³ysk ShowColor -> powrót do DisabledColor
        for (int i = 0; i < _sequence.Count; i++)
        {
            int idx = _sequence[i];
            Tiles[idx].SetColor(ShowColor);
            yield return new WaitForSeconds(StepFlashTime);
            Tiles[idx].SetColor(DisabledColor);
            yield return new WaitForSeconds(BetweenSteps);
        }

        // koniec pokazu – przywróæ kolory i w³¹cz przyciski
        foreach (var t in Tiles)
        {
            t.SetColor(BaseColor);
            t.SetInteractable(true);
        }

        _isPlaying = false;
        UpdateHUD();

    }

    public void OnTileClicked(int index)
    {
        if (_isPlaying) return;

        if (index == _sequence[_inputPos])
        {
            Tiles[index].Pulse(PlayColor, StepFlashTime * 0.9f);
            _inputPos++;

            if (_inputPos >= _sequence.Count)
            {
                StartCoroutine(CoAdvanceLevel());
            }
        }
        else
        {
            StartCoroutine(CoHandleError(index));
        }
        UpdateHUD();
    }

    public void ResetTileColors()
    {
        foreach (var t in Tiles) t.SetColor(BaseColor);
    }

    private IEnumerator CoHandleError(int index)
    {
        _isPlaying = true;
        foreach (var t in Tiles) t.SetInteractable(false);

        // klikniêty kafel: czerwony
        Tiles[index].Pulse(ErrorColor, ErrorFlashTime);

        // JEŒLI to by³o ostatnie ¿ycie (przed odjêciem)
        if (Lives == 1)
        {
            // króciutko poczekaj, ¿eby widaæ by³o "Pulse" klikniêtego
            yield return new WaitForSeconds(ErrorFlashTime * 0.6f);

            // wszystkie na czerwono, po czym wróæ do koloru "nieaktywne"
            yield return StartCoroutine(FlashAll(ErrorColor, AllFlashTime, DisabledColor));
        }

        // teraz normalna logika b³êdu
        yield return new WaitForSeconds(Mathf.Max(0f, ErrorFlashTime + AfterErrorDelay - (ErrorFlashTime * 0.6f)));

        Lives--;
        UpdateHUD();

        if (Lives <= 0)
        {
            Level = 1;
            Lives = 3;
            _sequence.Clear();
            GenerateNextStep();
            UpdateHUD();
        }

        PlayCurrentSequence();
    }
    private IEnumerator CoAdvanceLevel()
    {
        _isPlaying = true;
        foreach (var t in Tiles) t.SetInteractable(false);

        Level++;
        UpdateHUD();

        // wszystkie na zielono, potem wróæ do bazowego
        yield return StartCoroutine(FlashAll(SuccessColor, AllFlashTime, BaseColor));

        yield return new WaitForSeconds(AfterSuccessDelay);

        GenerateNextStep();
        PlayCurrentSequence();
    }

    private void UpdateHUD()
    {
        if (HudText)
            HudText.text = $"Poziom: {Level}    ¯ycia: {Lives}";
    }

    private IEnumerator FlashAll(Color c, float time, Color afterColor)
    {
        foreach (var t in Tiles) { t.SetColor(c); }
        yield return new WaitForSeconds(time);
        foreach (var t in Tiles) { t.SetColor(afterColor); }
    }

}
