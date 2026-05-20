using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    public ChestSlot[] chestSlots;          // Alle Slot-GameObjects im ChestCanvas
    public Button closeButton;              // Schliessen-Button im Canvas

    private Chest currentChest;
    private InventoryManager inventoryManager;

    void Awake()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void LoadChest(Chest chest)
    {
        currentChest = chest;

        // Alle Slots leeren
        for (int i = 0; i < chestSlots.Length; i++)
            chestSlots[i].ClearSlot();

        // Truhen-Items in Slots laden
        Chest.ChestItem[] items = chest.chestItems;
        for (int i = 0; i < items.Length && i < chestSlots.Length; i++)
        {
            if (items[i].quantity > 0)
                chestSlots[i].SetItem(items[i], i, this);
        }
    }

    // Wird von ChestSlot aufgerufen wenn der Spieler ein Item nimmt
    public void TakeItem(int slotIndex)
    {
        if (currentChest == null) return;

        Chest.ChestItem item = currentChest.chestItems[slotIndex];
        if (item.quantity <= 0) return;

        int leftOver = inventoryManager.AddItem(item.itemName, item.quantity, item.itemSprite, item.itemDescription);

        if (leftOver <= 0)
        {
            currentChest.RemoveItem(slotIndex);
            chestSlots[slotIndex].ClearSlot();
        }
        else
        {
            // Nur teilweise ins Inventar gepasst
            currentChest.chestItems[slotIndex].quantity = leftOver;
            chestSlots[slotIndex].UpdateQuantity(leftOver);
        }
    }

    // "Alles nehmen"-Button
    public void TakeAll()
    {
        for (int i = 0; i < currentChest.chestItems.Length; i++)
            TakeItem(i);
    }

    public void Close()
    {
        if (currentChest != null)
            currentChest.CloseChest();
    }
}