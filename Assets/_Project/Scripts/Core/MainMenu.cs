using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button quitButton;

    [Header("Level Select")]
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [SerializeField] private Button backButton;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshMenuState();
        HideLevelSelect();
    }

    public void StartNewGame()
    {
        GameSaveSystem.LoadNewGame();
    }

    public void ContinueGame()
    {
        GameSaveSystem.LoadSavedGameOrNewGame();
    }

    private void LoadLevel(string sceneName)
    {
        GameSaveSystem.LoadLevel(sceneName);
    }

    public void LoadDay1()
    {
        LoadLevel("Day1");
    }

    public void LoadDay2()
    {
        LoadLevel("Day2");
    }

    public void LoadDay3()
    {
        LoadLevel("Day3");
    }

    public void ShowLevelSelect()
    {
        RefreshLevelButtons();

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
        }
    }

    public void HideLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void RefreshMenuState()
    {
        bool hasSave = GameSaveSystem.HasSave;

        SetButtonLabel(newGameButton, "NEW GAME");
        SetButtonLabel(continueButton, "CONTINUE");

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSave);
            continueButton.interactable = hasSave;
        }

        RefreshLevelButtons();
    }

    private void RefreshLevelButtons()
    {
        SetLevelButtonState(level1Button, "DAY 1", GameSaveSystem.IsLevelUnlocked("Day1"));
        SetLevelButtonState(level2Button, "DAY 2", GameSaveSystem.IsLevelUnlocked("Day2"));
        SetLevelButtonState(level3Button, "DAY 3", GameSaveSystem.IsLevelUnlocked("Day3"));
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        Text buttonText = button.GetComponentInChildren<Text>(true);
        if (buttonText != null)
        {
            buttonText.text = label;
        }
    }

    private static void SetLevelButtonState(Button button, string label, bool isUnlocked)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = isUnlocked;
        SetButtonLabel(button, isUnlocked ? label : $"{label} LOCKED");
    }
}
