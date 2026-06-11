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
    private ItemType itemType = new ItemType();

    private Sprite itemSprite;
    private string itemName;
    private string itemDescription;

    private InventoryManager inventoryManager;
    private EquipmentSOLibary equipmentSOLibary;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
        equipmentSOLibary = GameObject.Find("InventoryCanvas").GetComponent<EquipmentSOLibary>();
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

    public void EquipGear(Sprite itemSprite, string itemName, string itemDescription)
    {
        if (slotInUse)
            UnEquipGear();

        this.itemSprite = itemSprite;
        slotImage.sprite = this.itemSprite;
        slotName.enabled = false;

        this.itemName = itemName;
        this.itemDescription = itemDescription;

        // ← Null-Check damit es nicht crasht wenn nicht zugewiesen
        if (playerDisplayImage != null)
            playerDisplayImage.sprite = this.itemSprite;

        for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
        {
            if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
                equipmentSOLibary.equipmentSO[i].EquipItem();
        }

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

    //Reset the Display Image//
    if (playerDisplayImage != null)
        playerDisplayImage.sprite = emptySprite;

    //Update the Player Stats//  ← ERST Stats abziehen (itemName noch gesetzt)
    for (int i = 0; i < equipmentSOLibary.equipmentSO.Length; i++)
    {
        if (equipmentSOLibary.equipmentSO[i].itemName == itemName)
            equipmentSOLibary.equipmentSO[i].UnequipItem();
    }

    //Reset Data//  ← DANN erst nullen
    this.itemName = null;
    this.itemDescription = null;

    GameObject.Find("StatManager").GetComponent<PlayerStats>().TurnOffPreviewStats();
    slotInUse = false;
}


}
