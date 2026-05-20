using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChestSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Sprite emptySprite;

    private int slotIndex = -1;
    private ChestUI chestUI;
    private bool hasItem = false;

    public void SetItem(Chest.ChestItem item, int index, ChestUI ui)
    {
        slotIndex = index;
        chestUI = ui;
        hasItem = true;

        itemImage.sprite = item.itemSprite != null ? item.itemSprite : emptySprite;
        quantityText.text = item.quantity.ToString();
        quantityText.enabled = item.quantity > 1;
    }

    public void ClearSlot()
    {
        hasItem = false;
        slotIndex = -1;
        itemImage.sprite = emptySprite;
        quantityText.enabled = false;
    }

    public void UpdateQuantity(int quantity)
    {
        quantityText.text = quantity.ToString();
        quantityText.enabled = quantity > 1;
    }

    // Linksklick = Item ins Inventar nehmen
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasItem || chestUI == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            chestUI.TakeItem(slotIndex);
    }
}