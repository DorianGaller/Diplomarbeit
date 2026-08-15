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

    public ItemType itemType;

    private bool initialized = false;   // NEU

    void Awake()
    {
        EnsureInitialized();
    }

    void Start()
    {
        EnsureInitialized();
    }

    // NEU: fasst Awake+Start zusammen und ist beliebig oft sicher aufrufbar,
    // egal ob das GameObject beim Szenenstart aktiv war oder nicht
    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        if (inventoryManager == null)
            inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();

        if (chestSlots == null || chestSlots.Length == 0)
            chestSlots = GetComponentsInChildren<ChestSlot>(true);

        if (closeButton   != null) closeButton.onClick.AddListener(Close);
        if (takeAllButton != null) takeAllButton.onClick.AddListener(TakeAll);
    }

    public void LoadAndOpen(Chest chest)
    {
        EnsureInitialized();   // NEU – garantiert Setup, auch wenn Awake() nie automatisch lief

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

    public void TakeItem(int slotIndex)
    {
        if (currentChest == null) return;

        Chest.ChestItem item = currentChest.chestItems[slotIndex];
        if (item.quantity <= 0) return;

        int leftOver = inventoryManager.AddItem(
            item.itemName, item.quantity, item.itemSprite, item.itemDescription, item.itemType);

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

    public void TakeOneItem(int slotIndex)
    {
        if (currentChest == null) return;

        Chest.ChestItem item = currentChest.chestItems[slotIndex];
        if (item.quantity <= 0) return;

        int leftOver = inventoryManager.AddItem(
            item.itemName, 1, item.itemSprite, item.itemDescription, item.itemType);

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