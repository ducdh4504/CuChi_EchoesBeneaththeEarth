using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] public Button playButton;
    [SerializeField] public Button quitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            GameSaveSystem.LoadSavedGameOrNewGame();
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
