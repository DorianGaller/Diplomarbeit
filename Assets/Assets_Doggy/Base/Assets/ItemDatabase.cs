using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Zentrale Nachschlagetabelle fuer alle Items im Spiel.
///
/// Warum das gebraucht wird:
/// - Die Slots speichern nur Strings + Sprites. Fuer EXP-Werte, Waffen-Erkennung
///   und spaeteres Speichern braucht es eine Stelle, die Name -> Daten aufloest.
/// - Als ScriptableObject-Asset ist sie szenenunabhaengig und ueberall verfuegbar,
///   ohne GameObject.Find() und ohne Inspector-Verdrahtung.
///
/// SETUP (einmalig):
/// 1. Ordner anlegen: Assets/Resources/
/// 2. Rechtsklick -> Create -> Inventory -> Item Database
/// 3. Asset muss exakt "ItemDatabase" heissen und in Assets/Resources/ liegen
/// 4. Im Inspector die Items eintragen
/// 5. Rechtsklick auf das Asset -> "Validate Database" prueft auf Fehler
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("Alle Items im Spiel. Der itemName muss exakt dem Namen entsprechen, " +
             "der in den Slots und in Item.cs verwendet wird.")]
    public ItemDefinition[] items;

    // ── Zugriff ───────────────────────────────────────────

    private const string ResourcePath = "ItemDatabase";

    private static ItemDatabase cachedInstance;
    private Dictionary<string, ItemDefinition> lookup;

    public static ItemDatabase Instance
    {
        get
        {
            if (cachedInstance == null)
            {
                cachedInstance = Resources.Load<ItemDatabase>(ResourcePath);

                if (cachedInstance == null)
                {
                    Debug.LogError(
                        "[ItemDatabase] Asset nicht gefunden! Erwartet: " +
                        "Assets/Resources/ItemDatabase.asset");
                }
            }
            return cachedInstance;
        }
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, ItemDefinition>();

        if (items == null) return;

        foreach (var def in items)
        {
            if (def == null || string.IsNullOrEmpty(def.itemName)) continue;

            if (lookup.ContainsKey(def.itemName))
            {
                Debug.LogWarning($"[ItemDatabase] Doppelter itemName: '{def.itemName}' " +
                                 "— nur der erste Eintrag wird verwendet.");
                continue;
            }

            lookup.Add(def.itemName, def);
        }
    }

    /// <summary>Liefert die Definition oder null, wenn das Item nicht eingetragen ist.</summary>
    public static ItemDefinition Get(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        ItemDatabase db = Instance;
        if (db == null) return null;

        if (db.lookup == null)
            db.BuildLookup();

        db.lookup.TryGetValue(itemName, out ItemDefinition def);
        return def;
    }

    // ── Bequeme Kurzformen ────────────────────────────────

    /// <summary>Waffen-EXP eines einzelnen Stuecks. 0 = taugt nicht als EXP-Material.</summary>
    public static int GetWeaponExpValue(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? def.weaponExpValue : 0;
    }

    /// <summary>EXP fuer einen ganzen Stack (Menge x Einzelwert).</summary>
    public static int GetWeaponExpValue(string itemName, int quantity)
    {
        return GetWeaponExpValue(itemName) * Mathf.Max(0, quantity);
    }

    public static bool IsWeapon(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null && def.weaponSO != null;
    }

    public static WeaponSO GetWeapon(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? def.weaponSO : null;
    }

    /// <summary>Sprite ueber den Namen — wird spaeter beim Laden eines Spielstands gebraucht.</summary>
    public static Sprite GetSprite(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? def.itemSprite : null;
    }

    public static string GetDescription(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? def.itemDescription : "";
    }

    public static ItemType GetItemType(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? def.itemType : ItemType.none;
    }

    public static int GetMaxStackSize(string itemName)
    {
        ItemDefinition def = Get(itemName);
        return def != null ? Mathf.Max(1, def.maxStackSize) : 1;
    }

    // ── Editor-Hilfe ──────────────────────────────────────

    [ContextMenu("Validate Database")]
    private void Validate()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("[ItemDatabase] Keine Items eingetragen.");
            return;
        }

        int problems = 0;
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < items.Length; i++)
        {
            ItemDefinition def = items[i];

            if (def == null)
            {
                Debug.LogError($"[ItemDatabase] Eintrag {i} ist leer."); problems++; continue;
            }

            if (string.IsNullOrEmpty(def.itemName))
            {
                Debug.LogError($"[ItemDatabase] Eintrag {i} hat keinen itemName."); problems++; continue;
            }

            if (!seen.Add(def.itemName))
            {
                Debug.LogError($"[ItemDatabase] Doppelter itemName: '{def.itemName}'"); problems++;
            }

            if (def.itemSprite == null)
            {
                Debug.LogWarning($"[ItemDatabase] '{def.itemName}' hat kein Sprite."); problems++;
            }

            if (def.weaponSO != null && def.itemType != ItemType.mainHand)
            {
                Debug.LogWarning($"[ItemDatabase] '{def.itemName}' hat ein WeaponSO, " +
                                 $"aber itemType ist '{def.itemType}' statt mainHand."); problems++;
            }

            if (def.weaponExpValue > 0 && def.weaponSO != null)
            {
                Debug.LogWarning($"[ItemDatabase] '{def.itemName}' ist eine Waffe und hat " +
                                 "trotzdem einen weaponExpValue."); problems++;
            }
        }

        if (problems == 0)
            Debug.Log($"[ItemDatabase] OK — {items.Length} Items, keine Probleme.");
        else
            Debug.Log($"[ItemDatabase] Pruefung fertig — {problems} Auffaelligkeit(en).");
    }
}

/// <summary>
/// Ein Item-Eintrag. Reine Daten, kein Verhalten.
/// </summary>
[System.Serializable]
public class ItemDefinition
{
    [Header("Basis")]
    public string itemName;
    public Sprite itemSprite;
    [TextArea] public string itemDescription;
    public ItemType itemType;

    [Tooltip("Nur relevant fuer stapelbare Typen (consumable, material).")]
    public int maxStackSize = 99;

    [Header("Waffen-Upgrade")]
    [Tooltip("EXP pro Stueck, wenn dieses Material in einen EXP-Slot gelegt wird. " +
             "0 = nicht als EXP verwendbar.")]
    public int weaponExpValue = 0;

    [Tooltip("Nur bei Waffen ausfuellen. Verbindet das Inventar-Item mit den Kampfwerten.")]
    public WeaponSO weaponSO;
}