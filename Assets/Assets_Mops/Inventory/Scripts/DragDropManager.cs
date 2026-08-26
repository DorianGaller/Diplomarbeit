using UnityEngine;
using UnityEngine.UI;

public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance;

    [Tooltip("UI Image im Canvas, das während des Ziehens dem Mauszeiger folgt")]
    [SerializeField] private Image dragIcon;

    public static MonoBehaviour currentDragSource;

    void Awake()
    {
        Instance = this;
        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);
    }

    public void BeginDrag(MonoBehaviour source, Sprite sprite)
    {
        currentDragSource = source;
        if (dragIcon != null)
        {
            dragIcon.sprite = sprite;
            dragIcon.gameObject.SetActive(true);
        }
    }

    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (dragIcon != null)
            dragIcon.rectTransform.position = screenPosition;
    }

    public void EndDrag()
    {
        currentDragSource = null;
        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);
    }

    // Generischer Datentausch zwischen zwei Slots (für ItemSlot <-> ItemSlot, EquipmentSlot <-> EquipmentSlot, usw.)
    public static void SwapSlots(IDraggableSlot a, IDraggableSlot b)
    {
        string bName = b.GetItemName();
        int bQty = b.GetQuantity();
        Sprite bSprite = b.GetItemSprite();
        string bDesc = b.GetItemDescription();
        ItemType bType = b.GetItemType();
        string bId = b.GetInstanceId();
        bool bHadItem = b.HasItem();

        if (a.HasItem())
            b.SetSlotData(a.GetItemName(), a.GetQuantity(), a.GetItemSprite(),
                          a.GetItemDescription(), a.GetItemType(), a.GetInstanceId());
        else
            b.ClearSlotData();

        if (bHadItem)
            a.SetSlotData(bName, bQty, bSprite, bDesc, bType, bId);
        else
            a.ClearSlotData();
    }
}