using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TeleportPortal : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Prompt")]
    [SerializeField] private string prompt = "Nhan E de di chuyen";

    [Header("Transition")]
    [SerializeField] private SceneTransitionController transitionController;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private string transitionMessage = "Dang di chuyen...";
    [SerializeField] private string sceneName;

    [Header("Optional Cutscene")]
    [SerializeField] private PlayableDirector cutsceneDirector;
    [SerializeField] private GameObject[] cutsceneObjectsToEnable;
    [SerializeField] private bool waitForCutscene = true;

    private bool isCutsceneRunning;

    public string GetInteractPrompt()
    {
        return prompt;
    }

    public bool CanInteract()
    {
        SceneTransitionController controller = GetTransitionController();
        if (isCutsceneRunning || controller == null || controller.IsTransitioning)
        {
            return false;
        }

        return teleportDestination != null || !string.IsNullOrWhiteSpace(sceneName);
    }

    public void Interact()
    {
        SceneTransitionController controller = GetTransitionController();
        if (controller == null)
        {
            Debug.LogWarning($"Cannot use portal '{name}' because SceneTransitionController was not found.");
            return;
        }

        if (cutsceneDirector != null)
        {
            StartCoroutine(PlayCutsceneThenUsePortal(controller));
            return;
        }

        UsePortal(controller);
    }

    private IEnumerator PlayCutsceneThenUsePortal(SceneTransitionController controller)
    {
        isCutsceneRunning = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        AnMovement movement = player != null ? player.GetComponent<AnMovement>() : null;
        ThirdPersonCamera gameplayCamera = Camera.main != null ? Camera.main.GetComponent<ThirdPersonCamera>() : null;

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = false;
        }

        SetCutsceneObjectsActive(true);

        cutsceneDirector.time = 0d;
        cutsceneDirector.Play();

        if (waitForCutscene)
        {
            while (cutsceneDirector != null && cutsceneDirector.state == PlayState.Playing)
            {
                yield return null;
            }
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.enabled = true;
        }

        SetCutsceneObjectsActive(false);

        isCutsceneRunning = false;
        UsePortal(controller);
    }

    private void SetCutsceneObjectsActive(bool active)
    {
        if (cutsceneObjectsToEnable == null)
        {
            return;
        }

        for (int i = 0; i < cutsceneObjectsToEnable.Length; i++)
        {
            if (cutsceneObjectsToEnable[i] != null)
            {
                cutsceneObjectsToEnable[i].SetActive(active);
            }
        }
    }

    private void UsePortal(SceneTransitionController controller)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            controller.LoadScene(sceneName, transitionMessage);
            return;
        }

        if (teleportDestination == null)
        {
            Debug.LogWarning($"Cannot use portal '{name}' because no teleport destination was assigned.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"Cannot use portal '{name}' because Player was not found.");
            return;
        }

        controller.TeleportPlayer(player.transform, teleportDestination, transitionMessage);
    }

    private SceneTransitionController GetTransitionController()
    {
        if (transitionController == null)
        {
            transitionController = SceneTransitionController.Instance;
        }

        return transitionController;
    }
}
