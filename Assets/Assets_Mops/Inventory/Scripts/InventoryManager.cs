using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory UI")]
    public GameObject InventoryMenu;
    public ItemSlot[] itemSlot;
    public ItemSO[] itemSOs;

    [Header("Panels")]
    public GameObject chestPanel;           // Chest_Panel – nur bei Truhe
    public GameObject inventoryDescription; // InventoryDescription – nur bei E

    private bool menuActivated;
    public bool chestOpen;

    void Update()
    {
        if (Input.GetButtonDown("Inventory") && !chestOpen)
        {
            if (menuActivated)
                CloseInventoryOnly();
            else
                OpenInventoryOnly();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (chestOpen)
                CloseChestView();
            else if (menuActivated)
                CloseInventoryOnly();
        }
    }

    // ── NUR INVENTAR (Taste E) ────────────────────────────

    private void OpenInventoryOnly()
    {
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        if (chestPanel != null)           chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(true);
    }

    private void CloseInventoryOnly()
    {
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        DeselectAllSlots();
    }

    // ── TRUHE + INVENTAR (Shelf-Button) ──────────────────

    public void OpenChestView()
    {
        chestOpen     = true;
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        if (chestPanel != null)           chestPanel.SetActive(true);
        if (inventoryDescription != null) inventoryDescription.SetActive(false);
    }

    public void CloseChestView()
    {
        chestOpen     = false;
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        if (chestPanel != null)           chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(false);
        DeselectAllSlots();
    }

    // ── BESTEHENDE METHODEN ───────────────────────────────

    public bool UseItem(string itemName)
    {
        for (int i = 0; i < itemSOs.Length; i++)
        {
            if (itemSOs[i].itemName == itemName)
                return itemSOs[i].UseItem();
        }
        return false;
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (!itemSlot[i].isFull && itemSlot[i].itemName == itemName)
            {
                int left = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                if (left > 0) left = AddItem(itemName, left, itemSprite, itemDescription);
                return left;
            }
        }
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].quantity == 0)
            {
                int left = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                if (left > 0) left = AddItem(itemName, left, itemSprite, itemDescription);
                return left;
            }
        }
        return quantity;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }
}