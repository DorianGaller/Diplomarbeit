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

    [Header("Menu Tabs")]
    public GameObject menuTabs;                  // Das Tab-Bar-GameObject (immer sichtbar wenn ein Menü offen)
    public GameObject inventoryTabSelected;      // SelectedPanel auf dem Inventory-Tab-Button
    public GameObject equipmentTabSelected;      // SelectedPanel auf dem Equipment-Tab-Button

    private bool menuActivated;
    public bool chestOpen;

    void Update()
    {
        if (Input.GetButtonDown("Inventory") || Input.GetKeyDown(KeyCode.Escape))
            Inventory();

        if (Input.GetButtonDown("EquipmentMenu") || Input.GetKeyDown(KeyCode.Escape))
            Equipment();
    }

    // ── TAB HELPER ────────────────────────────────────────

    private void SetTabState(bool inventoryActive, bool equipmentActive)
    {
        bool anyOpen = inventoryActive || equipmentActive;

        if (menuTabs != null)
            menuTabs.SetActive(anyOpen);

        if (inventoryTabSelected != null)
            inventoryTabSelected.SetActive(inventoryActive);

        if (equipmentTabSelected != null)
            equipmentTabSelected.SetActive(equipmentActive);
    }

    // ── INPUT HANDLER ─────────────────────────────────────

    public void Inventory()
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

    public void Equipment()
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
                SetTabState(false, true);
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

    public void OpenInventoryOnly()
    {
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        EquipmentMenu.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(true);
        SetTabState(true, false);
    }

    public void CloseInventoryOnly()
    {
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        DeselectAllSlots();
        SetTabState(false, false);
    }

    public void CloseEquipmentOnly()
    {
        menuActivated = false;
        Time.timeScale = 1f;
        EquipmentMenu.SetActive(false);
        DeselectAllSlots();
        SetTabState(false, false);
    }

    public void OpenEquipmentOnly()
    {
        menuActivated = true;
        Time.timeScale = 0f;
        EquipmentMenu.SetActive(true);
        InventoryMenu.SetActive(false);
        SetTabState(false, true);
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
        SetTabState(true, false);
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
        SetTabState(false, false);
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