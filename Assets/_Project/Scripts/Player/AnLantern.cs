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

    private void Awake()
    {
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        UpdateLanternLight();
    }

    private void Update()
    {
        DrainLantern();
        UpdateLanternLight();
    }

    private void DrainLantern()
    {
        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            return;
        }

        currentEnergy -= drainRate * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
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

        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        UpdateLanternLight();
    }

    public void RefillFull()
    {
        currentEnergy = maxEnergy;
        UpdateLanternLight();
    }
}