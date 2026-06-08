using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AnInteractor interactor;

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

    private void Start()
    {
        FindInteractorIfNeeded();
    }

    private void Update()
    {
        FindInteractorIfNeeded();

        if (interactor == null)
        {
            Hide();
            return;
        }

        IInteractable interactable = interactor.CurrentInteractable;

        if (interactable == null)
        {
            Hide();
            return;
        }

        Show(interactable.GetInteractPrompt());
    }

    private void FindInteractorIfNeeded()
    {
        if (interactor != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            interactor = player.GetComponent<AnInteractor>();
        }
    }

    private void Show(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }

        SetVisible(true);
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
