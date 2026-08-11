using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopApp : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Alle kaufbaren Eintraege: ItemSO + Preis.")]
    public ShopEntryData[] entries;

    [Header("Refs")]
    [Tooltip("PlayerStats des Spielers (haelt die Coins).")]
    public PlayerStats playerStats;

    [Tooltip("InventoryManager (nimmt gekaufte Ware auf).")]
    public InventoryManager inventoryManager;

    [Header("UI Refs")]
    [Tooltip("Label das den aktuellen Coin-Stand zeigt.")]
    public TMP_Text coinsLabel;

    [Tooltip("Eltern-Objekt fuer die Shop-Zeilen (mit Vertical Layout Group).")]
    public Transform itemListParent;

    [Tooltip("Prefab einer Shop-Zeile (mit ShopEntryUI).")]
    public GameObject shopEntryPrefab;

    [Header("Verkauf")]
    [Range(0f, 1f)]
    [Tooltip("Anteil des Kaufpreises, den man beim Verkauf zurueckbekommt.")]
    public float sellRate = 0.75f;

    [Header("Debug")]
    [Tooltip("Ausfuehrliche Logs beim Oeffnen des Shops.")]
    public bool verboseLogging = true;

    private int lastKnownCoins = -1;
    private readonly List<ShopEntryUI> spawnedRows = new List<ShopEntryUI>();

    void Awake()
    {
        // Refs automatisch suchen, falls im Inspector nicht gesetzt.
        if (playerStats == null)
        {
            var sm = GameObject.Find("StatManager");
            if (sm != null) playerStats = sm.GetComponent<PlayerStats>();
        }
        if (inventoryManager == null)
        {
            var ic = GameObject.Find("InventoryCanvas");
            if (ic != null) inventoryManager = ic.GetComponent<InventoryManager>();
        }
    }

    void OnEnable()
    {
        DiagnoseSetup();
        BuildList();
        RefreshCoins(force: true);
    }

    void Update()
    {
        // PlayerStats hat kein Aenderungs-Event -> Coins pollen.
        if (playerStats != null && playerStats.coins != lastKnownCoins)
            RefreshCoins();
    }

    // ── Diagnose ──────────────────────────────────────────
    // Laeuft einmal beim Oeffnen und deckt die typischen Setup-Fehler auf.

    private void DiagnoseSetup()
    {
        if (!verboseLogging) return;

        if (inventoryManager == null)
            Debug.LogError("[Shop] Kein InventoryManager gefunden.");
        else
            Debug.Log($"[Shop] InventoryManager: '{inventoryManager.gameObject.name}' " +
                      $"(InstanceID {inventoryManager.GetInstanceID()}), " +
                      $"itemSlots={inventoryManager.itemSlot.Length}, " +
                      $"equipmentSlots={inventoryManager.equipmentSlot.Length}");

        // Mehrere InventoryManager in der Szene? Klassische DontDestroyOnLoad-Falle.
        var allManagers = FindObjectsByType<InventoryManager>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allManagers.Length > 1)
            Debug.LogError($"[Shop] ACHTUNG: {allManagers.Length} InventoryManager im Spiel. " +
                           "Der Shop redet moeglicherweise mit dem falschen.");

        if (playerStats == null)
            Debug.LogError("[Shop] Keine PlayerStats gefunden.");

        // Datenfehler an den ItemSOs melden.
        foreach (var entry in entries)
        {
            if (entry == null || entry.item == null) continue;

            if (string.IsNullOrEmpty(entry.item.itemName))
                Debug.LogError($"[Shop] ItemSO '{entry.item.name}' hat keinen itemName.");

            if (entry.item.itemSprite == null)
                Debug.LogWarning($"[Shop] ItemSO '{entry.item.itemName}' hat kein itemSprite " +
                                 "-> der Inventar-Slot bleibt optisch leer.");

            if (entry.item.itemType == ItemType.none)
                Debug.LogWarning($"[Shop] ItemSO '{entry.item.itemName}' hat itemType 'none' " +
                                 "-> landet in den EQUIPMENT-Slots, nicht im Inventar-Tab. " +
                                 "Fuer Materialien 'consumable' setzen.");
        }
    }

    private void RefreshCoins(bool force = false)
    {
        if (playerStats == null) return;
        if (!force && playerStats.coins == lastKnownCoins) return;

        lastKnownCoins = playerStats.coins;
        if (coinsLabel != null)
            coinsLabel.text = $"{playerStats.coins} Coins";

        RefreshRows();
    }

    private void BuildList()
    {
        if (itemListParent == null || shopEntryPrefab == null) return;

        spawnedRows.Clear();

        // Alte Zeilen sofort ausblenden - Destroy greift erst am Frame-Ende.
        foreach (Transform child in itemListParent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        foreach (var entry in entries)
        {
            if (entry == null || entry.item == null) continue;

            var go = Instantiate(shopEntryPrefab, itemListParent);
            var ui = go.GetComponent<ShopEntryUI>();
            if (ui != null)
            {
                ui.Setup(entry, this);
                spawnedRows.Add(ui);
            }
            else
            {
                Debug.LogError("[Shop] shopEntryPrefab hat keine ShopEntryUI-Komponente.");
            }
        }
    }

    // Alle Zeilen neu bewerten (Besitzanzeige, Button-Zustaende).
    public void RefreshRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
            if (spawnedRows[i] != null) spawnedRows[i].Refresh();
    }

    // ── Abfragen fuer die UI ──────────────────────────────

    public int GetOwned(ShopEntryData entry)
    {
        if (entry == null || entry.item == null || inventoryManager == null) return 0;
        return inventoryManager.CountItem(entry.item.itemName);
    }

    public bool CanAfford(ShopEntryData entry)
    {
        if (entry == null || playerStats == null) return false;
        return playerStats.coins >= entry.price * Mathf.Max(1, entry.quantity);
    }

    public bool CanSell(ShopEntryData entry)
    {
        if (entry == null) return false;
        return GetOwned(entry) >= Mathf.Max(1, entry.quantity);
    }

    // ── Kauf ──────────────────────────────────────────────

    public void TryBuy(ShopEntryData entry)
    {
        if (entry == null || entry.item == null) return;
        if (playerStats == null || inventoryManager == null)
        {
            Debug.LogError("[Shop] PlayerStats oder InventoryManager fehlt.");
            return;
        }

        int amount = Mathf.Max(1, entry.quantity);
        int fullCost = entry.price * amount;

        // 1) Gegen den GESAMTPREIS pruefen, nicht gegen den Stueckpreis.
        if (playerStats.coins < fullCost)
        {
            Debug.Log($"[Shop] Zu wenig Coins fuer {entry.item.itemName} " +
                      $"({playerStats.coins}/{fullCost}).");
            return;
        }

        // 2) Einlagern. AddItem gibt zurueck, was NICHT reingepasst hat.
        int leftOver = inventoryManager.AddItem(
            entry.item.itemName,
            amount,
            entry.item.itemSprite,
            entry.item.itemDescription,
            entry.item.itemType);

        if (leftOver >= amount)
        {
            Debug.Log($"[Shop] Inventar voll, {entry.item.itemName} nicht gekauft.");
            return;
        }

        // 3) Nur das bezahlen, was wirklich angekommen ist.
        int boughtAmount = amount - leftOver;
        int totalCost = entry.price * boughtAmount;

        if (!playerStats.SpendCoins(totalCost))
        {
            Debug.LogError("[Shop] Bezahlung fehlgeschlagen trotz Deckung.");
            return;
        }

        Debug.Log($"[Shop] Gekauft: {boughtAmount}x {entry.item.itemName} fuer {totalCost} Coins. " +
                  $"Bestand laut CountItem: {inventoryManager.CountItem(entry.item.itemName)}, " +
                  $"Ziel-Slots: {(entry.item.itemType == ItemType.consumable ? "itemSlot" : "equipmentSlot")}");

        RefreshCoins(force: true);
    }

    // ── Verkauf ───────────────────────────────────────────

    public void TrySell(ShopEntryData entry)
    {
        if (entry == null || entry.item == null) return;
        if (playerStats == null || inventoryManager == null)
        {
            Debug.LogError("[Shop] PlayerStats oder InventoryManager fehlt.");
            return;
        }

        int amount = Mathf.Max(1, entry.quantity);

        int owned = inventoryManager.CountItem(entry.item.itemName);
        if (owned < amount)
        {
            Debug.Log($"[Shop] Nicht genug {entry.item.itemName} zum Verkaufen " +
                      $"(hast {owned}, brauchst {amount}). " +
                      "Hinweis: ausgeruestete Items zaehlen nicht mit.");
            return;
        }

        int removed = inventoryManager.RemoveItem(entry.item.itemName, amount);
        if (removed <= 0)
        {
            Debug.LogError($"[Shop] {entry.item.itemName} konnte nicht entfernt werden, " +
                           $"obwohl CountItem {owned} meldet.");
            return;
        }

        int payout = Mathf.RoundToInt(entry.price * sellRate) * removed;
        playerStats.AddCoins(payout);

        Debug.Log($"[Shop] Verkauft: {removed}x {entry.item.itemName} fuer {payout} Coins.");

        RefreshCoins(force: true);
    }
}