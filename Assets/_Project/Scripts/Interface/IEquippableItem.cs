public interface IEquippableItem
{
    HeldItemSlot Slot { get; }
    bool IsUnlocked { get; }
    bool IsEquipped { get; }

    bool CanEquip();
    void Equip();
    void Unequip();
    bool UseEquipped();
}
