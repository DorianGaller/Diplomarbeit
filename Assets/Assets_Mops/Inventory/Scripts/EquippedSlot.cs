using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EquippedSlot : MonoBehaviour, IPointerClickHandler
{
    //SLOT APPEARANCE//
    [SerializeField]
    private Image slotImage;

    [SerializeField]
    private TMP_Text slotName;

    [SerializeField]
    private Image playerDisplayImage;

    //SLOT DATA//
    [SerializeField]
    private  ItemType itemType = new ItemType();

    private Sprite itemSprite;
    private string itemName;
    private string itemDescription;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    //OTHER VARIABLES//
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
        }
    }

    void OnRightClick()
    {
        UnEquipGear();
    }

    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription)
    {

        if (slotInUse)
            UnEquipGear();
        

        //Update Image//
        this.itemSprite = itemSprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = false;

        //Update Data//
        this.itemName = itemName;
        this.itemDescription = itemDescription;

        //Update the Display Image//
        playerDisplayImage.sprite = this.itemSprite;

        slotInUse = true;

    }

    public void UnEquipGear()
    {
        inventoryManager.DeselectAllSlots();

        inventoryManager.AddItem(itemName, 1, itemSprite, itemDescription, itemType);

        //Update Image//
        this.itemSprite = emptySprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = true;

        //Reset Data//
        this.itemName = null;
        this.itemDescription = null;

        //Reset the Display Image//
        playerDisplayImage.sprite = emptySprite;

        slotInUse = false;
    }


}
