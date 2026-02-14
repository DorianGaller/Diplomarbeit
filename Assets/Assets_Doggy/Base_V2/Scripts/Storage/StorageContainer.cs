using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Haupt-Lager-System. Verwaltet alle Items und Statistiken.
/// Hänge dieses Script an dein Lager-GameObject in der Base.
/// </summary>
public class StorageContainer : MonoBehaviour
{
    [Header("Storage Settings")]
    [Tooltip("Name des Lagers (z.B. 'Hauptlager', 'Waffenkammer')")]
    public string storageName = "Base Storage";
    
    [Tooltip("Maximale Anzahl an Slots")]
    public int totalSlots = 50;
    
    [Header("Debug")]
    [Tooltip("Zeige Debug-Meldungen in der Console")]
    public bool showDebugLogs = true;

    // Das eigentliche Lager
    private List<ItemStack> storedItems = new List<ItemStack>();
    
    // Statistiken
    private Dictionary<ItemCategory, int> categoryStats = new Dictionary<ItemCategory, int>();
    private int totalTransactionsIn = 0;
    private int totalTransactionsOut = 0;
    
    // Events für UI Updates
    public delegate void StorageChanged();
    public event StorageChanged OnStorageChanged;

    void Awake()
    {
        InitializeStorage();
    }

    /// <summary>
    /// Initialisiert das Lager mit leeren Slots
    /// </summary>
    void InitializeStorage()
    {
        storedItems.Clear();
        
        for (int i = 0; i < totalSlots; i++)
        {
            storedItems.Add(null);
        }
        
        UpdateStatistics();
        
        if (showDebugLogs)
            Debug.Log($"[Storage] '{storageName}' initialized with {totalSlots} slots");
    }

    /// <summary>
    /// Fügt ein Item zum Lager hinzu
    /// </summary>
    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("[Storage] Tried to add null item!");
            return false;
        }

        int remainingQuantity = quantity;

        // Versuche erst in existierende Stacks zu packen
        for (int i = 0; i < storedItems.Count; i++)
        {
            if (storedItems[i] != null && 
                storedItems[i].itemData == item && 
                remainingQuantity > 0)
            {
                int added = storedItems[i].AddToStack(remainingQuantity);
                remainingQuantity -= added;
                
                if (showDebugLogs && added > 0)
                    Debug.Log($"[Storage] Added {added}x {item.itemName} to slot {i}");
            }
        }

        // Wenn noch Items übrig sind, fülle neue Slots
        while (remainingQuantity > 0)
        {
            int emptySlot = GetFirstEmptySlot();
            
            if (emptySlot == -1)
            {
                Debug.LogWarning($"[Storage] Not enough space! {remainingQuantity}x {item.itemName} could not be added");
                
                // Auch wenn nicht alles hinzugefügt wurde, updaten wir
                if (remainingQuantity < quantity)
                {
                    totalTransactionsIn++;
                    UpdateStatistics();
                    OnStorageChanged?.Invoke();
                }
                
                return false;
            }

            int amountForThisSlot = Mathf.Min(remainingQuantity, item.maxStackSize);
            storedItems[emptySlot] = new ItemStack(item, amountForThisSlot);
            remainingQuantity -= amountForThisSlot;
            
            if (showDebugLogs)
                Debug.Log($"[Storage] Added {amountForThisSlot}x {item.itemName} to new slot {emptySlot}");
        }

        totalTransactionsIn++;
        UpdateStatistics();
        OnStorageChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Entfernt Items aus einem bestimmten Slot
    /// </summary>
    public bool RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= storedItems.Count)
        {
            Debug.LogWarning($"[Storage] Invalid slot index: {slotIndex}");
            return false;
        }

        if (storedItems[slotIndex] == null)
        {
            Debug.LogWarning($"[Storage] Slot {slotIndex} is empty");
            return false;
        }

        if (storedItems[slotIndex].quantity < quantity)
        {
            Debug.LogWarning($"[Storage] Not enough items in slot {slotIndex}");
            return false;
        }

        string itemName = storedItems[slotIndex].itemData.itemName;
        storedItems[slotIndex].RemoveFromStack(quantity);

        if (storedItems[slotIndex].quantity <= 0)
        {
            storedItems[slotIndex] = null;
        }

        totalTransactionsOut++;
        UpdateStatistics();
        OnStorageChanged?.Invoke();
        
        if (showDebugLogs)
            Debug.Log($"[Storage] Removed {quantity}x {itemName} from slot {slotIndex}");
        
        return true;
    }

    /// <summary>
    /// Verschiebt Items zwischen zwei Slots
    /// </summary>
    public void MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot == toSlot) return;
        
        if (fromSlot < 0 || fromSlot >= storedItems.Count ||
            toSlot < 0 || toSlot >= storedItems.Count)
        {
            Debug.LogWarning("[Storage] Invalid slot indices for move");
            return;
        }

        // Tausche die Items
        ItemStack temp = storedItems[fromSlot];
        storedItems[fromSlot] = storedItems[toSlot];
        storedItems[toSlot] = temp;
        
        OnStorageChanged?.Invoke();
        
        if (showDebugLogs)
            Debug.Log($"[Storage] Moved item from slot {fromSlot} to {toSlot}");
    }

    /// <summary>
    /// Findet den ersten leeren Slot
    /// </summary>
    int GetFirstEmptySlot()
    {
        for (int i = 0; i < storedItems.Count; i++)
        {
            if (storedItems[i] == null) return i;
        }
        return -1;
    }

    /// <summary>
    /// Aktualisiert die Statistiken
    /// </summary>
    void UpdateStatistics()
    {
        categoryStats.Clear();
        
        foreach (var stack in storedItems)
        {
            if (stack != null && stack.itemData != null)
            {
                if (!categoryStats.ContainsKey(stack.itemData.category))
                {
                    categoryStats[stack.itemData.category] = 0;
                }
                categoryStats[stack.itemData.category] += stack.quantity;
            }
        }
    }

    // ===== PUBLIC GETTER METHODEN =====

    public Dictionary<ItemCategory, int> GetCategoryStats()
    {
        return new Dictionary<ItemCategory, int>(categoryStats);
    }

    public List<ItemStack> GetAllItems()
    {
        return storedItems;
    }

    public ItemStack GetItemAtSlot(int index)
    {
        if (index < 0 || index >= storedItems.Count) return null;
        return storedItems[index];
    }

    public int GetTotalItemCount()
    {
        return storedItems.Where(s => s != null).Sum(s => s.quantity);
    }

    public int GetOccupiedSlots()
    {
        return storedItems.Count(s => s != null);
    }

    public int GetEmptySlots()
    {
        return totalSlots - GetOccupiedSlots();
    }

    public int GetTotalTransactionsIn()
    {
        return totalTransactionsIn;
    }

    public int GetTotalTransactionsOut()
    {
        return totalTransactionsOut;
    }

    /// <summary>
    /// Leert das komplette Lager (für Tests)
    /// </summary>
    public void ClearStorage()
    {
        for (int i = 0; i < storedItems.Count; i++)
        {
            storedItems[i] = null;
        }
        
        totalTransactionsIn = 0;
        totalTransactionsOut = 0;
        
        UpdateStatistics();
        OnStorageChanged?.Invoke();
        
        if (showDebugLogs)
            Debug.Log("[Storage] Storage cleared");
    }
}
