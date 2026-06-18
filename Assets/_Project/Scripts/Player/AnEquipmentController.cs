using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class AnEquipmentController : MonoBehaviour
{
    private readonly Dictionary<HeldItemSlot, IEquippableItem> itemsBySlot = new Dictionary<HeldItemSlot, IEquippableItem>();

    private IEquippableItem currentItem;

    public HeldItemSlot CurrentSlot { get; private set; } = HeldItemSlot.None;
    public event Action<HeldItemSlot> OnEquippedSlotChanged;

    private void Awake()
    {
        CacheEquippableItems();
    }

    public void HandleSlotPressed(HeldItemSlot slot)
    {
        if (slot == HeldItemSlot.None)
        {
            UnequipCurrent();
            return;
        }

        if (!itemsBySlot.TryGetValue(slot, out IEquippableItem item) || item == null)
        {
            Debug.Log($"No equippable item assigned to {slot}.");
            return;
        }

        if (!item.CanEquip())
        {
            Debug.Log($"Cannot equip {slot} yet.");
            return;
        }

        if (currentItem == item)
        {
            UnequipCurrent();
            return;
        }

        Equip(item);
    }

    public void RefreshItems()
    {
        CacheEquippableItems();
    }

    private void Equip(IEquippableItem item)
    {
        if (currentItem != null && currentItem != item)
        {
            currentItem.Unequip();
        }

        currentItem = item;
        CurrentSlot = item.Slot;
        currentItem.Equip();
        NotifyEquippedSlotChanged();
    }

    private void UnequipCurrent()
    {
        if (currentItem == null && CurrentSlot == HeldItemSlot.None)
        {
            return;
        }

        currentItem?.Unequip();
        currentItem = null;
        CurrentSlot = HeldItemSlot.None;
        NotifyEquippedSlotChanged();
    }

    private void CacheEquippableItems()
    {
        itemsBySlot.Clear();

        foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (!(behaviour is IEquippableItem item) || item.Slot == HeldItemSlot.None)
            {
                continue;
            }

            if (itemsBySlot.ContainsKey(item.Slot))
            {
                Debug.LogWarning($"Duplicate equippable slot {item.Slot} on {behaviour.name}. Keeping the first item.");
                continue;
            }

            itemsBySlot.Add(item.Slot, item);
        }
    }

    private void NotifyEquippedSlotChanged()
    {
        OnEquippedSlotChanged?.Invoke(CurrentSlot);
    }
}
