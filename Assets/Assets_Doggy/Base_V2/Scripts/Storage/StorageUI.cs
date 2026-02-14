using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Haupt-UI Manager für das Storage System
/// Verwaltet alle Slots und Statistiken
/// Hänge dieses Script an dein Storage UI Panel
/// </summary>
public class StorageUI : MonoBehaviour
{
    [Header("Slot Container")]
    [Tooltip("Transform wo die Slots erstellt werden (meist ein GameObject mit Grid Layout)")]
    public Transform slotsContainer;
    
    [Tooltip("Prefab für einen einzelnen Slot")]
    public GameObject slotPrefab;

    [Header("Statistics Panel")]
    [Tooltip("Text für Statistiken")]
    public TextMeshProUGUI statsText;
    
    [Tooltip("Panel das die Stats enthält (kann ein/ausgeschaltet werden)")]
    public GameObject statsPanel;
    
    [Tooltip("Button um Stats Panel zu togglen")]
    public Button toggleStatsButton;

    [Header("Header Info")]
    [Tooltip("Text für Lager-Namen")]
    public TextMeshProUGUI storageNameText;
    
    [Tooltip("Text für Slot-Info (z.B. '25/50')")]
    public TextMeshProUGUI slotInfoText;

    [Header("Close Button")]
    public Button closeButton;

    [Header("Settings")]
    public bool autoRefresh = true;
    public bool showStatsOnOpen = true;

    private StorageContainer currentStorage;
    private List<StorageSlotUI> slotUIList = new List<StorageSlotUI>();
    private bool isInitialized = false;

