using System.Collections.Generic;
using UnityEngine;

public class AnInventory : MonoBehaviour
{
    private readonly List<ItemData> collectedItems = new List<ItemData>();

    public bool HasSmallMap { get; private set; }

    // using morse code and secret letter
    public bool HasMorseCode { get; private set; }
    public bool HasSecretDecree { get; private set; }

    public bool HasFlashlight { get; private set; }
    public int MapFragmentCount { get; private set; }

    // xử lý đồng bộ sử dụng morse code và mật lệnh đồng thời thực hiện nhiệm vụ truyền tin
    public bool HasMorsePuzzleDocuments => HasMorseCode && HasSecretDecree;

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

    #region // mở khoá sau khi kết thúc ngày 1
    public void UnlockMorseCode()
    {
        HasMorseCode = true;
        Debug.Log("Unlocked document: Morse Code Guide");
    }

    public void UnlockSecretDecree()
    {
        HasSecretDecree = true;
        Debug.Log("Unlocked document: Secret Decree");
    }
    #endregion
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

            case ItemEffectType.UnlockSecretDecree:
                UnlockSecretDecree();
                break;

            case ItemEffectType.UnlockFlashlight:
                HasFlashlight = true;
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