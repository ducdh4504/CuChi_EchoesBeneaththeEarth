using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OxygenDeathScreen : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private AnOxygen playerOxygen;
    [SerializeField] private string playerTag = "Player";

    [Header("Death Screen")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private TMP_Text deathMessageText;
    [SerializeField] private string deathMessage = "An đã hết Oxy";
    [SerializeField] private Color overlayColor = Color.black;
    [SerializeField] private Color messageColor = Color.white;
    [SerializeField] private float fadeInDuration = 1.8f;
    [SerializeField] private float messageDelay = 0.35f;
    [SerializeField] private float messageFadeDuration = 1.2f;
    [SerializeField] private float holdDuration = 1.4f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Coroutine deathRoutine;
    private Coroutine subscribeRoutine;
    private bool isSubscribed;
    private bool isHandlingDeath;

    private void Awake()
    {
        EnsureOverlayExists();
        SetOverlay(0f, 0f, false);
    }

    private void OnEnable()
    {
        TrySubscribe();
        if (!isSubscribed && subscribeRoutine == null)
        {
            subscribeRoutine = StartCoroutine(SubscribeWhenPlayerExists());
        }
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator SubscribeWhenPlayerExists()
    {
        while (!isSubscribed)
        {
            TrySubscribe();
            yield return new WaitForSecondsRealtime(0.25f);
        }

        subscribeRoutine = null;
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

        playerOxygen.OnOutOfOxygen += HandleOutOfOxygen;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || playerOxygen == null)
        {
            return;
        }

        playerOxygen.OnOutOfOxygen -= HandleOutOfOxygen;
        isSubscribed = false;
    }

    private void HandleOutOfOxygen()
    {
        if (isHandlingDeath)
        {
            return;
        }

        isHandlingDeath = true;
        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
        }

        deathRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return FadeOverlay(1f, 0f, fadeInDuration);

        if (messageDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(messageDelay);
        }

        yield return FadeOverlay(1f, 1f, messageFadeDuration);

        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator FadeOverlay(float targetOverlayAlpha, float targetTextAlpha, float duration)
    {
        float startOverlayAlpha = overlayGroup != null ? overlayGroup.alpha : 0f;
        float startTextAlpha = deathMessageText != null ? deathMessageText.alpha : 0f;
        float safeDuration = Mathf.Max(0.01f, duration);
        float startTime = Time.realtimeSinceStartup;

        while (true)
        {
            yield return null;

            float elapsed = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsed / safeDuration);
            SetOverlay(
                Mathf.Lerp(startOverlayAlpha, targetOverlayAlpha, t),
                Mathf.Lerp(startTextAlpha, targetTextAlpha, t),
                true);

            if (t >= 1f)
            {
                break;
            }
        }

        SetOverlay(targetOverlayAlpha, targetTextAlpha, true);
    }

    private void FindPlayerOxygenIfNeeded()
    {
        if (playerOxygen != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerOxygen = player.GetComponent<AnOxygen>();
        }

        if (playerOxygen == null)
        {
            playerOxygen = FindAnyObjectByType<AnOxygen>();
        }
    }

    private void EnsureOverlayExists()
    {
        if (overlayGroup != null && deathMessageText != null)
        {
            overlayGroup.transform.SetAsLastSibling();
            deathMessageText.text = deathMessage;
            ApplyColors();
            return;
        }

        GameObject overlay = new GameObject("OxygenDeathScreen", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = overlayColor;
        overlayImage.raycastTarget = true;

        overlayGroup = overlay.GetComponent<CanvasGroup>();

        GameObject message = new GameObject("DeathMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
        message.transform.SetParent(overlay.transform, false);

        RectTransform messageRect = message.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = Vector2.zero;
        messageRect.sizeDelta = new Vector2(760f, 140f);

        deathMessageText = message.GetComponent<TMP_Text>();
        deathMessageText.text = deathMessage;
        deathMessageText.alignment = TextAlignmentOptions.Center;
        deathMessageText.fontSize = 48f;
        deathMessageText.color = messageColor;
        deathMessageText.raycastTarget = false;
    }

    private void OnValidate()
    {
        ApplyColors();
        if (deathMessageText != null)
        {
            deathMessageText.text = deathMessage;
        }
    }

    private void ApplyColors()
    {
        if (overlayGroup != null && overlayGroup.TryGetComponent(out Image overlayImage))
        {
            overlayImage.color = overlayColor;
        }

        if (deathMessageText != null)
        {
            deathMessageText.color = messageColor;
        }
    }

    private void SetOverlay(float overlayAlpha, float textAlpha, bool blocksRaycasts)
    {
        if (overlayGroup != null)
        {
            overlayGroup.alpha = overlayAlpha;
            overlayGroup.interactable = blocksRaycasts;
            overlayGroup.blocksRaycasts = blocksRaycasts;
        }

        if (deathMessageText != null)
        {
            deathMessageText.alpha = textAlpha;
        }
    }
}
