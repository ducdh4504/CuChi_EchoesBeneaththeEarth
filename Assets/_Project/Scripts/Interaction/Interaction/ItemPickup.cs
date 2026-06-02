using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    [Header("Pickup Settings")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private bool allowTriggerPickupInput = true;

    private bool isPlayerNearby;
    private bool isPickedUp;

    private AnInventory playerInventory;
    private AnOxygen playerOxygen;

    private void Update()
    {
        // Cách 1: Player đứng trong trigger rồi nhấn E
        if (!allowTriggerPickupInput)
        {
            return;
        }

        if (!isPlayerNearby)
        {
            return;
        }

        if (Input.GetKeyDown(pickupKey))
        {
            PickupItem();
        }
    }

    // Cách 2: AnMovement nhấn E rồi gọi SendMessageUpwards("Interact")
    public void Interact()
    {
        if (!isPlayerNearby)
        {
            TryFindPlayerReferences();
        }

        PickupItem();
    }

    private void PickupItem()
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

        isPickedUp = true;

        ApplyItemEffect();

        if (playerInventory != null)
        {
            playerInventory.AddItem(itemData);
        }

        if (!string.IsNullOrEmpty(itemData.messageWhenCollected))
        {
            Debug.Log(itemData.messageWhenCollected);
        }

        if (itemData.destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyItemEffect()
    {
        switch (itemData.effectType)
        {
            case ItemEffectType.RestoreOxygen:
                if (playerOxygen != null)
                {
                    playerOxygen.RestoreOxygen(itemData.effectValue);
                }
                else
                {
                    Debug.LogWarning("Cannot restore oxygen because AnOxygen was not found on Player.");
                }
                break;

            //case ItemEffectType.RestoreHealth:
            //    Debug.Log("RestoreHealth effect is not implemented yet.");
            //    break;

            case ItemEffectType.RestoreLanternFuel:
                Debug.Log("RestoreLanternFuel effect is not implemented yet.");
                break;

            case ItemEffectType.UnlockMap:
            case ItemEffectType.UnlockMorseCode:
            case ItemEffectType.CollectMapFragment:
            case ItemEffectType.UnlockObjective:
                Debug.Log($"Key item collected: {itemData.itemName}");
                break;
        }
    }

    private void TryFindPlayerReferences()
    {
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
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerNearby = true;

        playerInventory = other.GetComponent<AnInventory>();
        playerOxygen = other.GetComponent<AnOxygen>();

        if (itemData != null)
        {
            Debug.Log($"Press {pickupKey} to pick up {itemData.itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerNearby = false;
        playerInventory = null;
        playerOxygen = null;
    }
}