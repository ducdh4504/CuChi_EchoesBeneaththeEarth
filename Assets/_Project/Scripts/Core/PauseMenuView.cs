using UnityEngine;
using UnityEngine.UI;

public class PauseMenuView : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private PauseMenuController controller;

    public void Bind(PauseMenuController pauseController)
    {
        controller = pauseController;
        FindButtonsIfNeeded();

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Awake()
    {
        FindButtonsIfNeeded();
    }

    private void FindButtonsIfNeeded()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.name.ToLowerInvariant();

            if (resumeButton == null && buttonName.Contains("resume"))
            {
                resumeButton = button;
            }
            else if (mainMenuButton == null && buttonName.Contains("main"))
            {
                mainMenuButton = button;
            }
        }
    }

    private void ResumeGame()
    {
        controller?.ResumeGame();
    }

    private void ReturnToMainMenu()
    {
        controller?.ReturnToMainMenu();
    }
}
