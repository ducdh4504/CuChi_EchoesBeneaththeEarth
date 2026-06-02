using System;
using UnityEngine;

public class AnOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float currentOxygen = 100f;
    [SerializeField] private float oxygenDecreaseRate = 3f;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    public float OxygenPercent => maxOxygen <= 0f ? 0f : currentOxygen / maxOxygen;
    public bool IsFull => currentOxygen >= maxOxygen;

    public event Action<float, float, float> OnOxygenChanged;
    public event Action OnOutOfOxygen;

    private bool isOutOfOxygen;

    private void Awake()
    {
        maxOxygen = Mathf.Max(1f, maxOxygen);
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
        isOutOfOxygen = currentOxygen <= 0f;
    }

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
        NotifyOxygenChanged();

        if (currentOxygen <= 0f)
        {
            isOutOfOxygen = true;
            Debug.Log("An is out of oxygen!");
            OnOutOfOxygen?.Invoke();
        }
    }

    public void RestoreOxygen(float amount)
    {
        TryRestoreOxygen(amount);
    }

    public bool TryRestoreOxygen(float amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (IsFull)
        {
            Debug.Log("Oxygen is already full.");
            return false;
        }

        float oxygenBeforeRestore = currentOxygen;

        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

        if (currentOxygen > 0f)
        {
            isOutOfOxygen = false;
        }

        float restoredAmount = currentOxygen - oxygenBeforeRestore;
        Debug.Log($"Oxygen restored: {restoredAmount}");

        NotifyOxygenChanged();

        return restoredAmount > 0f;
    }

    private void NotifyOxygenChanged()
    {
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen, OxygenPercent);
    }
}
