using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IDraggableSlot
{
    //=======ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;
    public ItemType itemType;

    [SerializeField]
    private int maxNumberofItems;

    //=======ITEM SLOTS======//
    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private Image itemImage;

    //=======ITEM DESCRIPTION SLOT======//
    public Image itemDescriptionImage;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    public GameObject selectedShader;
    public bool thisItemSelected;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
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

        this.quantity += quantity;
        if (this.quantity >= maxNumberofItems)
        {
            quantityText.text = maxNumberofItems.ToString();
            quantityText.enabled = true;
            isFull = true;

            int extraItems = this.quantity - maxNumberofItems;
            this.quantity = maxNumberofItems;
            return extraItems;
        }

        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick();

        if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick();
    }

    void OnLeftClick()
    {
        if (inventoryManager.chestOpen)
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            return;
        }

        if (thisItemSelected)
        {
            if (itemType != ItemType.consumable) return;
            bool usable = inventoryManager.UseItem(itemName);
            if (usable)
            {
                this.quantity -= 1;
                quantityText.text = this.quantity.ToString();
                if (this.quantity <= 0)
                    EmptySlot();
            }
        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;

            inventoryManager.ShowItemPreview(itemName, itemDescription, itemSprite);
        }
    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        itemImage.sprite = emptySprite;
        isFull = false;
        itemName = "";
        itemSprite = null;         // NEU
        itemDescription = "";      // NEU
        itemType = default;        // NEU – zur Sicherheit

        if (thisItemSelected)
            inventoryManager.ClearItemPreview();

        thisItemSelected = false;
        if (selectedShader != null) selectedShader.SetActive(false);
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
        newItem.itemType = itemType; 

        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingOrder = 5;

        itemToDrop.AddComponent<BoxCollider2D>();
        itemToDrop.transform.position = GameObject.FindWithTag("Player").transform.position + new Vector3(2, 0, 0);
        itemToDrop.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        this.quantity -= 1;
        quantityText.text = this.quantity.ToString();
        if (this.quantity <= 0)
            EmptySlot();
    }

    public int RemoveAmount(int amount)
    {
        if (amount <= 0 || quantity <= 0) return 0;

        int toRemove = Mathf.Min(amount, quantity);
        quantity -= toRemove;

        if (quantity <= 0)
        {
            EmptySlot();
        }
        else
        {
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }

        return toRemove;
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
        isFull = quantity >= maxNumberofItems;

        itemImage.sprite = itemSprite;
        itemImage.SetAllDirty();   // NEU – erzwingt Neuzeichnen
        quantityText.text = quantity.ToString();
        quantityText.enabled = quantity > 0;
    }

    public void ClearSlotData()
    {
        EmptySlot();
        quantity = 0;
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

        if (!InventoryManager.IsStackableType(source.GetItemType())) return;

        DragDropManager.SwapSlots(source, this);
        inventoryManager.DeselectAllSlots();   // NEU
    }
}