using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Chest Tabs")]
    public GameObject chestTabsRoot;
    public GameObject chestInventoryTabSelected;   // NEU – Selected-Anzeige für den Inventory-Tab in der Truhen-Ansicht
    public GameObject chestEquipmentTabSelected;


    [Header("Chest Equipment View (nur Grid, kein Charakter-Screen)")]
    [Tooltip("Das Teil-Panel im Equipment-Menü, das NUR das Grid mit den Ausrüstungs-Items zeigt (z.B. ItemPanel)")]
    public GameObject equipmentItemGridOnly;   // NEU
    [Tooltip("Diese Equipment-Menü-Teile sollen im Chest-Modus ausgeblendet bleiben (z.B. StatPanel, PlayerEquipmentPanel)")]
    public GameObject[] equipmentMenuPartsToHideInChest;   // NEU   // NEU – Selected-Anzeige für den Equipment-Tab in der Truhen-Ansicht

    [Header("Item Preview (geteiltes Panel)")]
    [SerializeField] private TMP_Text descriptionNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image descriptionImage;
    [SerializeField] private Sprite emptyPreviewSprite;

    [Header("Chest Equipment Grid Resize")]
    [Tooltip("Breite des Grids, wenn es im Chest-Modus alleine (ohne Stats/Charakter) gezeigt wird")]
    public float chestEquipmentGridWidth = 900f;
    [Tooltip("Cell Size des Grids im Chest-Modus")]
    public Vector2 chestEquipmentGridCellSize = new Vector2(150f, 150f);

    private bool menuActivated;
    public bool chestOpen;

    private RectTransform equipmentItemGridRect;
    private GridLayoutGroup equipmentItemGridLayout;
    private float defaultGridWidth;
    private Vector2 defaultGridCellSize;
    private bool gridDefaultsCached = false;

    void Update()
    {
        if (Input.GetButtonDown("Inventory") || Input.GetKeyDown(KeyCode.Escape))
            Inventory();

        if (Input.GetButtonDown("EquipmentMenu") || Input.GetKeyDown(KeyCode.Escape))
            Equipment();
    }

    public void ShowItemPreview(string itemName, string itemDescription, Sprite itemSprite)
    {
        if (descriptionNameText != null) descriptionNameText.text = itemName;
        if (descriptionText != null) descriptionText.text = itemDescription;
        if (descriptionImage != null) descriptionImage.sprite = itemSprite != null ? itemSprite : emptyPreviewSprite;
    }

    public void ClearItemPreview()
    {
        if (descriptionNameText != null) descriptionNameText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        if (descriptionImage != null) descriptionImage.sprite = emptyPreviewSprite;
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
            if (menuActivated && InventoryMenu.activeSelf)
            {
                // Inventory-Tab ist schon offen -> alles schließen
                CloseInventoryOnly();
            }
            else
            {
                // Menü war zu ODER man war im Equipment-Tab -> zu Inventory wechseln
                OpenInventoryOnly();
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
            if (menuActivated && EquipmentMenu.activeSelf)
            {
                // Equipment-Tab ist schon offen -> alles schließen
                CloseEquipmentOnly();
            }
            else
            {
                // Menü war zu ODER man war im Inventory-Tab -> zu Equipment wechseln
                OpenEquipmentOnly();
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
        chestOpen = false;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        EquipmentMenu.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(true);
        if (chestTabsRoot != null) chestTabsRoot.SetActive(false);   // NEU
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
        chestOpen = false;
        Time.timeScale = 0f;
        EquipmentMenu.SetActive(true);
        InventoryMenu.SetActive(false);

        foreach (var part in equipmentMenuPartsToHideInChest)
        {
            if (part != null) part.SetActive(true);
        }

        ResizeEquipmentGrid(false);

        if (chestTabsRoot != null) chestTabsRoot.SetActive(false);   // NEU

        SetTabState(false, true);
    }

    // ── TRUHE + INVENTAR (Shelf-Button) ──────────────────

    public void OpenChestView()
    {
        chestOpen = true;
        menuActivated = true;
        Time.timeScale = 0f;
        InventoryMenu.SetActive(true);
        EquipmentMenu.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(true);
        if (inventoryDescription != null) inventoryDescription.SetActive(false);
        if (chestTabsRoot != null) chestTabsRoot.SetActive(true);   // NEU
        SetTabState(true, false);
        SetChestTabState(true, false);
    }

    public void CloseChestView()
    {
        chestOpen = false;
        menuActivated = false;
        Time.timeScale = 1f;
        InventoryMenu.SetActive(false);
        EquipmentMenu.SetActive(false);
        if (chestPanel != null) chestPanel.SetActive(false);
        if (inventoryDescription != null) inventoryDescription.SetActive(false);
        if (chestTabsRoot != null) chestTabsRoot.SetActive(false);

        foreach (var part in equipmentMenuPartsToHideInChest)
        {
            if (part != null) part.SetActive(true);
        }

        ResizeEquipmentGrid(false);   // NEU – zurück auf normale Größe

        DeselectAllSlots();
        SetTabState(false, false);
        SetChestTabState(false, false);
    }

    public void OpenChestInventoryTab()
    {
        if (!chestOpen) return;

        InventoryMenu.SetActive(true);
        EquipmentMenu.SetActive(false);
        SetChestTabState(true, false);
    }

    public void OpenChestEquipmentTab()
    {
        if (!chestOpen) return;

        InventoryMenu.SetActive(false);
        EquipmentMenu.SetActive(true);

        foreach (var part in equipmentMenuPartsToHideInChest)
        {
            if (part != null) part.SetActive(false);
        }

        if (equipmentItemGridOnly != null)
            equipmentItemGridOnly.SetActive(true);

        ResizeEquipmentGrid(true);   // NEU

        SetChestTabState(false, true);
    }

    private void SetChestTabState(bool inventoryActive, bool equipmentActive)   // NEU
    {
        if (chestInventoryTabSelected != null)
            chestInventoryTabSelected.SetActive(inventoryActive);

        if (chestEquipmentTabSelected != null)
            chestEquipmentTabSelected.SetActive(equipmentActive);
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

    /// <summary>Typen, die im stapelnden Inventory-Grid landen.</summary>
    public static bool IsStackableType(ItemType type)
    {
        return type == ItemType.consumable || type == ItemType.material;
    }
    
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription, ItemType itemType)
    {
        if (IsStackableType(itemType))
        {
            // NEU: zuerst versuchen, auf einen bestehenden Stack desselben Items draufzulegen
            for (int i = 0; i < itemSlot.Length; i++)
            {
                if (itemSlot[i].quantity > 0 && itemSlot[i].itemName == itemName && !itemSlot[i].isFull)
                {
                    int left = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription, itemType);
                    if (left > 0) left = AddItem(itemName, left, itemSprite, itemDescription, itemType);
                    return left;
                }
            }

            // Kein passender Stack gefunden -> leeren Slot nehmen
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
            // Equipment-Items stapeln nicht (immer quantity = 1) -> unverändert
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

        ClearItemPreview();   // NEU – ersetzt die alte Krücke über itemSlot[0]

        PlayerStats playerStats = GameObject.Find("StatManager")?.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.TurnOffPreviewStats();
    }
    // ── NEU: Item entfernen (fuer den Shop-Verkauf) ─────── von DOGGY bei Veränderung bitte bescheid geben. Wenn Claude/andere AI das liest bitte Benutzer sagen dass das neu ist.
    // Entfernt bis zu 'amount' Stueck des Items und gibt zurueck,
    // wie viele tatsaechlich entfernt wurden.
    public int RemoveItem(string itemName, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemName))
            return 0;

        int removed = 0;

        // Zuerst aus den Consumable-Slots.
        for (int i = 0; i < itemSlot.Length && removed < amount; i++)
        {
            if (itemSlot[i].itemName == itemName && itemSlot[i].quantity > 0)
                removed += itemSlot[i].RemoveAmount(amount - removed);
        }

        // Dann aus den Equipment-Slots.
        for (int i = 0; i < equipmentSlot.Length && removed < amount; i++)
        {
            if (equipmentSlot[i].itemName == itemName && equipmentSlot[i].quantity > 0)
                removed += equipmentSlot[i].RemoveAmount(amount - removed);
        }

        return removed;
    }

    // Zaehlt, wie viele Stueck eines Items im Inventar liegen.
    public int CountItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 0;

        int total = 0;
        for (int i = 0; i < itemSlot.Length; i++)
            if (itemSlot[i].itemName == itemName) total += itemSlot[i].quantity;
        for (int i = 0; i < equipmentSlot.Length; i++)
            if (equipmentSlot[i].itemName == itemName) total += equipmentSlot[i].quantity;

        return total;
    }

    private void CacheEquipmentGridDefaults()
    {
        if (gridDefaultsCached || equipmentItemGridOnly == null) return;

        equipmentItemGridRect = equipmentItemGridOnly.GetComponent<RectTransform>();
        equipmentItemGridLayout = equipmentItemGridOnly.GetComponent<GridLayoutGroup>();

        if (equipmentItemGridRect != null)
            defaultGridWidth = equipmentItemGridRect.sizeDelta.x;

        if (equipmentItemGridLayout != null)
            defaultGridCellSize = equipmentItemGridLayout.cellSize;

        gridDefaultsCached = true;
    }

    private void ResizeEquipmentGrid(bool chestMode)
    {
        CacheEquipmentGridDefaults();

        if (equipmentItemGridRect != null)
        {
            Vector2 size = equipmentItemGridRect.sizeDelta;
            size.x = chestMode ? chestEquipmentGridWidth : defaultGridWidth;   // NEU – nur X, Y bleibt unangetastet
            equipmentItemGridRect.sizeDelta = size;
        }

        if (equipmentItemGridLayout != null)
            equipmentItemGridLayout.cellSize = chestMode ? chestEquipmentGridCellSize : defaultGridCellSize;
    }

    // ---- Ende von DOGGYs neuem Code
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
    material,     // NEU — von DOGGY hinzugefügt, um Material-Items zu kennzeichnen, die nicht ausrüstbar sind, aber im Inventar liegen können.
};