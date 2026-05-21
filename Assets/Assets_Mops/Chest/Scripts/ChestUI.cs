using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    [Header("Chest Slots")]
    public ChestSlot[] chestSlots;

    [Header("Buttons")]
    public Button closeButton;
    public Button takeAllButton;

    private Chest currentChest;
    private InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();

        // Slots automatisch aus Kindobjekten holen falls nicht manuell zugewiesen
        if (chestSlots == null || chestSlots.Length == 0)
            chestSlots = GetComponentsInChildren<ChestSlot>(true);
    }

    void Start()
    {
        if (closeButton   != null) closeButton.onClick.AddListener(Close);
        if (takeAllButton != null) takeAllButton.onClick.AddListener(TakeAll);
    }

    public void LoadAndOpen(Chest chest)
    {
        currentChest = chest;

        for (int i = 0; i < chestSlots.Length; i++)
            chestSlots[i].ClearSlot();

        Chest.ChestItem[] items = chest.chestItems;
        if (items == null) return;

        for (int i = 0; i < items.Length && i < chestSlots.Length; i++)
        {
            if (items[i] != null && items[i].quantity > 0)
                chestSlots[i].SetItem(items[i], i, this);
        }

        inventoryManager.OpenChestView();
    }

    // Rechtsklick: ganzen Stack nehmen
    public void TakeItem(int slotIndex)
    {
        if (currentChest == null) return;

        Chest.ChestItem item = currentChest.chestItems[slotIndex];
        if (item.quantity <= 0) return;

        int leftOver = inventoryManager.AddItem(
            item.itemName, item.quantity, item.itemSprite, item.itemDescription);

        if (leftOver <= 0)
        {
            currentChest.RemoveItem(slotIndex);
            chestSlots[slotIndex].ClearSlot();
        }
        else
        {
            currentChest.chestItems[slotIndex].quantity = leftOver;
            chestSlots[slotIndex].UpdateQuantity(leftOver);
        }
    }

    // Linksklick: 1 Stück nehmen
    public void TakeOneItem(int slotIndex)
    {
        if (currentChest == null) return;

        Chest.ChestItem item = currentChest.chestItems[slotIndex];
        if (item.quantity <= 0) return;

        int leftOver = inventoryManager.AddItem(
            item.itemName, 1, item.itemSprite, item.itemDescription);

        if (leftOver <= 0)
        {
            item.quantity -= 1;
            if (item.quantity <= 0)
            {
                currentChest.RemoveItem(slotIndex);
                chestSlots[slotIndex].ClearSlot();
            }
            else
            {
                chestSlots[slotIndex].UpdateQuantity(item.quantity);
            }
        }
    }

    public void TakeAll()
    {
        if (currentChest == null) return;
        for (int i = 0; i < currentChest.chestItems.Length; i++)
            TakeItem(i);
    }

    public void Close()
    {
        if (currentChest != null)
            currentChest.CloseChest();
    }
}