using System;
using UnityEngine;

public class AnLantern : MonoBehaviour
{
    [Header("Lantern Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float drainRate = 3f;

    [Header("Lantern Light")]
    [SerializeField] private Light lanternLight;
    [SerializeField] private float maxIntensity = 2.2f;
    [SerializeField] private float minIntensity = 0.15f;
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private float minRange = 1.5f;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercent => maxEnergy <= 0f ? 0f : currentEnergy / maxEnergy;

    public event Action<float, float, float> OnEnergyChanged;

    private void Awake()
    {
        maxEnergy = Mathf.Max(1f, maxEnergy);
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        UpdateLanternState();
    }

    private void Update()
    {
        DrainLantern();
    }

    private void DrainLantern()
    {
        if (currentEnergy <= 0f)
        {
            SetEnergy(0f);
            return;
        }

        SetEnergy(currentEnergy - drainRate * Time.deltaTime);
    }

    private void SetEnergy(float value)
    {
        float nextEnergy = Mathf.Clamp(value, 0f, maxEnergy);
        if (Mathf.Approximately(currentEnergy, nextEnergy))
        {
            return;
        }

        currentEnergy = nextEnergy;
        UpdateLanternState();
    }

    private void UpdateLanternState()
    {
        UpdateLanternLight();
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy, EnergyPercent);
    }

    private void UpdateLanternLight()
    {
        if (lanternLight == null)
        {
            return;
        }

        float percent = EnergyPercent;

        lanternLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, percent);
        lanternLight.range = Mathf.Lerp(minRange, maxRange, percent);
    }

    public void RestoreLight(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetEnergy(currentEnergy + amount);
    }

    public void RefillFull()
    {
        SetEnergy(maxEnergy);
    }
}
