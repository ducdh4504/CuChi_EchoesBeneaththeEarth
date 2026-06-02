using UnityEngine;
using UnityEngine.UI;

public class OxygenBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnOxygen anOxygen;
    [SerializeField] private Image oxygenFillImage;

    private void Update()
    {
        if (anOxygen == null || oxygenFillImage == null)
        {
            return;
        }

        oxygenFillImage.fillAmount = anOxygen.OxygenPercent;
    }
}