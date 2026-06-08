using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
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
            EquipGear();
        }
        else
        {
            inventoryManager.DeselectAllSlots();
            selectedShader.SetActive(true);
            thisItemSelected = true;

            // Description nur setzen wenn sie aktiv ist
            
        }
    }

    private void EquipGear()
    {
        if(itemType == ItemType.head)
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
    isFull = false;
    itemName = "";
    quantity = 0; // ✅ jetzt wird der Slot wieder als leer erkannt
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
        if (this.quantity <= 0)
            EmptySlot();
    }
}