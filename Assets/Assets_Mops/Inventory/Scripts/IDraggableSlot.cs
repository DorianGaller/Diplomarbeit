using UnityEngine;

public interface IDraggableSlot
{
    string GetItemName();
    int GetQuantity();
    Sprite GetItemSprite();
    string GetItemDescription();
    ItemType GetItemType();
    string GetInstanceId();
    bool HasItem();

    void SetSlotData(string itemName, int quantity, Sprite itemSprite,
                     string itemDescription, ItemType itemType, string instanceId);   // NEU: letzter Parameter
    void ClearSlotData();
}