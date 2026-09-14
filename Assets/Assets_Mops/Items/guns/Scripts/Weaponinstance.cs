using UnityEngine;
 
/// <summary>
/// Der Fortschritt EINES konkreten Waffen-Exemplars.
///
/// Bewusst nur primitive Felder (string, int) — damit ist die Klasse direkt
/// per JsonUtility speicherbar, ohne Sonderbehandlung. Alles, was sich
/// ableiten laesst (Schaden, Max-Level, WeaponSO), sind Properties und
/// werden NICHT mitgespeichert.
///
/// Die Verbindung zur Waffenklasse laeuft ueber weaponKey -> ItemDatabase -> WeaponSO.
/// </summary>
[System.Serializable]
public class WeaponInstance
{
    [Tooltip("Eindeutige ID dieses Exemplars. Wandert mit dem Item durch die Slots.")]
    public string instanceId;
 
    [Tooltip("itemName der Waffenklasse. UNVERAENDERLICH — hierueber wird das " +
             "WeaponSO aufgeloest. Nicht mit customName verwechseln.")]
    public string weaponKey;
 
    [Tooltip("Frei vom Spieler vergebener Name. Leer = weaponKey wird angezeigt.")]
    public string customName;
 
    [Tooltip("Ascension-Stufe, 1 bis WeaponStats.MaxStars.")]
    public int stars = 1;
 
    public int level = 1;
    public int currentExp = 0;
 
    public WeaponInstance() { }
 
    public WeaponInstance(string instanceId, string weaponKey)
    {
        this.instanceId = instanceId;
        this.weaponKey = weaponKey;
    }
 
    // ── Abgeleitete Werte (nicht serialisiert) ────────────
 
    /// <summary>Was im Upgrade-Panel als Titel steht.</summary>
    public string DisplayName =>
        string.IsNullOrEmpty(customName) ? weaponKey : customName;
 
    /// <summary>Die Waffenklasse. Null, wenn der weaponKey nicht in der ItemDatabase steht.</summary>
    public WeaponSO Weapon => ItemDatabase.GetWeapon(weaponKey);
 
    public int MaxLevel => WeaponStats.GetMaxLevel(stars);
 
    public int CurrentDamage => WeaponStats.GetDamage(Weapon, level);
 
    /// <summary>Schaden nach dem naechsten Level-Up — die gruene Zahl im Panel.</summary>
    public int NextLevelDamage => WeaponStats.GetDamageAtNextLevel(Weapon, level, stars);
 
    public int ExpForNextLevel => WeaponStats.GetExpForNextLevel(level);
 
    /// <summary>Fuellstand des EXP-Balkens, 0..1.</summary>
    public float ExpProgress => WeaponStats.GetExpProgress(level, currentExp, stars);
 
    public bool IsAtLevelCap => WeaponStats.IsAtLevelCap(level, stars);
 
    public bool CanAscend => WeaponStats.CanAscend(level, stars);
 
    // ── Veraenderung ──────────────────────────────────────
 
    /// <summary>
    /// Fuegt EXP hinzu und rechnet Level-Ups aus. Gibt zurueck, wie viele
    /// Level dazugekommen sind. Am Cap passiert nichts.
    /// </summary>
    public int AddExp(int amount)
    {
        if (amount <= 0 || IsAtLevelCap) return 0;
 
        WeaponStats.ApplyExp(level, currentExp, stars, amount,
                             out int newLevel, out int newExp, out int gained);
 
        level = newLevel;
        currentExp = newExp;
        return gained;
    }
 
    public override string ToString()
    {
        return $"{DisplayName} [{weaponKey}] id={instanceId} " +
               $"Lv{level}/{MaxLevel} ★{stars} exp={currentExp}/{ExpForNextLevel}";
    }
}
 