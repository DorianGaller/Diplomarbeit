using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory UI")]
    public GameObject InventoryMenu;
    public GameObject EquipmentMenu;
    public ItemSlot[] itemSlot;
    public EquipmentSlot[] equipmentSlot;
    public EquippedSlot[] equippedSlot;

    public ItemSO[] itemSOs;

    [Header("Panels")]
    public GameObject chestPanel;
    public GameObject inventoryDescription;

    private bool menuActivated;
    public bool chestOpen;

    void Update()
    {
        if (Input.GetButtonDown("Inventory") || Input.GetKeyDown(KeyCode.Escape))
            Inventory();

        if (Input.GetButtonDown("EquipmentMenu") || Input.GetKeyDown(KeyCode.Escape))
            Equipment();
    }

    void Inventory()
    {
        if (Input.GetButtonDown("Inventory") && !chestOpen)
        {
            if (menuActivated)
            {
                CloseInventoryOnly();
                EquipmentMenu.SetActive(false);
            }
            else
            {
                OpenInventoryOnly();
                EquipmentMenu.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1;
            if (chestOpen)
                CloseChestView();
            else if (menuActivated)
            {
                CloseInventoryOnly();
                CloseEquipmentOnly();
            }
        }
    }

    void Equipment()
    {
        if (Input.GetButtonDown("EquipmentMenu") && !chestOpen)
        {
            if (menuActivated)
            {
                CloseEquipmentOnly();
            }
            else
            {
                menuActivated = true;
                Time.timeScale = 0f;
                EquipmentMenu.SetActive(true);
                InventoryMenu.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1;
            if (chestOpen)
                CloseChestView();
            else if (menuActivated)
            {
                CloseInventoryOnly();
                CloseEquipmentOnly();
            }
        }
    }

    // ── NUR INVENTAR (Taste E) ────────────────────────────

    private void OpenInventoryOnly()
    {
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(true);
    }

    private void CloseInventoryOnly()
    {
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        DeselectAllSlots();
    }

    private void CloseEquipmentOnly()
    {
        menuActivated = false;
        Time.timeScale = 1f;
        EquipmentMenu.SetActive(false);
        DeselectAllSlots();
    }

    // ── TRUHE + INVENTAR (Shelf-Button) ──────────────────

    public void OpenChestView()
    {
        chestOpen = true;
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        if (chestPanel != null) chestPanel.SetActive(true);
        if (inventoryDescription != null) inventoryDescription.SetActive(false);
    }

    public void CloseChestView()
    {
        chestOpen = false;
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
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

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {
        if (itemType == ItemType.consumable)
        {
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].quantity == 0)
                {
                    int left = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if (left > 0) left = AddItem(itemName, left, itemSprite, itemDescription, itemType);
                    return left;
                }
            }
            return quantity;
        }

        else
        {
            for (int i = 0; i < equipmentSlot.Length; i++)
            {
                if (equipmentSlot[i].quantity == 0)
                {
                    int left = equipmentSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if (left > 0) left = AddItem(itemName, left, itemSprite, itemDescription, itemType);
                    return left;
                }
            }
            return quantity;
        }




    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }

        // Equipment Slots auch deselektieren
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].selectedShader.SetActive(false);
            equipmentSlot[i].thisItemSelected = false;
        }

        for (int i = 0; i < equippedSlot.Length; i++)
        {
            equippedSlot[i].selectedShader.SetActive(false);
            equippedSlot[i].thisItemSelected = false;
        }
    }

}

public enum ItemType
{
    none,
    consumable,
    head,
    arms,
    body,
    legs,
    mainHand,
    offHand,
    relic,
    feet,
};