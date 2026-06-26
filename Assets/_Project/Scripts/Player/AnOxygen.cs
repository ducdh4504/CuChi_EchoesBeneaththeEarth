using System;
using UnityEngine;

public class AnOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float currentOxygen = 100f;
    //[SerializeField] private float oxygenDecreaseRate = 3f; //100 / 3 ≈ 33 giây là hết oxy
    [SerializeField] private float oxygenDecreaseRate = 0.5f; //100 / 0.5 = 200 giây

    [Header("Drain Control")]
    [SerializeField] private bool pauseWhenStandingOnTerrain = true;
    [SerializeField] private bool restoreWhenStandingOnTerrain = true;
    [SerializeField] private float terrainOxygenRestorePercentPerSecond = 0.1f;
    [SerializeField] private float terrainCheckDistance = 1.5f;
    [SerializeField] private LayerMask terrainCheckMask = ~0;
    [SerializeField] private bool drainOnlyInsideDrainZone;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    public float OxygenPercent => maxOxygen <= 0f ? 0f : currentOxygen / maxOxygen;
    public bool IsFull => currentOxygen >= maxOxygen;

    public event Action<float, float, float> OnOxygenChanged;
    public event Action OnOutOfOxygen;

    private bool isOxygenDrainPaused;
    private int drainZoneCount;
    private bool isOutOfOxygen;

    private void Awake()
    {
        maxOxygen = Mathf.Max(1f, maxOxygen);
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
        isOutOfOxygen = currentOxygen <= 0f;
    }

    private void Update()
    {
        UpdateOxygenOverTime();
    }

    private void UpdateOxygenOverTime()
    {
        bool standingOnTerrainSurface = IsStandingOnTerrainSurface();

        if (isOutOfOxygen || isOxygenDrainPaused)
        {
            return;
        }

        if (standingOnTerrainSurface)
        {
            RestoreOxygenOnTerrainOverTime();
            return;
        }

        if (drainOnlyInsideDrainZone && drainZoneCount <= 0)
        {
            return;
        }

        DecreaseOxygenOverTime();
    }

    private void DecreaseOxygenOverTime()
    {
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

    private void RestoreOxygenOnTerrainOverTime()
    {
        if (!restoreWhenStandingOnTerrain || IsFull)
        {
            return;
        }

        float oxygenBeforeRestore = currentOxygen;
        float restoreAmount = maxOxygen * terrainOxygenRestorePercentPerSecond * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen + restoreAmount, 0f, maxOxygen);

        if (!Mathf.Approximately(currentOxygen, oxygenBeforeRestore))
        {
            NotifyOxygenChanged();
        }
    }

    private bool IsStandingOnTerrainSurface()
    {
        if (!pauseWhenStandingOnTerrain)
        {
            return false;
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.25f;
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, terrainCheckDistance, terrainCheckMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return hit.collider is TerrainCollider;
    }

    public void SetOxygenDrainPaused(bool paused)
    {
        isOxygenDrainPaused = paused;
    }

    public void EnterOxygenDrainZone()
    {
        drainZoneCount++;
    }

    public void ExitOxygenDrainZone()
    {
        drainZoneCount = Mathf.Max(0, drainZoneCount - 1);
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

    public void SetCurrentOxygen(float value)
    {
        currentOxygen = Mathf.Clamp(value, 0f, maxOxygen);
        isOutOfOxygen = currentOxygen <= 0f;
        NotifyOxygenChanged();
    }
}
