using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OxygenBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnOxygen playerOxygen;
    [SerializeField] private Image oxygenFillImage;
    [SerializeField] private TMP_Text oxygenText;

    [Header("Warning Settings")]
    [SerializeField] private float lowOxygenThreshold = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.2f);

    private bool isSubscribed;

    private void Awake()
    {
        FindPlayerOxygenIfNeeded();
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

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        FindPlayerOxygenIfNeeded();

        if (playerOxygen == null)
        {
            return;
        }

        playerOxygen.OnOxygenChanged += UpdateOxygenBar;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || playerOxygen == null)
        {
            isSubscribed = false;
            return;
        }

        playerOxygen.OnOxygenChanged -= UpdateOxygenBar;
        isSubscribed = false;
    }

    private void Refresh()
    {
        if (playerOxygen == null)
        {
            return;
        }

        UpdateOxygenBar(playerOxygen.CurrentOxygen, playerOxygen.MaxOxygen, playerOxygen.OxygenPercent);
    }

    private void FindPlayerOxygenIfNeeded()
    {
        if (playerOxygen != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerOxygen = player.GetComponent<AnOxygen>();
        }

        if (playerOxygen == null)
        {
            playerOxygen = FindAnyObjectByType<AnOxygen>();
        }
    }
}
