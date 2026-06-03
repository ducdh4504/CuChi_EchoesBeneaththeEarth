using UnityEngine;
using UnityEngine.UI;

public class LanternBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnLantern playerLantern;
    [SerializeField] private Image fillImage;

    private void Update()
    {
        UpdateLanternBar();
    }

    private void UpdateLanternBar()
    {
        if (playerLantern == null || fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = playerLantern.EnergyPercent;
    }
}