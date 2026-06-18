using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "CuChi/Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string itemName;

    [TextArea(3, 5)]
    public string description;

    public Sprite icon;

    [Header("Item Type")]
    public ItemType itemType;
    public ItemEffectType effectType;

    [Header("Effect")]
    public float effectValue;

    [Header("Story / Objective")]
    public string objectiveId;

    [TextArea(2, 4)]
    public string objectiveWhenCollected;

    [TextArea(2, 4)]
    public string messageWhenCollected;

    [Header("Pickup Settings")]
    public bool destroyAfterPickup = true;
}