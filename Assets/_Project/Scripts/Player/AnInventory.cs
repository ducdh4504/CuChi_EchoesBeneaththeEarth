using System;
using System.Collections.Generic;
using UnityEngine;

public class AnInventory : MonoBehaviour
{
    private readonly List<ItemData> collectedItems = new List<ItemData>();

    public bool HasSmallMap { get; private set; }

    // Documents
    public bool HasMorseCode { get; private set; }
    public bool HasSecretDecree { get; private set; }

    // Equipment
    public bool HasFlashlight { get; private set; }

    public int MapFragmentCount { get; private set; }

    // Dùng cho puzzle truyền tin sau này
    public bool HasMorsePuzzleDocuments => HasMorseCode && HasSecretDecree;

    public event Action<AnInventory> OnInventoryChanged;

    private void Awake()
    {
        LoadFromRuntimeState();
    }
    private void LoadFromRuntimeState()
    {
        HasSmallMap = RuntimeInventoryState.HasSmallMap;
        HasMorseCode = RuntimeInventoryState.HasMorseCode;
        HasSecretDecree = RuntimeInventoryState.HasSecretDecree;
        HasFlashlight = RuntimeInventoryState.HasFlashlight;
        MapFragmentCount = RuntimeInventoryState.MapFragmentCount;

        NotifyInventoryChanged();
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData is null. Cannot add item.");
            return;
        }

        if (!collectedItems.Contains(itemData))
        {
            collectedItems.Add(itemData);
        }

        bool changed = ApplyKeyItemEffect(itemData);

        Debug.Log($"Collected item: {itemData.itemName}");

        if (changed)
        {
            NotifyInventoryChanged();
        }
    }

public void SetHasMap(bool value)
    {
        HasSmallMap = value;
    }


    #region Unlock documents after Day 1 discovery

    public void UnlockMorseCode()
    {
        if (!SetMorseCodeUnlocked())
        {
            return;
        }

        RuntimeInventoryState.SetMorseCodeUnlocked();
        NotifyInventoryChanged();
    }

    public void UnlockSecretDecree()
    {
        if (!SetSecretDecreeUnlocked())
        {
            return;
        }

        RuntimeInventoryState.SetSecretDecreeUnlocked();
        NotifyInventoryChanged();
    }

    #endregion

    private bool ApplyKeyItemEffect(ItemData itemData)
    {
        switch (itemData.effectType)
        {
            case ItemEffectType.UnlockMap:
                if (HasSmallMap)
                {
                    return false;
                }

                HasSmallMap = true;
                RuntimeInventoryState.SetSmallMapUnlocked();
                return true;

            case ItemEffectType.UnlockMorseCode:
                //return SetMorseCodeUnlocked();
                bool morseChanged = SetMorseCodeUnlocked(); 
                if (morseChanged)
                {
                    RuntimeInventoryState.SetMorseCodeUnlocked();
                } 
                return morseChanged;

            case ItemEffectType.UnlockSecretDecree:
                //return SetSecretDecreeUnlocked();
                bool decreeChanged = SetSecretDecreeUnlocked();
                if (decreeChanged)
                {
                    RuntimeInventoryState.SetSecretDecreeUnlocked();
                }
                return decreeChanged;

            case ItemEffectType.UnlockFlashlight:
                if (HasFlashlight)
                {
                    return false;
                }

                HasFlashlight = true;
                RuntimeInventoryState.SetFlashlightUnlocked();
                Debug.Log("Unlocked equipment: Flashlight");
                return true;

            case ItemEffectType.CollectMapFragment:
                MapFragmentCount++;
                RuntimeInventoryState.AddMapFragment();
                return true;

            default:
                return false;
        }
    }

    private bool SetMorseCodeUnlocked()
    {
        if (HasMorseCode)
        {
            return false;
        }

        HasMorseCode = true;
        Debug.Log("Unlocked document: Morse Code Guide");
        return true;
    }

    private bool SetSecretDecreeUnlocked()
    {
        if (HasSecretDecree)
        {
            return false;
        }

        HasSecretDecree = true;
        Debug.Log("Unlocked document: Secret Decree");
        return true;
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke(this);
    }

    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        foreach (ItemData item in collectedItems)
        {
            if (item != null && item.itemId == itemId)
            {
                return true;
            }
        }

        return false;
    }
}
