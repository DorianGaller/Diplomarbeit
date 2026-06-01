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
        //Check to see if the slot is already full
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
        //Update Quantity text

        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }

        if(eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
{
    if (thisItemSelected)
    {
        // Item benutzen
        inventoryManager.UseItem(itemName);
        this.quantity -= 1;
        quantityText.text = this.quantity.ToString();

        // isFull zurücksetzen, da jetzt Platz frei ist
        isFull = false;

        if (this.quantity <= 0)
        {
            EmptySlot();
        }
    }
    else
    {
        // Item auswählen
        inventoryManager.DeselectAllSlots();
        selectedShader.SetActive(true);
        thisItemSelected = true;

        ItemDescriptionNameText.text = itemName;
        ItemDescriptionText.text = itemDescription;
        itemDescriptionImage.sprite = itemSprite;

        if (itemDescriptionImage.sprite == null)
        {
            itemDescriptionImage.sprite = emptySprite;
        }
    }
}

private void EmptySlot()
{
    quantityText.enabled = false;
    itemImage.sprite = emptySprite;
    ItemDescriptionNameText.text = "";
    ItemDescriptionText.text = "";
    itemDescriptionImage.sprite = emptySprite;

    // Wichtig: Slot zurücksetzen!
    itemName = "";
    itemDescription = "";
    quantity = 0;
    isFull = false;
    thisItemSelected = false;
    selectedShader.SetActive(false);
}

    public void OnRightClick()
    {
    
    }
}
