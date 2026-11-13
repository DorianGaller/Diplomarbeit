using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Steuert das komplette Gun-Upgrade-Panel: Waffen-Slot, vier EXP-Slots,
/// Anzeige (Name, Level, Sterne, EXP-Balken, Schaden) und den Apply-Button.
///
/// Zwei Modi je nach Levelstand der eingesetzten Waffe:
///   normal    -> beliebige Materialien, Button "APPLY EXP"
///   am Cap    -> Ascension noch nicht angebunden (Step 4), Button gesperrt
///
/// WICHTIG: Waffe und Materialien werden beim Reindrop tatsaechlich aus dem
/// Inventar entfernt (siehe UpgradeGunSlot/UpgradeExpSlot). Deshalb muss
/// beim Schliessen alles Unverbrauchte zurueckgegeben werden — sonst
/// verschwinden Items ins Nichts.
/// </summary>
public class WeaponUpgradeUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private UpgradeGunSlot gunSlot;
    [SerializeField] private UpgradeExpSlot[] expSlots;

    [Header("Anzeige — Kopf")]
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private GameObject rarityPlaceholder; // Step 3: fixer Platzhalter, kein echtes System

    [Header("Anzeige — Level")]
    [SerializeField] private TMP_Text levelText;       // "LVL: 1/10"
    [SerializeField] private Slider expBar;             // 0..1
    [SerializeField] private TMP_Text expText;          // "45 / 100 EXP"
    [SerializeField] private Image[] starImages;        // 5 Stueck, gefuellt = erreichter Stern

    [Header("Anzeige — Schaden")]
    [SerializeField] private TMP_Text currentDamageText;
    [SerializeField] private TMP_Text nextDamageText;   // gruene Vorschau

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private TMP_Text applyButtonLabel;

    [Header("Refs")]
    [Tooltip("Leer lassen — wird ueber GameObject.Find('InventoryCanvas') gesucht.")]
    [SerializeField] private InventoryManager inventoryManager;

    private WeaponInstance current;

    private void Awake()
    {
        if (inventoryManager == null)
        {
            var canvas = GameObject.Find("InventoryCanvas");
            if (canvas != null) inventoryManager = canvas.GetComponent<InventoryManager>();
        }

        if (applyButton != null)
            applyButton.onClick.AddListener(OnApplyClicked);

        if (nameField != null)
            nameField.onEndEdit.AddListener(OnNameEdited);
    }

    private void OnEnable()
    {
        current = null;
        gunSlot?.ClearWeapon();
        foreach (var slot in expSlots)
            slot?.ClearSlotData();

        Redraw();
    }

    private void OnDisable()
    {
        // Alles Unverbrauchte zurueck ins Inventar — Panel darf nie
        // etwas endgueltig verschlucken, ausser durch bewusstes Apply.
        ReturnCurrentWeaponToInventory();
        ReturnAllMaterialsToInventory();
    }

    // ── Von den Slots aufgerufen ──────────────────────────

    public void OnWeaponDropped(string instanceId)
    {
        current = WeaponRegistry.Get(instanceId);

        if (current == null)
            Debug.LogWarning($"[Upgrade] Keine Instanz fuer id={instanceId} — Altbestand?");

        Redraw();
    }

    public void OnWeaponRemoved()
    {
        current = null;
        Redraw();
    }

    /// <summary>Legt die aktuelle Waffe zurueck ins Inventar, ohne den Fortschritt zu veraendern.</summary>
    public void ReturnCurrentWeaponToInventory()
    {
        if (!gunSlot.HasWeapon || inventoryManager == null) return;

        inventoryManager.AddItem(
            gunSlot.GetItemName(), 1, gunSlot.GetItemSprite(),
            gunSlot.GetItemDescription(), ItemType.mainHand, gunSlot.GetInstanceId());

        gunSlot.ClearWeapon();
        current = null;
    }

    private void ReturnAllMaterialsToInventory()
    {
        if (inventoryManager == null) return;

        foreach (var slot in expSlots)
        {
            if (slot == null || !slot.HasMaterial) continue;

            inventoryManager.AddItem(
                slot.GetItemName(), slot.GetQuantity(), slot.GetItemSprite(),
                slot.GetItemDescription(), ItemType.material, null);

            slot.ClearSlotData();
        }
    }

    // ── Apply ─────────────────────────────────────────────

    private void OnApplyClicked()
    {
        if (current == null) return;

        if (current.IsAtLevelCap)
        {
            // Ascension kommt in Step 4. Fuers Erste: nichts tun, Button
            // sollte durch Redraw() ohnehin schon gesperrt sein.
            return;
        }

        int totalExp = 0;
        foreach (var slot in expSlots)
            if (slot != null) totalExp += slot.ExpValue;

        if (totalExp <= 0) return;

        current.AddExp(totalExp);

        foreach (var slot in expSlots)
            slot?.Consume();

        Redraw();
    }

    // ── Name ──────────────────────────────────────────────

    private void OnNameEdited(string newName)
    {
        if (current == null) return;

        current.customName = newName;
        Redraw();
    }

    // ── Anzeige ───────────────────────────────────────────

    private void Redraw()
    {
        bool hasWeapon = current != null;

        if (nameField != null)
        {
            nameField.interactable = hasWeapon;
            // SetTextWithoutNotify: verhindert, dass das Neu-Setzen selbst
            // wieder OnNameEdited ausloest.
            nameField.SetTextWithoutNotify(hasWeapon ? current.DisplayName : "");
        }

        if (rarityPlaceholder != null)
            rarityPlaceholder.SetActive(hasWeapon);

        if (!hasWeapon)
        {
            RedrawEmpty();
            return;
        }

        int maxLevel = current.MaxLevel;

        if (levelText != null)
            levelText.text = $"LVL: {current.level}/{maxLevel}";

        if (expBar != null)
            expBar.value = current.ExpProgress;

        if (expText != null)
        {
            expText.text = current.IsAtLevelCap
                ? "MAX LEVEL"
                : $"{current.currentExp} / {current.ExpForNextLevel} EXP";
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
                starImages[i].fillAmount = (i < current.stars) ? 1f : 0f;
        }

        if (currentDamageText != null)
            currentDamageText.text = current.CurrentDamage.ToString();

        if (nextDamageText != null)
        {
            nextDamageText.text = current.IsAtLevelCap
                ? current.CurrentDamage.ToString()
                : current.NextLevelDamage.ToString();
        }

        if (applyButton != null)
            applyButton.interactable = !current.IsAtLevelCap && GetPendingExp() > 0;

        if (applyButtonLabel != null)
            applyButtonLabel.text = current.IsAtLevelCap ? "MAX LEVEL" : "APPLY EXP";
    }

    private void RedrawEmpty()
    {
        if (levelText != null) levelText.text = "LVL: -/-";
        if (expBar != null) expBar.value = 0f;
        if (expText != null) expText.text = "- / - EXP";
        if (currentDamageText != null) currentDamageText.text = "-";
        if (nextDamageText != null) nextDamageText.text = "-";
        if (applyButton != null) applyButton.interactable = false;
        if (applyButtonLabel != null) applyButtonLabel.text = "APPLY EXP";

        foreach (var star in starImages)
            if (star != null) star.fillAmount = 0f;
    }

    private int GetPendingExp()
    {
        int total = 0;
        foreach (var slot in expSlots)
            if (slot != null) total += slot.ExpValue;
        return total;
    }

    /// <summary>
    /// Von den EXP-Slots nach jedem Drop aufzurufen, damit die Vorschau
    /// (Apply-Button, gruene Schadenszahl waere hier noch statisch) aktuell bleibt.
    /// Einfachster Weg: hier ebenfalls Redraw callen.
    /// </summary>
    public void OnExpSlotChanged() => Redraw();
}