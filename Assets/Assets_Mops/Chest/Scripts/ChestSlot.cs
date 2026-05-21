using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestSlot : MonoBehaviour, IPointerClickHandler
{
    //=======ITEM DATA======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public string itemDescription;
    public Sprite emptySprite;

    //=======SLOT UI======//
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;

    public GameObject selectedShader;
    public bool thisItemSelected;

    private int slotIndex = -1;
    private ChestUI chestUI;
    private bool hasItem = false;

    public void SetItem(Chest.ChestItem item, int index, ChestUI ui)
    {
        slotIndex       = index;
        chestUI         = ui;
        hasItem         = true;
        itemName        = item.itemName;
        quantity        = item.quantity;
        itemSprite      = item.itemSprite;
        itemDescription = item.itemDescription;

        itemImage.sprite     = itemSprite != null ? itemSprite : emptySprite;
        quantityText.text    = quantity.ToString();
        quantityText.enabled = quantity > 0;
    }

    public void ClearSlot()
    {
        hasItem = false;
        slotIndex = -1;
        itemName = "";
        quantity = 0;
        itemSprite = null;
        itemDescription = "";

        if (emptySprite != null) itemImage.sprite = emptySprite;
        quantityText.enabled = false;
        if (selectedShader != null) selectedShader.SetActive(false);
        thisItemSelected = false;
    }

    public void UpdateQuantity(int newQuantity)
    {
        quantity             = newQuantity;
        quantityText.text    = newQuantity.ToString();
        quantityText.enabled = newQuantity > 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasItem || chestUI == null) return;

        // Linksklick = 1 Stück ins Inventar nehmen
        if (eventData.button == PointerEventData.InputButton.Left)
            chestUI.TakeOneItem(slotIndex);

        // Rechtsklick = ganzen Stack ins Inventar nehmen
        if (eventData.button == PointerEventData.InputButton.Right)
            chestUI.TakeItem(slotIndex);
    }
}