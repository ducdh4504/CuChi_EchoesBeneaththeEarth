using System.Collections.Generic;
using UnityEngine;

public class AnInventory : MonoBehaviour
{
    private readonly List<ItemData> collectedItems = new List<ItemData>();

    public bool HasSmallMap { get; private set; }
    public bool HasMorseCode { get; private set; }
    public int MapFragmentCount { get; private set; }

    public void AddItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData is null. Cannot add item.");
            return;
        }

        collectedItems.Add(itemData);

        ApplyKeyItemEffect(itemData);

        Debug.Log($"Collected item: {itemData.itemName}");
    }

    private void ApplyKeyItemEffect(ItemData itemData)
    {
        switch (itemData.effectType)
        {
            case ItemEffectType.UnlockMap:
                HasSmallMap = true;
                break;

            case ItemEffectType.UnlockMorseCode:
                HasMorseCode = true;
                break;

            case ItemEffectType.CollectMapFragment:
                MapFragmentCount++;
                break;
        }
    }

    public bool HasItem(string itemId)
    {
        foreach (ItemData item in collectedItems)
        {
            if (item.itemId == itemId)
            {
                return true;
            }
        }

        return false;
    }
}