    void Awake()
    {
        // Setup Buttons
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseUI);
        }

        if (toggleStatsButton != null)
        {
            toggleStatsButton.onClick.AddListener(ToggleStatsPanel);
        }

        // Stats Panel initial state
        if (statsPanel != null)
        {
            statsPanel.SetActive(showStatsOnOpen);
        }
    }

    /// <summary>
    /// Initialisiert das UI mit einem Storage Container
    /// </summary>
    public void Initialize(StorageContainer storage)
    {
        if (storage == null)
        {
            Debug.LogError("[StorageUI] Cannot initialize with null storage!");
            return;
        }

        // Wenn schon ein Storage verbunden ist, disconnecte
        if (currentStorage != null)
        {
            currentStorage.OnStorageChanged -= RefreshUI;
        }

        currentStorage = storage;
        currentStorage.OnStorageChanged += RefreshUI;

        CreateSlots();
        RefreshUI();
        UpdateHeaderInfo();

        isInitialized = true;
        
        Debug.Log($"[StorageUI] Initialized with storage: {currentStorage.storageName}");
    }

    /// <summary>
    /// Erstellt alle Slot-UI Elemente
    /// </summary>
    void CreateSlots()
    {
        if (slotsContainer == null || slotPrefab == null)
        {
            Debug.LogError("[StorageUI] Slots Container oder Slot Prefab fehlt!");
            return;
        }

        // Lösche bestehende Slots
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        slotUIList.Clear();

        // Erstelle neue Slots
        var items = currentStorage.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            slotObj.name = $"Slot_{i}";
            
            StorageSlotUI slotUI = slotObj.GetComponent<StorageSlotUI>();
            if (slotUI != null)
            {
                slotUI.SetSlot(items[i], i);
                slotUIList.Add(slotUI);
            }
            else
            {
                Debug.LogError("[StorageUI] Slot Prefab hat kein StorageSlotUI Script!");
            }
        }

        Debug.Log($"[StorageUI] Created {slotUIList.Count} slots");
    }

    /// <summary>
    /// Aktualisiert das UI (wird automatisch aufgerufen wenn Storage sich ändert)
    /// </summary>
    public void RefreshUI()
    {
        if (!isInitialized || currentStorage == null) return;

        var items = currentStorage.GetAllItems();

        for (int i = 0; i < slotUIList.Count && i < items.Count; i++)
        {
            slotUIList[i].SetSlot(items[i], i);
        }

        UpdateStatistics();
        UpdateHeaderInfo();
    }

    /// <summary>
    /// Aktualisiert Header Informationen
    /// </summary>
    void UpdateHeaderInfo()
    {
        if (currentStorage == null) return;

        if (storageNameText != null)
        {
            storageNameText.text = currentStorage.storageName.ToUpper();
        }

        if (slotInfoText != null)
        {
            int occupied = currentStorage.GetOccupiedSlots();
            int total = currentStorage.GetAllItems().Count;
            slotInfoText.text = $"SLOTS: {occupied}/{total}";
        }
    }

    /// <summary>
    /// Aktualisiert Statistiken Panel
    /// </summary>
    void UpdateStatistics()
    {
        if (statsText == null || currentStorage == null) return;

        var categoryStats = currentStorage.GetCategoryStats();
        int totalItems = currentStorage.GetTotalItemCount();
        int occupiedSlots = currentStorage.GetOccupiedSlots();
        int emptySlots = currentStorage.GetEmptySlots();

        string stats = "<color=#00FFFF>═══════════════════════════</color>\n";
        stats += "<color=#00FFFF><size=18>LAGER SYSTEM v2.077</size></color>\n";
        stats += "<color=#00FFFF>═══════════════════════════</color>\n\n";

        stats += "<color=#FFD700>▸ ÜBERSICHT</color>\n";
        stats += $"<color=#FFFFFF>  Total Items:</color> <color=#00FF00>{totalItems}</color>\n";
        stats += $"<color=#FFFFFF>  Belegte Slots:</color> <color=#00FF00>{occupiedSlots}</color>\n";
        stats += $"<color=#FFFFFF>  Freie Slots:</color> <color=#00FF00>{emptySlots}</color>\n\n";

        stats += "<color=#FFD700>▸ KATEGORIEN</color>\n";
        if (categoryStats.Count > 0)
        {
            foreach (var kvp in categoryStats)
            {
                stats += $"<color=#00FFFF>  • {kvp.Key}:</color> <color=#FFFFFF>{kvp.Value}</color>\n";
            }
        }
        else
        {
            stats += "<color=#888888>  Keine Items im Lager</color>\n";
        }

        stats += "\n<color=#FFD700>▸ TRANSAKTIONEN</color>\n";
        stats += $"<color=#00FF00>  ↑ Eingänge:</color> <color=#FFFFFF>{currentStorage.GetTotalTransactionsIn()}</color>\n";
        stats += $"<color=#FF6666>  ↓ Ausgänge:</color> <color=#FFFFFF>{currentStorage.GetTotalTransactionsOut()}</color>\n";

        stats += "\n<color=#00FFFF>═══════════════════════════</color>";

        statsText.text = stats;
    }

    /// <summary>
    /// Wird von StorageSlotUI aufgerufen beim Drag & Drop
    /// </summary>
    public void MoveItemBetweenSlots(int fromSlot, int toSlot)
    {
        if (currentStorage != null)
        {
            currentStorage.MoveItem(fromSlot, toSlot);
        }
    }

    /// <summary>
    /// Toggle Stats Panel
    /// </summary>
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(!statsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Schließt das UI
    /// </summary>
    void CloseUI()
    {
        // Finde das StorageInteractable und schließe damit
        StorageInteractable interactable = FindObjectOfType<StorageInteractable>();
        if (interactable != null)
        {
            interactable.CloseStorageExternal();
        }
        else
        {
            // Fallback: Einfach Panel deaktivieren
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void OnDestroy()
    {
        // Cleanup
        if (currentStorage != null)
        {
            currentStorage.OnStorageChanged -= RefreshUI;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseUI);
        }

        if (toggleStatsButton != null)
        {
            toggleStatsButton.onClick.RemoveListener(ToggleStatsPanel);
        }
    }

    // ===== PUBLIC METHODEN FÜR EXTERNE SYSTEME =====

    public StorageContainer GetCurrentStorage()
    {
        return currentStorage;
    }

    public void ForceRefresh()
    {
        RefreshUI();
    }
}
