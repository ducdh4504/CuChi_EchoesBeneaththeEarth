//using UnityEngine;

//public class StoryItemPickup : MonoBehaviour, IInteractable, IInteractionAvailability
//{
//    [Header("Story Data")]
//    [SerializeField] private StoryDiscoveryData discoveryData;

//    [Header("References")]
//    [SerializeField] private StoryDiscoveryUI discoveryUI;
//    [SerializeField] private ObjectivePanelUI objectivePanelUI;
//    [SerializeField] private AnMovement playerMovement;

//    [Header("Interaction")]
//    [SerializeField] private string interactPrompt = "Nhấn E để kiểm tra hộp y tế cũ";
//    [SerializeField] private bool disableObjectAfterDiscovery;

//    private bool isDiscovered;

//    public string GetInteractPrompt()
//    {
//        return interactPrompt;
//    }

//    public bool CanInteract()
//    {
//        return !isDiscovered && discoveryData != null;
//    }

//    public void Interact()
//    {
//        if (!CanInteract())
//        {
//            return;
//        }

//        StartDiscovery();
//    }

//    private void StartDiscovery()
//    {
//        FindReferencesIfNeeded();

//        if (discoveryData == null)
//        {
//            Debug.LogWarning($"{gameObject.name} has no StoryDiscoveryData assigned.");
//            return;
//        }

//        if (discoveryUI == null)
//        {
//            Debug.LogWarning("StoryDiscoveryUI was not found or assigned.");
//            return;
//        }

//        isDiscovered = true;

//        SetPlayerControlEnabled(false);

//        discoveryUI.Show(discoveryData, CompleteDiscovery);
//    }

//    private void CompleteDiscovery()
//    {
//        if (
//            objectivePanelUI != null &&
//            discoveryData != null &&
//            !string.IsNullOrWhiteSpace(discoveryData.objectiveAfterDiscovery)
//        )
//        {
//            objectivePanelUI.SetObjective(discoveryData.objectiveAfterDiscovery);
//        }

//        SetPlayerControlEnabled(true);

//        if (disableObjectAfterDiscovery)
//        {
//            gameObject.SetActive(false);
//        }
//    }

//    private void SetPlayerControlEnabled(bool enabled)
//    {
//        FindPlayerMovementIfNeeded();

//        if (playerMovement == null)
//        {
//            return;
//        }

//        playerMovement.enabled = enabled;

//        if (!enabled)
//        {
//            Rigidbody rb = playerMovement.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                rb.linearVelocity = Vector3.zero;
//                rb.angularVelocity = Vector3.zero;
//            }
//        }
//    }

//    private void FindReferencesIfNeeded()
//    {
//        FindPlayerMovementIfNeeded();

//        if (discoveryUI == null)
//        {
//            discoveryUI = UnityEngine.Object.FindAnyObjectByType<StoryDiscoveryUI>(FindObjectsInactive.Include);
//        }

//        if (objectivePanelUI == null)
//        {
//            objectivePanelUI = UnityEngine.Object.FindAnyObjectByType<ObjectivePanelUI>(FindObjectsInactive.Include);
//        }
//    }

//    private void FindPlayerMovementIfNeeded()
//    {
//        if (playerMovement != null)
//        {
//            return;
//        }

//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        if (player == null)
//        {
//            Debug.LogWarning("Player not found. Make sure An has Tag = Player.");
//            return;
//        }

//        playerMovement = player.GetComponent<AnMovement>();
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryItemPickup : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Story Data")]
    [SerializeField] private StoryDiscoveryData discoveryData;

    [Header("References")]
    [SerializeField] private StoryDiscoveryUI discoveryUI;
    [SerializeField] private ObjectivePanelUI objectivePanelUI;
    [SerializeField] private AnMovement playerMovement;

    [Header("Interaction")]
    [SerializeField] private string interactPrompt = "Nhấn E để kiểm tra hộp y tế cũ";
    [SerializeField] private bool disableObjectAfterDiscovery;

    [Header("Scene Transition")]
    [SerializeField] private bool loadNextSceneAfterDiscovery = true;
    [SerializeField] private string nextSceneName = "Chapter_02_DarkMessage";
    [SerializeField] private float sceneLoadDelay = 0.5f;

    private bool isDiscovered;

    public string GetInteractPrompt()
    {
        return interactPrompt;
    }

    public bool CanInteract()
    {
        return !isDiscovered && discoveryData != null;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        StartDiscovery();
    }

    private void StartDiscovery()
    {
        FindReferencesIfNeeded();

        if (discoveryData == null)
        {
            Debug.LogWarning($"{gameObject.name} has no StoryDiscoveryData assigned.");
            return;
        }

        if (discoveryUI == null)
        {
            Debug.LogWarning("StoryDiscoveryUI was not found or assigned.");
            return;
        }

        isDiscovered = true;

        SetPlayerControlEnabled(false);

        discoveryUI.Show(discoveryData, CompleteDiscovery);
    }

    private void CompleteDiscovery()
    {
        if (disableObjectAfterDiscovery)
        {
            gameObject.SetActive(false);
        }

        if (loadNextSceneAfterDiscovery)
        {
            StartCoroutine(LoadNextSceneAfterDelay());
            return;
        }

        if (
            objectivePanelUI != null &&
            discoveryData != null &&
            !string.IsNullOrWhiteSpace(discoveryData.objectiveAfterDiscovery)
        )
        {
            objectivePanelUI.SetObjective(discoveryData.objectiveAfterDiscovery);
        }

        EndStoryControl();
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Next scene name is empty. Cannot load next scene.");
            EndStoryControl();
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void EndStoryControl()
    {

        SetPlayerControlEnabled(true);
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        FindPlayerMovementIfNeeded();

        if (playerMovement == null)
        {
            return;
        }

        playerMovement.enabled = enabled;

        if (!enabled)
        {
            Rigidbody rb = playerMovement.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void FindReferencesIfNeeded()
    {
        FindPlayerMovementIfNeeded();

        if (discoveryUI == null)
        {
            discoveryUI = Object.FindAnyObjectByType<StoryDiscoveryUI>(FindObjectsInactive.Include);
        }

        if (objectivePanelUI == null)
        {
            objectivePanelUI = Object.FindAnyObjectByType<ObjectivePanelUI>(FindObjectsInactive.Include);
        }
    }

    private void FindPlayerMovementIfNeeded()
    {
        if (playerMovement != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found. Make sure An has Tag = Player.");
            return;
        }

        playerMovement = player.GetComponent<AnMovement>();
    }
}