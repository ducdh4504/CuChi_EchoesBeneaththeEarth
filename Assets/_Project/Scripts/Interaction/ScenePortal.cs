using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class ScenePortal : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Prompt")]
    [SerializeField] private string prompt = "Nhan E de chuyen scene";

    [Header("Scene Transition")]
    [SerializeField] private SceneTransitionController transitionController;
    [SerializeField] private string sceneName;
    [SerializeField] private string transitionMessage = "Dang chuyen canh...";

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
        return !isCutsceneRunning
            && controller != null
            && !controller.IsTransitioning
            && !string.IsNullOrWhiteSpace(sceneName);
    }

    public void Interact()
    {
        SceneTransitionController controller = GetTransitionController();
        if (controller == null)
        {
            Debug.LogWarning($"Cannot use scene portal '{name}' because SceneTransitionController was not found.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"Cannot use scene portal '{name}' because no scene name was assigned.");
            return;
        }

        if (cutsceneDirector != null)
        {
            StartCoroutine(PlayCutsceneThenLoadScene(controller));
            return;
        }

        LoadScene(controller);
    }

    private IEnumerator PlayCutsceneThenLoadScene(SceneTransitionController controller)
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
        LoadScene(controller);
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

    private void LoadScene(SceneTransitionController controller)
    {
        controller.LoadScene(sceneName, transitionMessage);
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
