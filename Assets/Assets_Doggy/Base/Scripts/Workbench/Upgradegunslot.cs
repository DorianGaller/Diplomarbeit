using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Das "Add Gun to Upgrade Stats"-Feld. Nimmt per Drop genau eine Waffe an
/// und haelt sie dort, solange das Panel offen ist.
///
/// Anders als WorkbenchSlot ist das kein reiner Proxy: die Waffe WIRD
/// tatsaechlich hierher verschoben (SwapSlots mit dem Quell-Slot), damit
/// sie im Inventar sichtbar fehlt, waehrend sie im Panel liegt — wie im
/// Mockup, wo der Inventar-Slot leer wird sobald die Waffe im Panel steckt.
///
/// Beim Schliessen des Panels MUSS die Waffe zurueckgegeben werden
/// (siehe WeaponUpgradeUI.OnDisable) — sonst verschwindet sie schlicht.
/// </summary>
public class UpgradeGunSlot : MonoBehaviour, IDraggableSlot, IDropHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySprite;

    [Tooltip("Wird informiert, wenn eine Waffe reingezogen oder entfernt wird.")]
    [SerializeField] private WeaponUpgradeUI upgradeUI;

    private string itemName;
    private string instanceId;
    private Sprite itemSprite;
    private string itemDescription;

    public bool HasWeapon => !string.IsNullOrEmpty(instanceId);

    /// <summary>Direkt von WeaponUpgradeUI genutzt, um die Waffe ohne Drag reinzusetzen.</summary>
    public void SetWeapon(string name, string id, Sprite sprite, string description)
    {
        itemName = name;
        instanceId = id;
        itemSprite = sprite;
        itemDescription = description;
        Redraw();
    }

    public void ClearWeapon()
    {
        itemName = null;
        instanceId = null;
        itemSprite = null;
        itemDescription = null;
        Redraw();
    }

    private void Redraw()
    {
        if (itemImage == null) return;

        itemImage.sprite = HasWeapon ? itemSprite : emptySprite;
        itemImage.SetAllDirty();
    }

    // ── IDraggableSlot ────────────────────────────────────

    public string GetItemName() => itemName ?? "";
    public int GetQuantity() => HasWeapon ? 1 : 0;
    public Sprite GetItemSprite() => itemSprite;
    public string GetItemDescription() => itemDescription ?? "";
    public ItemType GetItemType() => ItemType.mainHand;
    public string GetInstanceId() => instanceId;
    public bool HasItem() => HasWeapon;

    public void SetSlotData(string itemName, int quantity, Sprite itemSprite,
                            string itemDescription, ItemType itemType, string instanceId)
    {
        // Sicherheitsnetz: sollte durch OnDrop-Guard eigentlich nie mit
        // einer Nicht-Waffe aufgerufen werden.
        if (!ItemDatabase.IsWeapon(itemName))
        {
            Debug.LogWarning($"[UpgradeGunSlot] '{itemName}' ist keine Waffe, wird abgelehnt.");
            return;
        }

        SetWeapon(itemName, instanceId, itemSprite, itemDescription);
        upgradeUI?.OnWeaponDropped(instanceId);
    }

    public void ClearSlotData()
    {
        ClearWeapon();
        upgradeUI?.OnWeaponRemoved();
    }

    // ── Drop ──────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
        IDraggableSlot source = DragDropManager.currentDragSource as IDraggableSlot;
        if (source == null || (object)source == this || !source.HasItem()) return;

        if (!ItemDatabase.IsWeapon(source.GetItemName())) return;
        if (string.IsNullOrEmpty(source.GetInstanceId())) return; // Altbestand ohne Instanz

        // Bereits eine Waffe im Panel? Erst zurueck ins Inventar, dann tauschen.
        if (HasWeapon)
            upgradeUI?.ReturnCurrentWeaponToInventory();

        DragDropManager.SwapSlots(source, this);
    }
}