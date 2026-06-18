using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightBatteryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnFlashlight playerFlashlight;
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Warning Settings")]
    [SerializeField] private float lowBatteryThreshold = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 0.85f, 0.25f);
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.2f);

    private bool isSubscribed;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        FindPlayerFlashlightIfNeeded();
        SetVisible(false);
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void Start()
    {
        TrySubscribe();
        Refresh();
    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
            Refresh();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void UpdateBatteryBar(float currentBattery, float maxBattery, float batteryPercent)
    {
        batteryPercent = Mathf.Clamp01(batteryPercent);

        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = batteryPercent;
            batteryFillImage.color = batteryPercent <= lowBatteryThreshold ? warningColor : normalColor;
        }

        if (batteryText != null)
        {
            int percent = Mathf.RoundToInt(batteryPercent * 100f);
            batteryText.text = $"PIN {percent}%";
        }
    }

    private void HandleFlashlightStateChanged(bool hasFlashlight, bool isEquipped, bool isLightOn)
    {
        SetVisible(hasFlashlight);
        Refresh();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        FindPlayerFlashlightIfNeeded();

        if (playerFlashlight == null)
        {
            SetVisible(false);
            return;
        }

        playerFlashlight.OnBatteryChanged += UpdateBatteryBar;
        playerFlashlight.OnFlashlightStateChanged += HandleFlashlightStateChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || playerFlashlight == null)
        {
            isSubscribed = false;
            return;
        }

        playerFlashlight.OnBatteryChanged -= UpdateBatteryBar;
        playerFlashlight.OnFlashlightStateChanged -= HandleFlashlightStateChanged;
        isSubscribed = false;
    }

    private void Refresh()
    {
        FindPlayerFlashlightIfNeeded();

        if (playerFlashlight == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(playerFlashlight.HasFlashlight);

        UpdateBatteryBar(
            playerFlashlight.CurrentBattery,
            playerFlashlight.MaxBattery,
            playerFlashlight.BatteryPercent);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            return;
        }

        if (batteryFillImage != null)
        {
            batteryFillImage.enabled = visible;
        }

        if (batteryText != null)
        {
            batteryText.enabled = visible;
        }
    }

    private void FindPlayerFlashlightIfNeeded()
    {
        if (playerFlashlight != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerFlashlight = player.GetComponent<AnFlashlight>();
        }

        if (playerFlashlight == null)
        {
            playerFlashlight = FindAnyObjectByType<AnFlashlight>();
        }
    }
}