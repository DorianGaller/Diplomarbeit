using UnityEngine;

/// <summary>
/// Test-Script zum einfachen Hinzufügen von Items ins Lager
/// Nützlich für Tests und Debugging
/// Hänge dies an ein beliebiges GameObject in der Szene
/// </summary>
public class TestItemAdder : MonoBehaviour
{
    [Header("Storage Reference")]
    [Tooltip("Ziehe hier dein Storage Container GameObject rein")]
    public StorageContainer targetStorage;

    [Header("Test Items")]
    [Tooltip("Items die du testen möchtest")]
    public ItemData[] testItems;

    [Header("Controls")]
    [Tooltip("Drücke diese Taste um ein zufälliges Test-Item hinzuzufügen")]
    public KeyCode addItemKey = KeyCode.T;
    
    [Tooltip("Drücke diese Taste um 10 zufällige Items hinzuzufügen")]
    public KeyCode addMultipleKey = KeyCode.Y;
    
    [Tooltip("Drücke diese Taste um das Lager zu leeren")]
    public KeyCode clearStorageKey = KeyCode.C;

    [Header("Settings")]
    [Tooltip("Menge die pro Tastendruck hinzugefügt wird")]
    public int quantityPerAdd = 1;
    
    [Tooltip("Zeige Meldungen in der Console")]
    public bool showDebugMessages = true;

    void Update()
    {
        // Einzelnes Item hinzufügen
        if (Input.GetKeyDown(addItemKey))
        {
            AddRandomItem();
        }

        // Mehrere Items hinzufügen
        if (Input.GetKeyDown(addMultipleKey))
        {
            for (int i = 0; i < 10; i++)
            {
                AddRandomItem();
            }
        }

        // Lager leeren
        if (Input.GetKeyDown(clearStorageKey))
        {
            ClearStorage();
        }
    }

    void AddRandomItem()
    {
        if (targetStorage == null)
        {
            Debug.LogWarning("[TestItemAdder] Kein Storage Container zugewiesen!");
            return;
        }

        if (testItems == null || testItems.Length == 0)
        {
            Debug.LogWarning("[TestItemAdder] Keine Test-Items zugewiesen!");
            return;
        }

        // Wähle zufälliges Item
        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
        
        if (randomItem == null)
        {
            Debug.LogWarning("[TestItemAdder] Test-Item ist null!");
            return;
        }

        // Füge zum Lager hinzu
        bool success = targetStorage.AddItem(randomItem, quantityPerAdd);

        if (showDebugMessages)
        {
            if (success)
            {
                Debug.Log($"[TestItemAdder] Added {quantityPerAdd}x {randomItem.itemName}");
            }
            else
            {
                Debug.LogWarning($"[TestItemAdder] Failed to add {randomItem.itemName} - storage might be full");
            }
        }
    }

    void ClearStorage()
    {
        if (targetStorage == null)
        {
            Debug.LogWarning("[TestItemAdder] Kein Storage Container zugewiesen!");
            return;
        }

        targetStorage.ClearStorage();

        if (showDebugMessages)
        {
            Debug.Log("[TestItemAdder] Storage cleared!");
        }
    }

    // Öffentliche Methoden für Buttons etc.
    public void AddSpecificItem(ItemData item, int quantity = 1)
    {
        if (targetStorage != null && item != null)
        {
            targetStorage.AddItem(item, quantity);
        }
    }

    public void AddRandomItemPublic()
    {
        AddRandomItem();
    }

    public void ClearStoragePublic()
    {
        ClearStorage();
    }
}
