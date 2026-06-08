using UnityEngine;

public class TunnelEntranceInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Prompt")]
    [SerializeField] private string prompt = "Nhan E de vao dia dao";

    [Header("Transition")]
    [SerializeField] private SceneTransitionController transitionController;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private string transitionMessage = "Dang vao dia dao...";
    [SerializeField] private string sceneName;

    public string GetInteractPrompt()
    {
        return prompt;
    }

    public bool CanInteract()
    {
        SceneTransitionController controller = GetTransitionController();
        if (controller == null || controller.IsTransitioning)
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
            Debug.LogWarning("Cannot enter tunnel because SceneTransitionController was not found.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            controller.LoadScene(sceneName, transitionMessage);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Cannot enter tunnel because Player was not found.");
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
