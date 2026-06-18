using System.Collections;
using TMPro;
using UnityEngine;

public class FlashlightPickupPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine showRoutine;

    private void Awake()
    {
        if (promptText == null)
        {
            promptText = GetComponentInChildren<TMP_Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        Hide();
    }

    public void ShowFlashlightPrompt(string message, float duration)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        showRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    private IEnumerator ShowRoutine(string message, float duration)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }

        SetVisible(true);

        yield return new WaitForSeconds(duration);

        Hide();
        showRoutine = null;
    }

    private void Hide()
    {
        SetVisible(false);
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

        if (promptText != null)
        {
            promptText.enabled = visible;
        }
    }
}