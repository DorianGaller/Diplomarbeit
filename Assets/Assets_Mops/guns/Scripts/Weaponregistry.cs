using UnityEngine;
using System.Collections.Generic;
 
/// <summary>
/// Haelt alle existierenden Waffen-Exemplare. Szenenuebergreifend via DontDestroyOnLoad.
///
/// Die Inventar-Slots speichern nur die instanceId als String — der eigentliche
/// Fortschritt liegt hier. Dadurch koennen zwei Waffen derselben Klasse
/// unterschiedliche Level haben, ohne dass das Inventar davon wissen muss.
///
/// SETUP: keines noetig. Die Registry legt sich beim ersten Zugriff selbst an.
/// Wer sie im Inspector sehen will, haengt das Skript auf ein leeres GameObject
/// in der Startszene — dann ist die Liste im Play Mode einsehbar.
/// </summary>
public class WeaponRegistry : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
 
    private static WeaponRegistry instance;
    private static bool isQuitting;
 
    public static WeaponRegistry Instance
    {
        get
        {
            if (isQuitting) return null;
 
            if (instance == null)
            {
                instance = FindFirstObjectByType<WeaponRegistry>();
 
                if (instance == null)
                {
                    var go = new GameObject("WeaponRegistry");
                    instance = go.AddComponent<WeaponRegistry>();
                }
            }
            return instance;
        }
    }
 
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
 
        instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildLookup();
    }
 
    private void OnApplicationQuit() => isQuitting = true;
 
    // ── Daten ─────────────────────────────────────────────
 
    [Tooltip("Alle existierenden Waffen-Exemplare. Im Play Mode zum Mitlesen.")]
    [SerializeField] private List<WeaponInstance> weapons = new List<WeaponInstance>();
 
    private Dictionary<string, WeaponInstance> lookup;
 
    private void RebuildLookup()
    {
        lookup = new Dictionary<string, WeaponInstance>();
 
        foreach (var w in weapons)
        {
            if (w == null || string.IsNullOrEmpty(w.instanceId)) continue;
            if (lookup.ContainsKey(w.instanceId)) continue;
 
            lookup.Add(w.instanceId, w);
        }
    }
 
    private Dictionary<string, WeaponInstance> Lookup
    {
        get
        {
            if (lookup == null) RebuildLookup();
            return lookup;
        }
    }
 
    // ── Zugriff ───────────────────────────────────────────
 
    /// <summary>
    /// Erzeugt ein neues Exemplar der Waffenklasse und gibt es zurueck.
    /// Muss an JEDER Stelle aufgerufen werden, an der eine Waffe entsteht
    /// (Shop-Kauf, Loot, Crafting) — sonst hat sie keine Identitaet.
    /// </summary>
    public static WeaponInstance Create(string weaponKey)
    {
        if (string.IsNullOrEmpty(weaponKey))
        {
            Debug.LogError("[WeaponRegistry] Create() ohne weaponKey.");
            return null;
        }
 
        WeaponRegistry reg = Instance;
        if (reg == null) return null;
 
        var inst = new WeaponInstance(reg.GenerateId(), weaponKey);
 
        reg.weapons.Add(inst);
        reg.Lookup.Add(inst.instanceId, inst);
 
        return inst;
    }
 
    /// <summary>Liefert das Exemplar oder null, wenn die ID unbekannt ist.</summary>
    public static WeaponInstance Get(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return null;
 
        WeaponRegistry reg = Instance;
        if (reg == null) return null;
 
        reg.Lookup.TryGetValue(instanceId, out WeaponInstance inst);
        return inst;
    }
 
    public static bool Exists(string instanceId)
    {
        return Get(instanceId) != null;
    }
 
    /// <summary>
    /// Loescht ein Exemplar endgueltig. Nur aufrufen, wenn die Waffe
    /// wirklich aus dem Spiel verschwindet (Verkauf, Verschrottung).
    /// NICHT beim Ablegen oder Umsortieren.
    /// </summary>
    public static bool Remove(string instanceId)
    {
        WeaponInstance inst = Get(instanceId);
        if (inst == null) return false;
 
        WeaponRegistry reg = Instance;
        reg.weapons.Remove(inst);
        reg.Lookup.Remove(instanceId);
        return true;
    }
 
    /// <summary>Alle Exemplare — fuer Save/Load und Debug-Anzeigen.</summary>
    public static IReadOnlyList<WeaponInstance> All =>
        Instance != null ? Instance.weapons : new List<WeaponInstance>();
 
    // ── ID-Erzeugung ──────────────────────────────────────
 
    private string GenerateId()
    {
        // Kurz genug zum Mitlesen im Inspector, lang genug gegen Kollisionen.
        for (int attempt = 0; attempt < 16; attempt++)
        {
            string id = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            if (!Lookup.ContainsKey(id)) return id;
        }
 
        // Praktisch unerreichbar — dann eben die volle GUID.
        return System.Guid.NewGuid().ToString("N");
    }
 
    // ── Debug ─────────────────────────────────────────────
 
    [ContextMenu("Log All Weapons")]
    private void LogAll()
    {
        if (weapons.Count == 0)
        {
            Debug.Log("[WeaponRegistry] Keine Waffen registriert.");
            return;
        }
 
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[WeaponRegistry] {weapons.Count} Exemplar(e):");
        foreach (var w in weapons)
            sb.AppendLine("  " + w);
 
        Debug.Log(sb.ToString());
    }
}
 