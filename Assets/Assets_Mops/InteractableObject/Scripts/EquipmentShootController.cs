using UnityEngine;
using System.Reflection;

/// <summary>
/// Füge diesen Script einem beliebigen GameObject hinzu (z.B. GameManager).
/// Er sucht automatisch den Main Character und aktiviert/deaktiviert canShoot
/// je nachdem ob ein bestimmtes Item in einem bestimmten EquippedSlot steckt.
/// </summary>
public class EquipmentShootController : MonoBehaviour
{
    [Header("Item Requirement")]
    [Tooltip("Name des Items das canShoot aktiviert (muss mit itemName in EquippedSlot übereinstimmen)")]
    [SerializeField] private string requiredItemName = "Pistol";

    [Header("Slot Reference")]
    [Tooltip("Der EquippedSlot der geprüft wird (z.B. MainHand-Slot aus InventoryCanvas)")]
    [SerializeField] private EquippedSlot targetSlot;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private InteractKeys interactKeys;
    private FieldInfo itemNameField;
    private string lastEquippedName = null;

    private void Start()
    {
        // Main Character per Tag finden
        GameObject mainCharacter = GameObject.FindWithTag("Player");
        if (mainCharacter == null)
        {
            Debug.LogError("EquipmentShootController: Kein GameObject mit Tag 'Player' gefunden!");
            return;
        }

        interactKeys = mainCharacter.GetComponent<InteractKeys>();
        if (interactKeys == null)
        {
            Debug.LogError("EquipmentShootController: InteractKeys nicht auf dem Player gefunden!");
            return;
        }

        // Reflection-Cache: einmal holen, nicht jeden Frame
        itemNameField = typeof(EquippedSlot).GetField(
            "itemName",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (itemNameField == null)
        {
            Debug.LogError("EquipmentShootController: Feld 'itemName' in EquippedSlot nicht gefunden!");
            return;
        }

        if (targetSlot == null)
            Debug.LogWarning("EquipmentShootController: Kein targetSlot im Inspector gesetzt!");

        // Initialen Zustand setzen
        UpdateShootState();
    }

    private void Update()
    {
        if (interactKeys == null || targetSlot == null || itemNameField == null) return;

        string currentName = itemNameField.GetValue(targetSlot) as string;

        // Nur updaten wenn sich was geändert hat (Performance)
        if (currentName == lastEquippedName) return;

        lastEquippedName = currentName;
        UpdateShootState();
    }

    private void UpdateShootState()
    {
        if (interactKeys == null) return;

        bool hasItem = lastEquippedName == requiredItemName;
        interactKeys.canShoot = hasItem;

        if (showDebugLog)
            Debug.Log($"EquipmentShootController: canShoot = {hasItem} " +
                      $"(Slot hat: '{lastEquippedName ?? "leer"}', benötigt: '{requiredItemName}')");
    }
}