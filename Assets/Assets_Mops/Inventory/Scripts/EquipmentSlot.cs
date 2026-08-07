using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IDraggableSlot
{
    //=======ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;

    //=======ITEM SLOT======//
    [SerializeField]
    private Image itemImage;

    //========EQUIPPED SLOTS=======//
    [SerializeField]
    private EquippedSlot headSlot, armsSlot, bodySlot, legsSlot, mainHandSlot, offHandSlot, relicSlot, feetSlot;

    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;
    private EquipmentSOLibary equipmentSOLibary;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibary>();
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {
        if (isFull)
            return quantity;

        this.itemType = itemType;

        this.itemName = itemName;
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;
        this.itemDescription = itemDescription;

        this.quantity = 1;
        isFull = true;
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick();

        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick();
    }

    public void OnLeftClick()
    {
        if (inventoryManager.chestOpen)
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            return;
        }

        if (isFull)
        {
            if (thisItemSelected)
            {
                EquipGear();
            }
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
        else
        {
            GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
        }
    }

    private void EquipGear()
    {
        if (itemType == ItemType.head)
            headSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.arms)
            armsSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.body)
            bodySlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.legs)
            legsSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.mainHand)
            mainHandSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.offHand)
            offHandSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.relic)
            relicSlot.EquipGear(itemSprite, itemName, itemDescription);
        else if (itemType == ItemType.feet)
            feetSlot.EquipGear(itemSprite, itemName, itemDescription);

        EmptySlot();
    }

    private void EmptySlot()
{
    itemImage.sprite = emptySprite;
    itemImage.SetAllDirty();   // NEU
    isFull = false;
    itemName = "";
    quantity = 0;
}

    public void OnRightClick()
    {
        if (inventoryManager.chestOpen) return;

        if (quantity <= 0) return;

        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.quantity = 1;
        newItem.itemName = itemName;
        newItem.sprite = itemSprite;
        newItem.itemDescription = itemDescription;

        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingOrder = 5;

        itemToDrop.AddComponent<BoxCollider2D>();
        itemToDrop.transform.position = GameObject.FindWithTag("Player").transform.position + new Vector3(2, 0, 0);
        itemToDrop.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        this.quantity -= 1;
        if (this.quantity <= 0)
            EmptySlot();
    }

    public int RemoveAmount(int amount)
    {
        if (amount <= 0 || quantity <= 0) return 0;

        EmptySlot();
        return 1;
    }

    // ── NEU: Drag & Drop ──────────────────────────────────

    public string GetItemName() => itemName;
    public int GetQuantity() => quantity;
    public Sprite GetItemSprite() => itemSprite;
    public string GetItemDescription() => itemDescription;
    public ItemType GetItemType() => itemType;
    public bool HasItem() => quantity > 0;

    public void SetSlotData(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.itemSprite = itemSprite;
        this.itemDescription = itemDescription;
        this.itemType = itemType;
        isFull = quantity > 0;

        itemImage.sprite = itemSprite;
        itemImage.SetAllDirty();   // NEU
    }

    public void ClearSlotData()
    {
        EmptySlot();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryManager.chestOpen) return;
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
        if (inventoryManager.chestOpen) return;

        IDraggableSlot source = DragDropManager.currentDragSource as IDraggableSlot;
        if (source == null || (object)source == this || !source.HasItem()) return;

        if (source.GetItemType() == ItemType.consumable) return;

        DragDropManager.SwapSlots(source, this);
        inventoryManager.DeselectAllSlots();
    }
}