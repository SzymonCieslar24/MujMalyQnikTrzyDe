using StarterAssets;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pokazuje ekran tylko wtedy, gdy wciœniêty jest klawisz Tab (nowy Input System).
/// </summary>
public class TabMenuController : MonoBehaviour
{
    [SerializeField] private GameObject tabMenu;
    [SerializeField] private GameObject tournamentMenu;

    public TextMeshProUGUI StaminaText;
    public TextMeshProUGUI RegenText;
    public TextMeshProUGUI JumpText;
    public TextMeshProUGUI ImpulseText;

    [SerializeField] private ThirdPersonController player;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.tabKey.isPressed)
        {
            if (!tabMenu.activeSelf)
                tabMenu.SetActive(true);

            if (StaminaText)
                StaminaText.text = $"Wytrzyma³oœæ: {player.GetMaxStamina():0.0}";

            if (RegenText)
                RegenText.text = $"Regeneracja: {player.GetRegenSpeed():0.0}";

            if (JumpText)
                JumpText.text = $"Wysokoœæ skoku: {player.GetJumpHeight():0.0}";

            if (ImpulseText)
                ImpulseText.text = $"Zryw: {player.GetDistance():0.0}";


            if (tournamentMenu.activeSelf)
                tournamentMenu.SetActive(false);
        }

        else
        {
            if (tabMenu.activeSelf)
                tabMenu.SetActive(false);

            if (!tournamentMenu.activeSelf)
                tournamentMenu.SetActive(true);
        }
    }
}
