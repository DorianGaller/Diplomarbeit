using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Repräsentiert einen einzelnen Slot im Storage UI
/// Unterstützt Drag & Drop zum Verschieben von Items
/// Hänge dieses Script an dein Slot-Prefab
/// </summary>
public class StorageSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components (Auto-Find oder Manuell)")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Image slotBackground;
    
    [Header("Tooltip (Optional)")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Header("Cyberpunk Styling")]
    public Color emptySlotColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    public Color filledSlotColor = new Color(0.15f, 0.3f, 0.4f, 0.9f);
    public Color highlightColor = new Color(0.2f, 0.5f, 0.6f, 1f);

    [Header("Drag Settings")]
    public float dragAlpha = 0.6f;

    private ItemStack currentItem;
    private int slotIndex;
    private Canvas canvas;
    private GameObject draggedObject;
    private StorageUI storageUIManager;
    private Color originalBackgroundColor;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        storageUIManager = GetComponentInParent<StorageUI>();

        // Auto-find Komponenten wenn nicht zugewiesen
        if (slotBackground == null)
            slotBackground = GetComponent<Image>();

        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform != null)
                itemIcon = iconTransform.GetComponent<Image>();
        }

        if (quantityText == null)
        {
            Transform textTransform = transform.Find("QuantityText");
            if (textTransform != null)
                quantityText = textTransform.GetComponent<TextMeshProUGUI>();
        }

        // Tooltip verstecken
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        originalBackgroundColor = slotBackground != null ? slotBackground.color : Color.white;
    }

    /// <summary>
    /// Setzt den Slot mit Item-Daten
    /// </summary>
    public void SetSlot(ItemStack item, int index)
    {
        currentItem = item;
        slotIndex = index;
        UpdateVisuals();
    }

    /// <summary>
    /// Aktualisiert die visuelle Darstellung
    /// </summary>
    void UpdateVisuals()
    {
        if (currentItem != null && currentItem.itemData != null)
        {
            // Slot hat Item
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.itemData.icon;
                itemIcon.enabled = true;
                itemIcon.color = Color.white;
            }

            if (quantityText != null)
            {
                quantityText.text = currentItem.quantity > 1 ? currentItem.quantity.ToString() : "";
            }

            if (slotBackground != null)
            {
                slotBackground.color = filledSlotColor;
            }
        }
        else
        {
            // Slot ist leer
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
            }

            if (slotBackground != null)
            {
                slotBackground.color = emptySlotColor;
            }
        }
    }

    // ===== DRAG & DROP SYSTEM =====

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || currentItem.itemData == null) return;

        // Erstelle visuelles Drag-Objekt
        draggedObject = new GameObject("DraggedItem");
        draggedObject.transform.SetParent(canvas.transform);
        draggedObject.transform.SetAsLastSibling(); // Immer on top

        Image dragImage = draggedObject.AddComponent<Image>();
        dragImage.sprite = currentItem.itemData.icon;
        dragImage.raycastTarget = false;
        
        // Semi-transparent während Drag
        Color dragColor = Color.white;
        dragColor.a = dragAlpha;
        dragImage.color = dragColor;

        // Größe anpassen
        RectTransform dragRect = draggedObject.GetComponent<RectTransform>();
        if (itemIcon != null)
        {
            dragRect.sizeDelta = itemIcon.rectTransform.sizeDelta;
        }

        draggedObject.transform.position = eventData.position;

        // Original Slot leicht transparent machen
        if (itemIcon != null)
        {
            Color iconColor = itemIcon.color;
            iconColor.a = 0.4f;
            itemIcon.color = iconColor;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedObject != null)
        {
            draggedObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy Drag-Objekt
        if (draggedObject != null)
        {
            Destroy(draggedObject);
        }

        // Original Slot zurücksetzen
        if (itemIcon != null)
        {
            Color iconColor = itemIcon.color;
            iconColor.a = 1f;
            itemIcon.color = iconColor;
        }

        // Finde Ziel-Slot
        GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
        if (targetObject == null) return;

        StorageSlotUI targetSlot = targetObject.GetComponent<StorageSlotUI>();
        
        // Falls wir auf den Hintergrund eines Slots geklickt haben
        if (targetSlot == null)
        {
            targetSlot = targetObject.GetComponentInParent<StorageSlotUI>();
        }

        if (targetSlot != null && targetSlot != this)
        {
            // Informiere Storage UI Manager über Move
            if (storageUIManager != null)
            {
                storageUIManager.MoveItemBetweenSlots(slotIndex, targetSlot.slotIndex);
            }
        }
    }

    // ===== TOOLTIP SYSTEM =====

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && currentItem.itemData != null)
        {
            ShowTooltip();
            
            // Highlight Slot
            if (slotBackground != null)
            {
                originalBackgroundColor = slotBackground.color;
                slotBackground.color = highlightColor;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
        
        // Reset Highlight
        if (slotBackground != null)
        {
            slotBackground.color = originalBackgroundColor;
        }
    }

    void ShowTooltip()
    {
        if (tooltipPanel != null && tooltipText != null && currentItem != null)
        {
            tooltipPanel.SetActive(true);
            
            string tooltip = $"<color=#00FFFF><b>{currentItem.itemData.itemName}</b></color>\n";
            tooltip += $"<color=#FFD700>Category:</color> {currentItem.itemData.category}\n";
            tooltip += $"<color=#FFFFFF>Quantity:</color> {currentItem.quantity}\n";
            
            if (!string.IsNullOrEmpty(currentItem.itemData.description))
            {
                tooltip += $"\n<color=#AAAAAA><i>{currentItem.itemData.description}</i></color>";
            }
            
            tooltipText.text = tooltip;
        }
    }

    void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    // ===== GETTER =====

    public int GetSlotIndex()
    {
        return slotIndex;
    }

    public ItemStack GetCurrentItem()
    {
        return currentItem;
    }
}
