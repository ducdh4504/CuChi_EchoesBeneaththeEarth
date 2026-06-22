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
    // using morse code
    [SerializeField] private AnInventory playerInventory;
    [Header("Discovery Rewards")]
    [SerializeField] private bool unlockMorseCodeAfterDiscovery = true;
    [SerializeField] private bool unlockSecretDecreeAfterDiscovery = true;

    //Display quest
    [Header("Mission")]
    [SerializeField] private string missionIdToCompleteAfterDiscovery;
    [SerializeField] private string objectiveAfterDiscoveryOverride;

    [Header("Interaction")]
    [SerializeField] private string interactPrompt = "Nhấn E để kiểm tra hộp y tế cũ";
    [SerializeField] private bool disableObjectAfterDiscovery;

    [Header("Scene Transition")]
    [SerializeField] private bool loadNextSceneAfterDiscovery = true;
    [SerializeField] private string nextSceneName = "Day2";
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
        ApplyDiscoveryRewards();
        HandleMissionAfterDiscovery();

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

    //Display quest
    private void HandleMissionAfterDiscovery()
    {
        if (MissionManager.Instance == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(missionIdToCompleteAfterDiscovery))
        {
            MissionManager.Instance.CompleteMission(missionIdToCompleteAfterDiscovery);
        }

        if (!string.IsNullOrWhiteSpace(objectiveAfterDiscoveryOverride))
        {
            MissionManager.Instance.SetCurrentObjective(objectiveAfterDiscoveryOverride);
        }
    }

    // using morse code
    private void ApplyDiscoveryRewards()
    {
        FindPlayerInventoryIfNeeded();

        if (playerInventory == null)
        {
            Debug.LogWarning("Cannot unlock story documents because AnInventory was not found on Player.");
            return;
        }

        if (unlockMorseCodeAfterDiscovery)
        {
            playerInventory.UnlockMorseCode();
        }

        if (unlockSecretDecreeAfterDiscovery)
        {
            playerInventory.UnlockSecretDecree();
        }
    }

    private void FindPlayerInventoryIfNeeded()
    {
        if (playerInventory != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found. Make sure An has Tag = Player.");
            return;
        }

        playerInventory = player.GetComponent<AnInventory>();
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

        GameSaveSystem.UnlockLevel(nextSceneName);
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
        FindPlayerInventoryIfNeeded();

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
