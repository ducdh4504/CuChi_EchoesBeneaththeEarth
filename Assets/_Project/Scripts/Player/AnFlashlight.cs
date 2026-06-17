using System;
using UnityEngine;

public class AnFlashlight : MonoBehaviour
{
    [Header("Flashlight State")]
    [SerializeField] private bool hasFlashlight;
    [SerializeField] private bool equipAfterUnlock = true;
    [SerializeField] private bool turnOnAfterUnlock;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float currentBattery = 100f;
    [SerializeField] private float batteryDrainRate = 2.5f;

    [Header("Input Behavior")]
    [SerializeField] private float doubleTapWindow = 0.35f;

    [Header("Visual References")]
    [SerializeField] private GameObject flashlightVisualRoot;
    [SerializeField] private Transform flashlightPivot;
    [SerializeField] private Light flashlightLight;

    [Header("Aim")]
    [SerializeField] private Transform aimReference;
    [SerializeField] private bool followCameraDirection = true;
    [SerializeField] private float aimFollowSpeed = 20f;

    [Header("Light Settings")]
    [SerializeField] private float maxIntensity = 4.5f;
    [SerializeField] private float minLitIntensity = 1.2f;
    [SerializeField] private float maxRange = 16f;
    [SerializeField] private float minLitRange = 7f;

    public bool HasFlashlight => hasFlashlight;
    public bool IsEquipped { get; private set; }
    public bool IsLightOn { get; private set; }

    public float CurrentBattery => currentBattery;
    public float MaxBattery => maxBattery;
    public float BatteryPercent => maxBattery <= 0f ? 0f : currentBattery / maxBattery;
    public bool IsBatteryFull => currentBattery >= maxBattery;

    public event Action<float, float, float> OnBatteryChanged;
    public event Action<bool, bool, bool> OnFlashlightStateChanged;

    private float lastButtonPressedTime = -999f;

    private void Awake()
    {
        maxBattery = Mathf.Max(1f, maxBattery);
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        FindAimReferenceIfNeeded();

        if (!hasFlashlight)
        {
            IsEquipped = false;
            IsLightOn = false;
        }

        ApplyVisualState();
        NotifyBatteryChanged();
        NotifyStateChanged();
    }

    private void Update()
    {
        DrainBatteryIfNeeded();
    }

    private void LateUpdate()
    {
        UpdateAimDirection();
    }

