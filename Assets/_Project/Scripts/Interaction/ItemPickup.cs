using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    //Display quest
    [Header("Mission")]
    [SerializeField] private string missionIdToStartOnPickup;
    [SerializeField] private string missionIdToCompleteOnPickup;
    [SerializeField] private string objectiveAfterPickup;

    private bool isPickedUp;
    private AnInventory playerInventory;
    private AnOxygen playerOxygen;
    //private AnLantern playerLantern;

    #region xử lý đèn pin
    private AnFlashlight playerFlashlight;
    private ObjectivePanelUI objectivePanelUI;
    [SerializeField] private FlashlightPickupPromptUI flashlightPickupPromptUI;

    [SerializeField, TextArea(2, 4)]
    private string flashlightPickupPrompt =
        "Đã nhặt đèn pin. Nhấn phím 2 để sử dụng. Đèn pin có giới hạn pin, hãy sử dụng cẩn thận.";

    [SerializeField] private float flashlightPickupPromptDuration = 5f;
    #endregion

    public string GetInteractPrompt()
    {
        if (itemData == null)
        {
            return "Nhấn E để nhặt vật phẩm";
        }

        return $"Nhấn E để nhặt {itemData.itemName}";
    }

    public bool CanInteract()
    {
        return !isPickedUp && itemData != null;
    }

    public void Interact()
    {
        TryPickup();
    }

    private void TryPickup()
    {
        if (isPickedUp)
        {
            return;
        }

        if (itemData == null)
        {
            Debug.LogWarning($"{gameObject.name} has no ItemData assigned.");
            return;
        }

        FindPlayerReferencesIfNeeded();

        if (!TryApplyItemEffect())
        {
            return;
        }

        isPickedUp = true;

        if (playerInventory != null)
        {
            playerInventory.AddItem(itemData);
        }
        HandleMissionAfterPickup();

        if (!string.IsNullOrWhiteSpace(itemData.messageWhenCollected))
        {
            Debug.Log(itemData.messageWhenCollected);
        }

        // xử lý đèn pin
        ShowFlashlightPickupPromptIfNeeded();
        if (
            itemData.effectType != ItemEffectType.UnlockFlashlight &&
            objectivePanelUI != null &&
            !string.IsNullOrWhiteSpace(itemData.objectiveWhenCollected)
        )
                {
                    objectivePanelUI.SetObjective(itemData.objectiveWhenCollected);
                }

        if (itemData.destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }
    private void HandleMissionAfterPickup()
    {
        if (MissionManager.Instance == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(missionIdToStartOnPickup))
        {
            MissionManager.Instance.StartMission(missionIdToStartOnPickup);
        }

        if (!string.IsNullOrWhiteSpace(missionIdToCompleteOnPickup))
        {
            MissionManager.Instance.CompleteMission(missionIdToCompleteOnPickup);
        }

        if (!string.IsNullOrWhiteSpace(objectiveAfterPickup))
        {
            MissionManager.Instance.SetCurrentObjective(objectiveAfterPickup);
        }
    }

    private bool TryApplyItemEffect()
    {
        switch (itemData.effectType)
        {
            case ItemEffectType.None:
                return true;

            case ItemEffectType.RestoreOxygen:
                return TryRestorePlayerOxygen();

            // xử lý đèn pin
            case ItemEffectType.RestoreFlashlightBattery:
                return TryRestorePlayerFlashlightBattery();
            case ItemEffectType.UnlockFlashlight:
                return TryUnlockPlayerFlashlight();

            case ItemEffectType.UnlockMap:
            case ItemEffectType.UnlockMorseCode:
            case ItemEffectType.UnlockSecretDecree:
            case ItemEffectType.UnlockObjective:
            case ItemEffectType.CollectMapFragment:
                return true;

            default:
                Debug.LogWarning($"Unsupported item effect: {itemData.effectType}");
                return true;
        }
    }

    private bool TryRestorePlayerOxygen()
    {
        if (playerOxygen == null)
        {
            Debug.LogWarning("Cannot restore oxygen because AnOxygen was not found on Player.");
            return false;
        }

        return playerOxygen.TryRestoreOxygen(itemData.effectValue);
    }

    // xử lý đèn pin
    private bool TryUnlockPlayerFlashlight()
    {
        if (playerFlashlight == null)
        {
            Debug.LogWarning("Cannot unlock flashlight because AnFlashlight was not found on Player.");
            return false;
        }

        return playerFlashlight.UnlockFlashlight();
    }
    private void ShowFlashlightPickupPromptIfNeeded()
    {
        if (itemData == null)
        {
            return;
        }

        if (itemData.effectType != ItemEffectType.UnlockFlashlight)
        {
            return;
        }

        if (flashlightPickupPromptUI == null)
        {
            flashlightPickupPromptUI = Object.FindAnyObjectByType<FlashlightPickupPromptUI>(FindObjectsInactive.Include);
        }

        if (flashlightPickupPromptUI == null)
        {
            Debug.LogWarning("FlashlightPickupPromptUI was not found in scene.");
            return;
        }

        flashlightPickupPromptUI.ShowFlashlightPrompt(
            flashlightPickupPrompt,
            flashlightPickupPromptDuration);
    }
    private bool TryRestorePlayerFlashlightBattery()
    {
        if (playerFlashlight == null)
        {
            Debug.LogWarning("Cannot restore flashlight battery because AnFlashlight was not found on Player.");
            return false;
        }

        return playerFlashlight.TryRestoreBattery(itemData.effectValue);
    }

    private void FindPlayerReferencesIfNeeded()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found. Make sure An has Tag = Player.");
            return;
        }

        // xử lý đèn pin
        if (playerInventory == null)
        {
            playerInventory = player.GetComponent<AnInventory>();
        }

        if (playerOxygen == null)
        {
            playerOxygen = player.GetComponent<AnOxygen>();
        }

        if (playerFlashlight == null)
        {
            playerFlashlight = player.GetComponent<AnFlashlight>();
        }

        if (objectivePanelUI == null)
        {
            objectivePanelUI = Object.FindAnyObjectByType<ObjectivePanelUI>(FindObjectsInactive.Include);
        }
    }
}
