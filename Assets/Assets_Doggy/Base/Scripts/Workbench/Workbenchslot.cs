using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Ein Slot im Workbench-Grid. Haelt SELBST KEINE DATEN — er ist ein Spiegel
/// eines echten Slots im InventoryCanvas.
///
/// Warum so: Das Inventar speichert seinen Zustand in den Slot-Komponenten.
/// Wuerde die Workbench eigene Kopien halten, muessten beide Seiten dauernd
/// synchron gehalten werden, und jede vergessene Stelle waere ein verlorenes
/// Item. Als Proxy gibt es weiterhin nur eine Wahrheit: den echten Slot.
///
/// Weil WorkbenchSlot IDraggableSlot implementiert, funktioniert der
/// bestehende DragDropManager ohne Aenderung — inklusive SwapSlots.
/// </summary>
public class WorkbenchSlot : MonoBehaviour,
    IDraggableSlot, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Refs")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Sprite emptySprite;

    private IDraggableSlot bound;
    private WorkbenchInventoryView view;

    /// <summary>Verbindet diesen Slot mit einem echten Inventar-Slot. null = leer und inaktiv.</summary>
    public void Bind(IDraggableSlot realSlot, WorkbenchInventoryView owner)
    {
        bound = realSlot;
        view = owner;
        Refresh();
    }

    /// <summary>Liest den gebundenen Slot neu aus und zeichnet die Anzeige.</summary>
    public void Refresh()
    {
        bool has = bound != null && bound.HasItem();

        if (itemImage != null)
        {
            itemImage.sprite = has ? bound.GetItemSprite() : emptySprite;
            itemImage.SetAllDirty();
        }

        if (quantityText != null)
        {
            // Einzelstuecke brauchen keine Mengenanzeige.
            int qty = has ? bound.GetQuantity() : 0;
            quantityText.text = qty > 1 ? qty.ToString() : "";
            quantityText.enabled = qty > 1;
        }
    }

    // ── IDraggableSlot: alles durchreichen ────────────────

    public string GetItemName() => bound != null ? bound.GetItemName() : "";
    public int GetQuantity() => bound != null ? bound.GetQuantity() : 0;
    public Sprite GetItemSprite() => bound != null ? bound.GetItemSprite() : null;
    public string GetItemDescription() => bound != null ? bound.GetItemDescription() : "";
    public ItemType GetItemType() => bound != null ? bound.GetItemType() : ItemType.none;
    public string GetInstanceId() => bound != null ? bound.GetInstanceId() : null;
    public bool HasItem() => bound != null && bound.HasItem();

    public void SetSlotData(string itemName, int quantity, Sprite itemSprite,
                            string itemDescription, ItemType itemType, string instanceId)
    {
        if (bound == null) return;

        bound.SetSlotData(itemName, quantity, itemSprite, itemDescription, itemType, instanceId);
        Refresh();
    }

    public void ClearSlotData()
    {
        if (bound == null) return;

        bound.ClearSlotData();
        Refresh();
    }

    // ── Drag & Drop ───────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;
        DragDropManager.Instance.BeginDrag(this, GetItemSprite());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((object)DragDropManager.currentDragSource == this)
            DragDropManager.Instance.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragDropManager.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (bound == null) return;

        IDraggableSlot source = DragDropManager.currentDragSource as IDraggableSlot;
        if (source == null || (object)source == this || !source.HasItem()) return;

        // Der aktive Tab entscheidet, was hier liegen darf.
        if (view != null && !view.AcceptsType(source.GetItemType())) return;

        DragDropManager.SwapSlots(source, this);

        // Beide Seiten neu zeichnen: der Quell-Slot kann auch im Grid liegen.
        if (view != null) view.RefreshAll();
        else Refresh();
    }
}