    public void HandleFlashlightButtonPressed()
    {
        if (!hasFlashlight)
        {
            Debug.Log("An chưa có đèn pin.");
            return;
        }

        bool isDoubleTap = Time.time - lastButtonPressedTime <= doubleTapWindow;

        if (isDoubleTap && IsEquipped)
        {
            HolsterFlashlight();
            lastButtonPressedTime = -999f;
            return;
        }

        lastButtonPressedTime = Time.time;

        if (!IsEquipped)
        {
            EquipFlashlight(true);
            return;
        }

        if (IsLightOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }

    public bool UnlockFlashlight()
    {
        if (hasFlashlight)
        {
            Debug.Log("An already has a flashlight.");
            return false;
        }

        hasFlashlight = true;

        if (currentBattery <= 0f)
        {
            currentBattery = maxBattery;
        }

        if (equipAfterUnlock)
        {
            EquipFlashlight(turnOnAfterUnlock);
        }
        else
        {
            IsEquipped = false;
            IsLightOn = false;
            ApplyVisualState();
            NotifyStateChanged();
        }

        NotifyBatteryChanged();

        Debug.Log("Flashlight unlocked.");
        return true;
    }

    public void EquipFlashlight(bool turnOn)
    {
        if (!hasFlashlight)
        {
            return;
        }

        IsEquipped = true;

        if (turnOn)
        {
            TurnOn();
        }
        else
        {
            IsLightOn = false;
            ApplyVisualState();
            NotifyStateChanged();
        }
    }

    public void HolsterFlashlight()
    {
        IsLightOn = false;
        IsEquipped = false;

        ApplyVisualState();
        NotifyStateChanged();

        Debug.Log("Flashlight holstered.");
    }

    public void TurnOn()
    {
        if (!hasFlashlight)
        {
            Debug.Log("Cannot turn on flashlight because An does not have it.");
            return;
        }

        if (currentBattery <= 0f)
        {
            Debug.Log("Flashlight battery is empty.");
            TurnOff();
            return;
        }

        IsEquipped = true;
        IsLightOn = true;

        ApplyVisualState();
        NotifyStateChanged();
    }

    public void TurnOff()
    {
        IsLightOn = false;

        ApplyVisualState();
        NotifyStateChanged();
    }

    public bool TryRestoreBattery(float amount)
    {
        if (!hasFlashlight)
        {
            Debug.Log("Cannot restore flashlight battery because An does not have a flashlight yet.");
            return false;
        }

        if (amount <= 0f)
        {
            return false;
        }

        if (IsBatteryFull)
        {
            Debug.Log("Flashlight battery is already full.");
            return false;
        }

        float batteryBeforeRestore = currentBattery;

        currentBattery = Mathf.Clamp(currentBattery + amount, 0f, maxBattery);

        float restoredAmount = currentBattery - batteryBeforeRestore;
        Debug.Log($"Flashlight battery restored: {restoredAmount}");

        NotifyBatteryChanged();
        ApplyVisualState();

        return restoredAmount > 0f;
    }

    public void RefillFull()
    {
        currentBattery = maxBattery;
        NotifyBatteryChanged();
        ApplyVisualState();
    }

    private void DrainBatteryIfNeeded()
    {
        if (!hasFlashlight || !IsEquipped || !IsLightOn)
        {
            return;
        }

        if (currentBattery <= 0f)
        {
            currentBattery = 0f;
            TurnOff();
            NotifyBatteryChanged();
            return;
        }

        currentBattery -= batteryDrainRate * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        NotifyBatteryChanged();
        ApplyVisualState();

        if (currentBattery <= 0f)
        {
            Debug.Log("Flashlight battery is empty. Light turned off.");
            TurnOff();
        }
    }

    private void UpdateAimDirection()
    {
        if (!followCameraDirection || flashlightPivot == null)
        {
            return;
        }

        FindAimReferenceIfNeeded();

        if (aimReference == null)
        {
            return;
        }

        flashlightPivot.rotation = Quaternion.Slerp(
            flashlightPivot.rotation,
            aimReference.rotation,
            Time.deltaTime * aimFollowSpeed);
    }

    private void ApplyVisualState()
    {
        if (flashlightVisualRoot != null)
        {
            flashlightVisualRoot.SetActive(hasFlashlight && IsEquipped);
        }

        if (flashlightLight == null)
        {
            return;
        }

        bool shouldLightBeEnabled = hasFlashlight && IsEquipped && IsLightOn && currentBattery > 0f;

        flashlightLight.enabled = shouldLightBeEnabled;

        if (!shouldLightBeEnabled)
        {
            return;
        }

        float percent = BatteryPercent;

        flashlightLight.intensity = Mathf.Lerp(minLitIntensity, maxIntensity, percent);
        flashlightLight.range = Mathf.Lerp(minLitRange, maxRange, percent);
    }

    private void FindAimReferenceIfNeeded()
    {
        if (aimReference != null)
        {
            return;
        }

        if (Camera.main != null)
        {
            aimReference = Camera.main.transform;
        }
    }

    private void NotifyBatteryChanged()
    {
        OnBatteryChanged?.Invoke(currentBattery, maxBattery, BatteryPercent);
    }

    private void NotifyStateChanged()
    {
        OnFlashlightStateChanged?.Invoke(hasFlashlight, IsEquipped, IsLightOn);
    }
}