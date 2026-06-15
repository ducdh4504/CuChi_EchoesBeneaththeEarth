using TMPro;
using UnityEngine;

public class ObjectivePanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private string prefix = "Mục tiêu: ";

    private void Awake()
    {
        if (objectiveText == null)
        {
            objectiveText = GetComponentInChildren<TMP_Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void SetObjective(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = string.IsNullOrWhiteSpace(objective)
                ? string.Empty
                : $"{prefix}{objective}";
        }

        SetVisible(!string.IsNullOrWhiteSpace(objective));
    }

    public void ClearObjective()
    {
        if (objectiveText != null)
        {
            objectiveText.text = string.Empty;
        }

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

        if (objectiveText != null)
        {
            objectiveText.enabled = visible;
        }
    }
}