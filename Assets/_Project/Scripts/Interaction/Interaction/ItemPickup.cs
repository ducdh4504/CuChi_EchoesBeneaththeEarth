using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [Header("Prompt")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    private bool isPickedUp;
    private AnInventory playerInventory;
    private AnOxygen playerOxygen;

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
                Debug.LogWarning("RestoreLanternFuel effect is not implemented yet.");
                return true;

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

    private void FindPlayerReferencesIfNeeded()
    {
        if (playerInventory != null && playerOxygen != null)
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
    }

    private void OnTriggerEnter(Collider other)
    {
        AnOxygen oxygen = other.GetComponentInParent<AnOxygen>();
        if (oxygen == null)
        {
            return;
        }

        playerOxygen = oxygen;
        playerInventory = other.GetComponentInParent<AnInventory>();

        if (itemData != null)
        {
            Debug.Log($"Press {pickupKey} to pick up {itemData.itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<AnOxygen>() != playerOxygen)
        {
            return;
        }

        playerInventory = null;
        playerOxygen = null;
    }
}
