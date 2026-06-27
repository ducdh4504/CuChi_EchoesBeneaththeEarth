using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string PauseSceneName = "PauseScene";

    private static PauseMenuController instance;

    private float previousTimeScale = 1f;
    private bool isPaused;
    private bool isLoadingPauseScene;
    private bool hadCursorVisible;
    private CursorLockMode previousCursorLockMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == PauseSceneName)
        {
            return;
        }

        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (!ShouldExistInScene(scene))
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }

            Time.timeScale = 1f;
            return;
        }

        EnsureEventSystem();

        if (instance != null)
        {
            instance.ClosePauseMenu();
            return;
        }

        GameObject controllerObject = new GameObject("PauseMenuController");
        DontDestroyOnLoad(controllerObject);
        instance = controllerObject.AddComponent<PauseMenuController>();
    }

private static bool ShouldExistInScene(Scene scene)
    {
        return scene.IsValid()
            && scene.isLoaded
            && (scene.name.StartsWith("Day") || scene.name == "Terrain");
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused || isLoadingPauseScene)
        {
            return;
        }

        previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        hadCursorVisible = Cursor.visible;
        previousCursorLockMode = Cursor.lockState;

        isPaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartCoroutine(LoadPauseSceneRoutine());
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = previousTimeScale;
        Cursor.visible = hadCursorVisible;
        Cursor.lockState = previousCursorLockMode;

        if (IsPauseSceneLoaded())
        {
            SceneManager.UnloadSceneAsync(PauseSceneName);
        }
    }

    public void ReturnToMainMenu()
    {
        GameSaveSystem.SaveCurrentGame();
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private IEnumerator LoadPauseSceneRoutine()
    {
        isLoadingPauseScene = true;

        if (!IsPauseSceneLoaded())
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(PauseSceneName, LoadSceneMode.Additive);
            while (loadOperation != null && !loadOperation.isDone)
            {
                yield return null;
            }
        }

        BindPauseSceneView();
        isLoadingPauseScene = false;
    }

    private void BindPauseSceneView()
    {
        PauseMenuView pauseView = FindAnyObjectByType<PauseMenuView>(FindObjectsInactive.Include);
        if (pauseView != null)
        {
            pauseView.Bind(this);
        }
    }

    private void ClosePauseMenu()
    {
        isPaused = false;
        isLoadingPauseScene = false;
        Time.timeScale = 1f;

        if (IsPauseSceneLoaded())
        {
            SceneManager.UnloadSceneAsync(PauseSceneName);
        }
    }

    private static bool IsPauseSceneLoaded()
    {
        Scene pauseScene = SceneManager.GetSceneByName(PauseSceneName);
        return pauseScene.IsValid() && pauseScene.isLoaded;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}
