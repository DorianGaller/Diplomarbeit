using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Das Inventar-Grid im Workbench-Canvas. Zeigt dieselben Daten wie das
/// echte Inventar, indem es jeden WorkbenchSlot an einen echten Slot bindet.
///
/// Zwei Tabs, passend zu den beiden Arrays im InventoryManager:
///   Inventory  -> itemSlot[]       (stapelbar: consumable, material)
///   Equipment  -> equipmentSlot[]  (Einzelstuecke: Waffen, Ruestung)
/// </summary>
public class WorkbenchInventoryView : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Leer lassen — wird ueber GameObject.Find('InventoryCanvas') gesucht.")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Die Slot-Objekte des Grids, in Lesereihenfolge.")]
    [SerializeField] private WorkbenchSlot[] slots;

    [Header("Tabs")]
    [SerializeField] private Button inventoryTabButton;
    [SerializeField] private Button equipmentTabButton;

    [Tooltip("Optional: Rahmen o.ae., der den aktiven Tab markiert.")]
    [SerializeField] private GameObject inventoryTabHighlight;
    [SerializeField] private GameObject equipmentTabHighlight;

    private bool showingEquipment;

    private void Awake()
    {
        if (inventoryManager == null)
        {
            var canvas = GameObject.Find("InventoryCanvas");
            if (canvas != null)
                inventoryManager = canvas.GetComponent<InventoryManager>();
        }

        if (inventoryManager == null)
            Debug.LogError("[Workbench] Kein InventoryManager gefunden.");

        if (inventoryTabButton != null)
            inventoryTabButton.onClick.AddListener(ShowInventory);

        if (equipmentTabButton != null)
            equipmentTabButton.onClick.AddListener(ShowEquipment);
    }

    private void OnEnable()
    {
        // Beim Oeffnen immer frisch binden — im Inventar kann sich
        // zwischenzeitlich alles geaendert haben.
        Rebind();
    }

    // ── Tabs ──────────────────────────────────────────────

    public void ShowInventory()
    {
        showingEquipment = false;
        Rebind();
    }

    public void ShowEquipment()
    {
        showingEquipment = true;
        Rebind();
    }

    /// <summary>Welche Item-Typen im aktuellen Tab abgelegt werden duerfen.</summary>
    public bool AcceptsType(ItemType type)
    {
        return showingEquipment
            ? !InventoryManager.IsStackableType(type)
            : InventoryManager.IsStackableType(type);
    }

    // ── Binden und Zeichnen ───────────────────────────────

    private void Rebind()
    {
        if (inventoryManager == null || slots == null) return;

        int count = showingEquipment
            ? inventoryManager.equipmentSlot.Length
            : inventoryManager.itemSlot.Length;

        if (slots.Length < count)
        {
            Debug.LogWarning(
                $"[Workbench] Grid hat {slots.Length} Slots, das Inventar aber {count}. " +
                "Die hinteren Items sind hier nicht erreichbar.");
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            IDraggableSlot real = null;

            if (i < count)
            {
                real = showingEquipment
                    ? (IDraggableSlot)inventoryManager.equipmentSlot[i]
                    : (IDraggableSlot)inventoryManager.itemSlot[i];
            }

            slots[i].Bind(real, this);
        }

        UpdateTabHighlight();
    }

    /// <summary>
    /// Zeichnet alle Slots neu, ohne die Bindung anzufassen.
    /// Nach jedem Drop und nach jeder Aktion im Upgrade-Panel aufrufen.
    /// </summary>
    public void RefreshAll()
    {
        if (slots == null) return;

        foreach (var slot in slots)
            if (slot != null) slot.Refresh();
    }

    private void UpdateTabHighlight()
    {
        if (inventoryTabHighlight != null)
            inventoryTabHighlight.SetActive(!showingEquipment);

        if (equipmentTabHighlight != null)
            equipmentTabHighlight.SetActive(showingEquipment);
    }
}