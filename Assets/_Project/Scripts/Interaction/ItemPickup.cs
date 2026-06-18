using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

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
        // Nhặt đèn pin
        if (itemData.effectType == ItemEffectType.UnlockFlashlight)
        {
            return "Nhấn E để lấy đèn pin";
        }
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

        if (!string.IsNullOrWhiteSpace(itemData.messageWhenCollected))
        {
            Debug.Log(itemData.messageWhenCollected);
        }

        // xử lý đèn pin
        ShowFlashlightPickupPromptIfNeeded();
        if (objectivePanelUI != null && !string.IsNullOrWhiteSpace(itemData.objectiveWhenCollected))
        {
            objectivePanelUI.SetObjective(itemData.objectiveWhenCollected);
        }

        if (itemData.destroyAfterPickup)
        {
            Destroy(gameObject);
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

            // Có thể loại bỏ sau này - hiện tại giữ tránh bug
            //case ItemEffectType.RestoreLanternFuel:
            //    Debug.LogWarning("RestoreLanternFuel is deprecated. Using flashlight battery restore instead.");
            //    return TryRestorePlayerLanternFuel();

            // xử lý đèn pin
            case ItemEffectType.RestoreFlashlightBattery:
                return TryRestorePlayerFlashlightBattery();
            case ItemEffectType.UnlockFlashlight:
                return TryUnlockPlayerFlashlight();

            case ItemEffectType.UnlockMap:
            case ItemEffectType.UnlockMorseCode:
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

    //private bool TryRestorePlayerLanternFuel()
    //{
    //    if (playerLantern == null)
    //    {
    //        Debug.LogWarning("Cannot restore lantern fuel because AnLantern was not found on Player.");
    //        return false;
    //    }

    //    playerLantern.RestoreLight(itemData.effectValue);
    //    return true;
    //}

    private void FindPlayerReferencesIfNeeded()
    {
        //if (playerInventory != null && playerOxygen != null && playerLantern != null)
        //{
        //    return;
        //}

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

        //playerInventory = player.GetComponent<AnInventory>();
        //playerOxygen = player.GetComponent<AnOxygen>();
        //playerLantern = player.GetComponent<AnLantern>();
    }
}
