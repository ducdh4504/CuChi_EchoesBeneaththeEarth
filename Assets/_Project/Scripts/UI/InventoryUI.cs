using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private AnInventory playerInventory;

    [Header("Slot Item Images")]
    [SerializeField] private Image morseSlotItemImage;        // Slot 1 > Item
    [SerializeField] private Image flashlightSlotItemImage;   // Slot 2 > Item
    [SerializeField] private Image secretDecreeSlotItemImage; // Slot 3 > Item
    [SerializeField] private Image mapSlotItemImage;          // Slot 4 > Item

    [Header("Icons")]
    [SerializeField] private Sprite morseIcon;
    [SerializeField] private Sprite flashlightIcon;
    [SerializeField] private Sprite secretDecreeIcon;
    [SerializeField] private Sprite mapIcon;

    private bool isSubscribed;

    private void Awake()
    {
        FindPlayerInventoryIfNeeded();
        SetupSlotImages();
        Refresh();
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void Start()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        FindPlayerInventoryIfNeeded();

        if (playerInventory == null)
        {
            return;
        }

        playerInventory.OnInventoryChanged += HandleInventoryChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || playerInventory == null)
        {
            isSubscribed = false;
            return;
        }

        playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        isSubscribed = false;
    }

    private void HandleInventoryChanged(AnInventory inventory)
    {
        Refresh();
    }

    private void Refresh()
    {
        FindPlayerInventoryIfNeeded();

        if (playerInventory == null)
        {
            SetSlotVisible(morseSlotItemImage, false);
            SetSlotVisible(flashlightSlotItemImage, false);
            SetSlotVisible(secretDecreeSlotItemImage, false);
            SetSlotVisible(mapSlotItemImage, false);
            return;
        }

        SetSlotVisible(morseSlotItemImage, playerInventory.HasMorseCode);
        SetSlotVisible(flashlightSlotItemImage, playerInventory.HasFlashlight);
        SetSlotVisible(secretDecreeSlotItemImage, playerInventory.HasSecretDecree);
        SetSlotVisible(mapSlotItemImage, playerInventory.HasSmallMap);
    }

    private void SetupSlotImages()
    {
        SetupSlotImage(morseSlotItemImage, morseIcon);
        SetupSlotImage(flashlightSlotItemImage, flashlightIcon);
        SetupSlotImage(secretDecreeSlotItemImage, secretDecreeIcon);
        SetupSlotImage(mapSlotItemImage, mapIcon);
    }

    private static void SetupSlotImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        if (sprite != null)
        {
            image.sprite = sprite;
        }

        image.preserveAspect = true;
        image.gameObject.SetActive(false);
    }

    private static void SetSlotVisible(Image image, bool visible)
    {
        if (image == null)
        {
            return;
        }

        image.gameObject.SetActive(visible);
    }

    private void FindPlayerInventoryIfNeeded()
    {
        if (playerInventory != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerInventory = player.GetComponent<AnInventory>();
        }

        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<AnInventory>();
        }
    }
}
