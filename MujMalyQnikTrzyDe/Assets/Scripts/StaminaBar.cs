using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private ThirdPersonController player;
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    private void Update()
    {
        if (player != null && slider != null)
            slider.value = player.GetStamina01();
    }
}
