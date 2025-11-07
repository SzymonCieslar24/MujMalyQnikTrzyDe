using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * @class TileButton
 * @brief Pojedynczy kafelek; przekazuje klik do menad¿era i robi krótki „b³ysk”.
 */
public class TileButton : MonoBehaviour, IPointerClickHandler
{
    public int Index;
    public Image Image;
    public MemoryManager Game;

    private Color _baseColor;
    private Coroutine _pulseCo;

    private void Awake()
    {
        if (!Image) Image = GetComponent<Image>();
    }

    private void Start()
    {
        _baseColor = Image ? Image.color : Color.gray;
        if (!Game)
        {
#if UNITY_2023_1_OR_NEWER
            Game = Object.FindFirstObjectByType<MemoryManager>();
#else
            Game = FindObjectOfType<MemoryManager>();
#endif
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Game != null) Game.OnTileClicked(Index);
    }

    public void SetColor(Color c)
    {
        c.a = 1f;
        if (Image) Image.color = c;
        _baseColor = c;
    }

    public void Pulse(Color flashColor, float duration)
    {
        if (_pulseCo != null) StopCoroutine(_pulseCo);
        _pulseCo = StartCoroutine(CoPulse(flashColor, duration));
    }
    public void SetInteractable(bool value)
    {
        var btn = GetComponent<Button>();
        if (btn) btn.interactable = value;
    }

    private IEnumerator CoPulse(Color flashColor, float duration)
    {
        if (Image) Image.color = flashColor;
        yield return new WaitForSeconds(duration);
        if (Image) Image.color = _baseColor;
    }
}
