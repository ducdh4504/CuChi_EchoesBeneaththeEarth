using UnityEngine;

public class AnOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float currentOxygen = 100f;
    [SerializeField] private float oxygenDecreaseRate = 3f;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    public float OxygenPercent => currentOxygen / maxOxygen;

    private bool isOutOfOxygen;

    private void Update()
    {
        DecreaseOxygenOverTime();
    }

    private void DecreaseOxygenOverTime()
    {
        if (isOutOfOxygen)
        {
            return;
        }

        currentOxygen -= oxygenDecreaseRate * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

        if (currentOxygen <= 0f)
        {
            isOutOfOxygen = true;
            Debug.Log("An is out of oxygen!");
        }
    }

    public void RestoreOxygen(float amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

        if (currentOxygen > 0f)
        {
            isOutOfOxygen = false;
        }

        Debug.Log($"Oxygen restored: {amount}");
    }
}
