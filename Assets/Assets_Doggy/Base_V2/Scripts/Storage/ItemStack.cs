using UnityEngine;

/// <summary>
/// Repräsentiert einen Stack von Items im Lager/Inventar
/// </summary>
[System.Serializable]
public class ItemStack
{
    public ItemData itemData;
    public int quantity;

    public ItemStack(ItemData data, int qty = 1)
    {
        itemData = data;
        quantity = Mathf.Max(1, qty);
    }

    /// <summary>
    /// Prüft ob weitere Items zum Stack hinzugefügt werden können
    /// </summary>
    public bool CanAddToStack(int amount)
    {
        if (itemData == null) return false;
        return quantity + amount <= itemData.maxStackSize;
    }

    /// <summary>
    /// Fügt Items zum Stack hinzu
    /// </summary>
    public int AddToStack(int amount)
    {
        if (itemData == null) return 0;
        
        int spaceAvailable = itemData.maxStackSize - quantity;
        int amountToAdd = Mathf.Min(amount, spaceAvailable);
        
        quantity += amountToAdd;
        return amountToAdd;
    }

    /// <summary>
    /// Entfernt Items vom Stack
    /// </summary>
    public bool RemoveFromStack(int amount)
    {
        if (quantity < amount) return false;
        
        quantity -= amount;
        return true;
    }

    /// <summary>
    /// Erstellt eine Kopie des Stacks
    /// </summary>
    public ItemStack Clone()
    {
        return new ItemStack(itemData, quantity);
    }
}
