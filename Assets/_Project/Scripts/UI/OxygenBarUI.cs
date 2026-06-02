using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image oxygenFillImage;
    [SerializeField] private TMP_Text oxygenText;

    [Header("Warning Settings")]
    [SerializeField] private float lowOxygenThreshold = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.2f);

    public void UpdateOxygenBar(float currentOxygen, float maxOxygen, float oxygenPercent)
    {
        oxygenPercent = Mathf.Clamp01(oxygenPercent);

        if (oxygenFillImage != null)
        {
            oxygenFillImage.fillAmount = oxygenPercent;
            oxygenFillImage.color = oxygenPercent <= lowOxygenThreshold ? warningColor : normalColor;
        }

        if (oxygenText != null)
        {
            int percent = Mathf.RoundToInt(oxygenPercent * 100f);
            oxygenText.text = $"OXYGEN {percent}%";
        }
    }
}
