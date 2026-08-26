using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EquippedSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IDraggableSlot
{
    [SerializeField]
    private Image slotImage;

    [SerializeField]
    private TMP_Text slotName;

    [SerializeField]
    private Image playerDisplayImage;

    [SerializeField]
    private ItemType itemType = new ItemType();

    private Sprite itemSprite;
    private string itemName;
    private string itemDescription;
    private string instanceId;

    private InventoryManager inventoryManager;
    private EquipmentSOLibary equipmentSOLibary;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibary>();
    }

    private bool slotInUse;
    [SerializeField]
    public GameObject selectedShader;

    [SerializeField]
    public bool thisItemSelected;

    [SerializeField]
    private Sprite emptySprite;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick();

        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick();
    }

    void OnLeftClick()
    {
        if (thisItemSelected && slotInUse)
            UnEquipGear();

        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
            {
                if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
                    equipmentSOLibary.equipmentSO[i].PreviewEquipment();
            }
        }
    }

    void OnRightClick()
    {
        UnEquipGear();
    }

    // Überladung, damit bestehende Aufrufer weiter kompilieren
    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription)
        => EquipGear(itemSprite, itemName, itemDescription, null);

    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription, string instanceId)
    {
        if (slotInUse)
            UnEquipGear();

        this.instanceId = instanceId;
        this.itemSprite = itemSprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = false;

        this.itemName = itemName;
        this.itemDescription = itemDescription;

        if (playerDisplayImage != null)
            playerDisplayImage.sprite = this.itemSprite;

        for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
                equipmentSOLibary.equipmentSO[i].EquipItem();
        }

        slotInUse = true;

        // NEU: Vorschau-Panel schließen, da das Item jetzt wirklich ausgerüstet ist
        GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
    }

    public void UnEquipGear()
    {
        inventoryManager.DeselectAllSlots();
        inventoryManager.AddItem(itemName, 1, itemSprite, itemDescription, itemType, instanceId);

        this.itemSprite = emptySprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = true;

        if (playerDisplayImage != null)
            playerDisplayImage.sprite = emptySprite;

        for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
                equipmentSOLibary.equipmentSO[i].UnequipItem();
        }

        this.itemName = null;
        this.itemDescription = null;
        this.instanceId = null;

        GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
        slotInUse = false;
    }

    public void ClearEquippedItem()
    {
        inventoryManager.DeselectAllSlots();

        for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
                equipmentSOLibary.equipmentSO[i].UnequipItem();
        }

        this.itemSprite = emptySprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = true;

        if (playerDisplayImage != null)
            playerDisplayImage.sprite = emptySprite;

        this.itemName = null;
        this.itemDescription = null;
        this.instanceId = null;

        GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
        slotInUse = false;
    }

    // ── NEU: Drag & Drop ──────────────────────────────────

    public string GetItemName() => itemName;
    public int GetQuantity() => slotInUse ? 1 : 0;
    public Sprite GetItemSprite() => itemSprite;
    public string GetItemDescription() => itemDescription;
    public ItemType GetItemType() => itemType;
    public bool HasItem() => slotInUse;
    public string GetInstanceId() => instanceId;

    public void SetSlotData(string itemName, int quantity, Sprite itemSprite,
                            string itemDescription, ItemType itemType, string instanceId)
    {
        // Läuft bewusst über EquipGear(), damit Stats korrekt mit angewendet werden
        EquipGear(itemSprite, itemName, itemDescription, instanceId);
    }

    public void ClearSlotData()
    {
        if (slotInUse)
            ClearEquippedItem();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;
        DragDropManager.Instance.BeginDrag(this, itemSprite);
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
        IDraggableSlot source = DragDropManager.currentDragSource as IDraggableSlot;
        if (source == null || (object)source == this || !source.HasItem()) return;

        if (source.GetItemType() != itemType) return;   // falscher Ausrüstungsslot (z.B. Kopf-Item auf Bein-Slot)

        EquipGear(source.GetItemSprite(), source.GetItemName(),
                  source.GetItemDescription(), source.GetInstanceId());
        source.ClearSlotData();
        inventoryManager.DeselectAllSlots();
    }
}