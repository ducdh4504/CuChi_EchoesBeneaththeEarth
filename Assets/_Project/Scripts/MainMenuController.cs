using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // Load cảnh màn 1 khi bấm nút Play
        SceneManager.LoadScene("Chapter_01_SecretEntrance");
    }

    public void QuitGame()
    {
        // Thoát game khi bấm nút Quit
        Debug.Log("Đã bấm thoát game!");
        Application.Quit();
    }
}