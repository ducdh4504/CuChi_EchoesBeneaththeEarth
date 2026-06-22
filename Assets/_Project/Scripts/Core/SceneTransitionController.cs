using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [Header("Fade UI")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private float fadeDuration = 0.65f;
    [SerializeField] private float holdDuration = 0.35f;

    [Header("Ending Transition")]
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private float endSceneFadeDuration = 1.8f;
    [SerializeField] private float endSceneHoldDuration = 0.8f;

    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetFade(0f);

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }
    }

    public void TeleportPlayer(Transform player, Transform destination, string message)
    {
        if (IsTransitioning || player == null || destination == null)
        {
            return;
        }

        StartCoroutine(TeleportRoutine(player, destination, message));
    }

    public void LoadScene(string sceneName, string message)
    {
        if (IsTransitioning || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName, message));
    }

    private IEnumerator TeleportRoutine(Transform player, Transform destination, string message)
    {
        IsTransitioning = true;
        SetMessage(message);

        AnMovement movement = player.GetComponent<AnMovement>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        yield return FadeTo(1f);
        yield return new WaitForSeconds(holdDuration);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = destination.position;
            rb.rotation = destination.rotation;
        }
        else
        {
            player.SetPositionAndRotation(destination.position, destination.rotation);
        }

        Physics.SyncTransforms();
        yield return null;

        if (movement != null)
        {
            movement.enabled = true;
        }

        yield return FadeTo(0f);
        SetMessage(string.Empty);
        IsTransitioning = false;
    }

    private IEnumerator LoadSceneRoutine(string sceneName, string message)
    {
        IsTransitioning = true;
        SetMessage(message);

        float sceneFadeDuration = GetFadeDurationForScene(sceneName);
        float sceneHoldDuration = GetHoldDurationForScene(sceneName);

        yield return FadeTo(1f, sceneFadeDuration);
        yield return new WaitForSecondsRealtime(sceneHoldDuration);

        GameSaveSystem.UnlockLevel(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        yield return FadeTo(targetAlpha, fadeDuration);
    }

    private IEnumerator FadeTo(float targetAlpha, float durationOverride)
    {
        if (fadeGroup == null)
        {
            yield break;
        }

        float startAlpha = fadeGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, durationOverride);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetFade(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetFade(targetAlpha);
    }

    private void SetFade(float alpha)
    {
        if (fadeGroup == null)
        {
            return;
        }

        fadeGroup.alpha = alpha;
        fadeGroup.blocksRaycasts = alpha > 0.01f;
        fadeGroup.interactable = alpha > 0.01f;
    }

    private void SetMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message ?? string.Empty;
        }
    }

    private float GetFadeDurationForScene(string sceneName)
    {
        return IsEndScene(sceneName) ? endSceneFadeDuration : fadeDuration;
    }

    private float GetHoldDurationForScene(string sceneName)
    {
        return IsEndScene(sceneName) ? endSceneHoldDuration : holdDuration;
    }

    private bool IsEndScene(string sceneName)
    {
        return string.Equals(sceneName, endSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
}
