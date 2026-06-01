using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //=======ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

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

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (isFull)
            return quantity;

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

    public void OnLeftClick()
    {
        // Im Truhen-Modus: nur auswählen, kein Benutzen, keine Description
        if (inventoryManager.chestOpen)
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;
            return;
        }

        if (thisItemSelected)
        {
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

            // Description nur setzen wenn sie aktiv ist
            if (ItemDescriptionNameText != null) ItemDescriptionNameText.text = itemName;
            if (ItemDescriptionText != null)     ItemDescriptionText.text = itemDescription;
            if (itemDescriptionImage != null)
            {
                itemDescriptionImage.sprite = itemSprite != null ? itemSprite : emptySprite;
            }
        }
    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        itemImage.sprite = emptySprite;
        isFull = false;
        itemName = "";

        if (ItemDescriptionNameText != null) ItemDescriptionNameText.text = "";
        if (ItemDescriptionText != null)     ItemDescriptionText.text = "";
        if (itemDescriptionImage != null)    itemDescriptionImage.sprite = emptySprite;
    }

    public void OnRightClick()
    {
        // Im Truhen-Modus kein Droppen
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
        quantityText.text = this.quantity.ToString();
        if (this.quantity <= 0)
            EmptySlot();
    }
}