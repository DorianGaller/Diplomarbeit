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
/// SPEICHERORT:
/// Assets/Assets_Mops/Items/Resources/ItemDatabase.asset
///
/// Wichtig: Resources.Load() findet nur Assets, die in einem Ordner mit dem
/// exakten Namen "Resources" liegen. Der Ordner darf aber irgendwo im Projekt
/// stehen — deshalb liegt er direkt bei den Items und nicht unter Assets/Resources/.
/// Geladen wird immer nur der Pfad INNERHALB des Resources-Ordners, hier also
/// schlicht "ItemDatabase".
///
/// SETUP (einmalig):
/// 1. Rechtsklick -> Create -> Inventory -> Item Database
/// 2. Asset muss exakt "ItemDatabase" heissen und in
///    Assets/Assets_Mops/Items/Resources/ liegen
/// 3. Im Inspector die Items eintragen
/// 4. Rechtsklick auf das Asset -> "Validate Database" prueft auf Fehler
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("Alle Items im Spiel. Der itemName muss exakt dem Namen entsprechen, " +
             "der in den Slots und in Item.cs verwendet wird.")]
    public ItemDefinition[] items;

    // ── Zugriff ───────────────────────────────────────────

    /// <summary>
    /// Pfad INNERHALB des Resources-Ordners — nicht der Projektpfad.
    /// Das Asset selbst liegt unter Assets/Assets_Mops/Items/Resources/ItemDatabase.asset.
    /// Laege es z.B. in .../Resources/Datenbanken/, waere der Wert "Datenbanken/ItemDatabase".
    /// </summary>
    private const string ResourcePath = "ItemDatabase";

    /// <summary>Voller Projektpfad — nur fuer Fehlermeldungen und den Editor-Fallback.</summary>
    private const string AssetPath = "Assets/Assets_Mops/Items/Resources/ItemDatabase.asset";

    private static ItemDatabase cachedInstance;
    private Dictionary<string, ItemDefinition> lookup;

    public static ItemDatabase Instance
    {
        get
        {
            if (cachedInstance == null)
            {
                cachedInstance = Resources.Load<ItemDatabase>(ResourcePath);

#if UNITY_EDITOR
                // Fallback: Wurde das Asset im Projekt verschoben oder liegt es (noch)
                // nicht in einem Resources-Ordner, wird es im Editor trotzdem gefunden.
                // Im Build gibt es diesen Weg nicht — dort MUSS es unter Resources/ liegen.
                if (cachedInstance == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDatabase");

                    if (guids.Length > 0)
                    {
                        string foundPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        cachedInstance =
                            UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDatabase>(foundPath);

                        Debug.LogWarning(
                            $"[ItemDatabase] Asset liegt unter '{foundPath}' und damit nicht " +
                            $"im erwarteten Resources-Ordner. Im Editor funktioniert es, im " +
                            $"fertigen Build NICHT. Bitte nach '{AssetPath}' verschieben.");
                    }
                }
#endif

                if (cachedInstance == null)
                {
                    Debug.LogError(
                        "[ItemDatabase] Asset nicht gefunden! Erwartet: " + AssetPath +
                        " (der Ordner muss exakt 'Resources' heissen).");
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