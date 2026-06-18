using UnityEngine;

public class SimpleEquippableItem : MonoBehaviour, IEquippableItem
{
    [SerializeField] private HeldItemSlot slot = HeldItemSlot.Slot1;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private GameObject visualRoot;

    public HeldItemSlot Slot => slot;
    public bool IsUnlocked => isUnlocked;
    public bool IsEquipped { get; private set; }

    private void Awake()
    {
        ApplyVisualState();
    }

    public bool CanEquip()
    {
        return isUnlocked;
    }

    public void Unlock()
    {
        isUnlocked = true;
    }

    public void Equip()
    {
        IsEquipped = true;
        ApplyVisualState();
    }

    public void Unequip()
    {
        IsEquipped = false;
        ApplyVisualState();
    }

    public bool UseEquipped()
    {
        Unequip();
        return IsEquipped;
    }

    private void ApplyVisualState()
    {
        GameObject target = visualRoot != null ? visualRoot : gameObject;
        target.SetActive(isUnlocked && IsEquipped);
    }
}
