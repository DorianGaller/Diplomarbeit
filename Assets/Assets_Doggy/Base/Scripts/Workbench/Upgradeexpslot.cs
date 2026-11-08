using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Einer der vier "Add Exp"-Slots. Nimmt ein Material samt SEINES GESAMTEN
/// STACKS an (wie vom Nutzer gefordert: "immer der ganze Stack").
///
/// Wie UpgradeGunSlot kein reiner Proxy — das Material wird tatsaechlich
/// hierher verschoben und beim Apply endgueltig verbraucht. Wird das Panel
/// stattdessen geschlossen, muss es zurueck ins Inventar (siehe WeaponUpgradeUI).
/// </summary>
public class UpgradeExpSlot : MonoBehaviour, IDraggableSlot, IDropHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text expPreviewText;
    [SerializeField] private Sprite emptySprite;

    [Tooltip("Damit der Apply-Button und die Anzeige nach jedem Drop reagieren.")]
    [SerializeField] private WeaponUpgradeUI upgradeUI;

    private string itemName;
    private int quantity;
    private Sprite itemSprite;
    private string itemDescription;

    public bool HasMaterial => quantity > 0;

    /// <summary>Wie viel EXP dieser Slot aktuell beitraegt.</summary>
    public int ExpValue => HasMaterial ? ItemDatabase.GetWeaponExpValue(itemName, quantity) : 0;

    private void Redraw()
    {
        if (itemImage != null)
        {
            itemImage.sprite = HasMaterial ? itemSprite : emptySprite;
            itemImage.SetAllDirty();
        }

        if (quantityText != null)
        {
            quantityText.text = quantity > 0 ? quantity.ToString() : "0";
        }

        if (expPreviewText != null)
        {
            int exp = ExpValue;
            expPreviewText.text = exp > 0 ? $"+{exp} EXP" : "";
        }
    }

    /// <summary>Leert den Slot ohne die Daten zurueckzugeben — nach dem Apply.</summary>
    public void Consume()
    {
        itemName = null;
        quantity = 0;
        itemSprite = null;
        itemDescription = null;
        Redraw();
    }

    // ── IDraggableSlot ────────────────────────────────────

    public string GetItemName() => itemName ?? "";
    public int GetQuantity() => quantity;
    public Sprite GetItemSprite() => itemSprite;
    public string GetItemDescription() => itemDescription ?? "";
    public ItemType GetItemType() => ItemType.material;
    public string GetInstanceId() => null; // Materialien haben keine Instanz-ID
    public bool HasItem() => HasMaterial;

    public void SetSlotData(string itemName, int quantity, Sprite itemSprite,
                            string itemDescription, ItemType itemType, string instanceId)
    {
        if (!InventoryManager.IsStackableType(itemType) || ItemDatabase.GetWeaponExpValue(itemName) <= 0)
        {
            Debug.LogWarning($"[UpgradeExpSlot] '{itemName}' taugt nicht als EXP-Material, wird abgelehnt.");
            return;
        }

        this.itemName = itemName;
        this.quantity = quantity;
        this.itemSprite = itemSprite;
        this.itemDescription = itemDescription;
        Redraw();
        upgradeUI?.OnExpSlotChanged();
    }

    public void ClearSlotData()
    {
        itemName = null;
        quantity = 0;
        itemSprite = null;
        itemDescription = null;
        Redraw();
        upgradeUI?.OnExpSlotChanged();
    }

    // ── Drop ──────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
        IDraggableSlot source = DragDropManager.currentDragSource as IDraggableSlot;
        if (source == null || (object)source == this || !source.HasItem()) return;

        // Nur Materialien mit echtem EXP-Wert. Beliebiges Material laut
        // Vorgabe erlaubt — aber 0-EXP-Items abzulehnen ist kein Widerspruch
        // dazu, nur ein sinnvoller Guard.
        if (ItemDatabase.GetWeaponExpValue(source.GetItemName()) <= 0) return;

        DragDropManager.SwapSlots(source, this);
    }
}