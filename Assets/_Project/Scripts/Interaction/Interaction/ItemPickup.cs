using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    private bool isPickedUp;
    private AnInventory playerInventory;
    private AnOxygen playerOxygen;
    private AnLantern playerLantern;

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

        if (!string.IsNullOrWhiteSpace(itemData.messageWhenCollected))
        {
            Debug.Log(itemData.messageWhenCollected);
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

            case ItemEffectType.RestoreLanternFuel:
                return TryRestorePlayerLanternFuel();

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

    private bool TryRestorePlayerLanternFuel()
    {
        if (playerLantern == null)
        {
            Debug.LogWarning("Cannot restore lantern fuel because AnLantern was not found on Player.");
            return false;
        }

        playerLantern.RestoreLight(itemData.effectValue);
        return true;
    }

    private void FindPlayerReferencesIfNeeded()
    {
        if (playerInventory != null && playerOxygen != null && playerLantern != null)
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
        playerOxygen = player.GetComponent<AnOxygen>();
        playerLantern = player.GetComponent<AnLantern>();
    }
}
