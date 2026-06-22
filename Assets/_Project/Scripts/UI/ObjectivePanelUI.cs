using TMPro;
using UnityEngine;

public class ObjectivePanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private string defaultTitle = "Nhiem vu hien tai";
    [SerializeField] private string objectivePrefix = "Muc tieu: ";

    private bool hasContent;
    private int temporaryHideRequests;

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
        SetMission(defaultTitle, objective, MissionState.Active);
    }

    public void SetMission(string missionTitle, string objective, MissionState state)
    {
        hasContent =
            !string.IsNullOrWhiteSpace(missionTitle) ||
            !string.IsNullOrWhiteSpace(objective);

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(missionTitle)
                ? defaultTitle
                : missionTitle;
        }

        if (objectiveText != null)
        {
            string stateText = GetStateText(state);

            objectiveText.text = string.IsNullOrWhiteSpace(objective)
                ? stateText
                : $"{stateText}{objectivePrefix}{objective}";
        }

        SetVisible(hasContent);
    }

    public void ClearObjective()
    {
        hasContent = false;

        if (titleText != null)
        {
            titleText.text = string.Empty;
        }

        if (objectiveText != null)
        {
            objectiveText.text = string.Empty;
        }

        SetVisible(false);
    }

    public void HideTemporarily()
    {
        temporaryHideRequests++;
        ApplyVisible(false);
    }

    public void ShowAfterTemporaryHide()
    {
        if (temporaryHideRequests > 0)
        {
            temporaryHideRequests--;
        }

        SetVisible(hasContent);
    }

    private string GetStateText(MissionState state)
    {
        switch (state)
        {
            case MissionState.Completed:
                return "Trang thai: Hoan thanh\n";

            case MissionState.Active:
                return "Trang thai: Dang thuc hien\n";

            default:
                return string.Empty;
        }
    }

    private void SetVisible(bool visible)
    {
        ApplyVisible(visible && temporaryHideRequests <= 0);
    }

    private void ApplyVisible(bool visible)
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

        if (titleText != null)
        {
            titleText.enabled = visible;
        }
    }
}
