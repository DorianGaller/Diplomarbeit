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

    private int lastKnownCoins = -1;

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
        BuildList();
        RefreshCoins(force: true);
    }

    void Update()
    {
        // PlayerStats hat kein Aenderungs-Event -> Coins pollen, nur bei Aenderung neu zeichnen.
        if (playerStats != null && playerStats.coins != lastKnownCoins)
            RefreshCoins();
    }

    private void RefreshCoins(bool force = false)
    {
        if (playerStats == null) return;
        if (!force && playerStats.coins == lastKnownCoins) return;

        lastKnownCoins = playerStats.coins;
        if (coinsLabel != null)
            coinsLabel.text = $"{playerStats.coins} Coins";
    }

    private void BuildList()
    {
        if (itemListParent == null || shopEntryPrefab == null) return;

        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        foreach (var entry in entries)
        {
            if (entry == null || entry.item == null) continue;
            var go = Instantiate(shopEntryPrefab, itemListParent);
            var ui = go.GetComponent<ShopEntryUI>();
            if (ui != null) ui.Setup(entry, this);
        }
    }

    // Wird vom ShopEntryUI-Button aufgerufen.
    public void TryBuy(ShopEntryData entry)
    {
        if (entry == null || entry.item == null) return;
        if (playerStats == null || inventoryManager == null)
        {
            Debug.LogError("[ShopApp] PlayerStats oder InventoryManager fehlt.");
            return;
        }

        // 1) Kann sich der Spieler das leisten?
        if (playerStats.coins < entry.price)
        {
            Debug.Log($"[Shop] Zu wenig Coins fuer {entry.item.itemName}.");
            return;
        }

        // 2) Passt die Ware ueberhaupt ins Inventar?
        //    AddItem gibt die NICHT untergebrachte Menge zurueck.
        int leftOver = inventoryManager.AddItem(
            entry.item.itemName,
            entry.quantity,
            entry.item.itemSprite,
            entry.item.itemDescription,
            entry.item.itemType);

        if (leftOver >= entry.quantity)
        {
            // Nichts ging rein -> Inventar voll. NICHT zahlen.
            Debug.Log($"[Shop] Inventar voll, {entry.item.itemName} nicht gekauft.");
            return;
        }

        // 3) Nur die tatsaechlich untergebrachte Menge berechnen und abziehen.
        int boughtAmount = entry.quantity - leftOver;
        int totalCost = entry.price * boughtAmount;

        if (!playerStats.SpendCoins(totalCost))
        {
            // Sollte nach der Pruefung oben nicht passieren, aber sicher ist sicher.
            Debug.LogWarning("[Shop] Bezahlung fehlgeschlagen trotz Deckung.");
            return;
        }

        Debug.Log($"[Shop] Gekauft: {boughtAmount}x {entry.item.itemName} fuer {totalCost} Coins.");
    }
}