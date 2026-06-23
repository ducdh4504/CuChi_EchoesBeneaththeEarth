using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("UI")]
    [SerializeField] private GameObject morsePuzzleUIPrefab;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private string promptText = "Nhấn E để chơi mã Morse";

    [Header("Behaviour")]
    [SerializeField] private bool disablePlayerMovementWhileOpen = true;
    [SerializeField] private bool showCursorWhileOpen = true;
    [SerializeField] private bool pauseOxygenWhileOpen = true;
    [SerializeField] private float autoCloseAfterWinSeconds = 1.0f;

    [Header("On Win - Scene Transition")]
    [SerializeField] private bool loadSceneOnWin = true;
    [SerializeField] private string sceneOnWin = "Day3";
    [SerializeField] private string transitionMessage = "Đã giải mã thành công...";

    private GameObject currentInstance;
    private MorseChallenge currentChallenge;
    private MonoBehaviour disabledMovement;
    private AnOxygen pausedOxygen;
    private ObjectivePanelUI hiddenObjectivePanel;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;
    private bool inventoryWasActive;
    private bool closing;
    private bool winLatched;

    public string GetInteractPrompt() => promptText;

    public bool CanInteract() => currentInstance == null && morsePuzzleUIPrefab != null;

    public void Interact()
    {
        if (currentInstance != null || morsePuzzleUIPrefab == null) return;

        currentInstance = Instantiate(morsePuzzleUIPrefab);
        currentInstance.SetActive(true);

        currentChallenge = currentInstance.GetComponentInChildren<MorseChallenge>(true);
        if (currentChallenge != null)
        {
            currentChallenge.ResetGame();
            currentChallenge.onWin.AddListener(HandleWin);
        }

        if (inventoryUI != null)
        {
            inventoryWasActive = inventoryUI.activeSelf;
            inventoryUI.SetActive(false);
        }

        hiddenObjectivePanel = FindAnyObjectByType<ObjectivePanelUI>(FindObjectsInactive.Include);
        if (hiddenObjectivePanel != null)
        {
            hiddenObjectivePanel.HideTemporarily();
        }

        var player = GameObject.FindGameObjectWithTag("Player");

        if (disablePlayerMovementWhileOpen && player != null)
        {
            disabledMovement = player.GetComponent("AnMovement") as MonoBehaviour;
            if (disabledMovement != null) disabledMovement.enabled = false;
        }

        if (pauseOxygenWhileOpen && player != null)
        {
            pausedOxygen = player.GetComponent<AnOxygen>();
            if (pausedOxygen != null) pausedOxygen.enabled = false;
        }

        if (showCursorWhileOpen)
        {
            previousLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if (currentInstance == null || closing) return;

        Keyboard keyboard = Keyboard.current;
        if ((keyboard != null && keyboard.eKey.wasPressedThisFrame) || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
        {
            Close();
        }
    }

    private void HandleWin()
    {
        if (closing) return;
        winLatched = true;
        StartCoroutine(CloseAfterDelay(autoCloseAfterWinSeconds));
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        closing = true;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        Close();
    }

    private void Close()
    {
        if (currentChallenge != null)
        {
            currentChallenge.onWin.RemoveListener(HandleWin);
            currentChallenge = null;
        }

        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }

        if (inventoryUI != null && inventoryWasActive)
        {
            inventoryUI.SetActive(true);
        }

        if (hiddenObjectivePanel != null)
        {
            hiddenObjectivePanel.ShowAfterTemporaryHide();
            hiddenObjectivePanel = null;
        }

        if (disabledMovement != null)
        {
            disabledMovement.enabled = true;
            disabledMovement = null;
        }

        if (pausedOxygen != null)
        {
            pausedOxygen.enabled = true;
            pausedOxygen = null;
        }

        if (showCursorWhileOpen)
        {
            Cursor.lockState = previousLockMode;
            Cursor.visible = previousCursorVisible;
        }

        closing = false;

        if (winLatched && loadSceneOnWin && !string.IsNullOrWhiteSpace(sceneOnWin))
        {
            winLatched = false;
            TriggerSceneTransition();
        }
        else
        {
            winLatched = false;
        }
    }

    private void TriggerSceneTransition()
    {
        GameSaveSystem.UnlockLevel(sceneOnWin);

        if (SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.LoadScene(sceneOnWin, transitionMessage);
        }
        else
        {
            GameSaveSystem.CapturePlayerOxygenForSceneTransition();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneOnWin);
        }
    }

    private void OnDisable()
    {
        if (currentInstance != null) Close();
    }
}
