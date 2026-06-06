using UnityEngine;
using UnityEngine.UI;

public class LanternBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnLantern playerLantern;
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        FindLanternIfNeeded();
    }

    private void OnEnable()
    {
        FindLanternIfNeeded();

        if (playerLantern != null)
        {
            playerLantern.OnEnergyChanged += UpdateLanternBar;
            UpdateLanternBar(playerLantern.CurrentEnergy, playerLantern.MaxEnergy, playerLantern.EnergyPercent);
        }
    }

    private void OnDisable()
    {
        if (playerLantern != null)
        {
            playerLantern.OnEnergyChanged -= UpdateLanternBar;
        }
    }

    private void UpdateLanternBar(float currentEnergy, float maxEnergy, float energyPercent)
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = Mathf.Clamp01(energyPercent);
    }

    private void FindLanternIfNeeded()
    {
        if (playerLantern != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLantern = player.GetComponent<AnLantern>();
        }
    }
}
