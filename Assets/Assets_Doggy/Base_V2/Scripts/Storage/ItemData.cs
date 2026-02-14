using UnityEngine;

/// <summary>
/// ScriptableObject für Item Daten.
/// Erstelle neue Items: Rechtsklick in Assets -> Create -> Inventory -> Item
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "New Item";
    public string itemID = "item_000"; // Eindeutige ID
    
    [Header("Visuals")]
    public Sprite icon;
    
    [Header("Category & Stacking")]
    public ItemCategory category = ItemCategory.Resource;
    public int maxStackSize = 99;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "Item description here...";
}

/// <summary>
/// Item Kategorien - Einfach hier neue Kategorien hinzufügen
/// </summary>
public enum ItemCategory
{
    Resource,
    Weapon,
    Consumable,
    QuestItem,
    CyberwearePart,
    Ammo,
    CraftingMaterial,
    Junk
}
