using UnityEngine;

/// <summary>
/// Zentrale Formeln fuer das Waffen-Levelsystem.
///
/// Bewusst statisch und ohne Abhaengigkeit zu Instanzen oder UI:
/// alles hier ist eine reine Funktion (gleiche Eingabe -> gleiche Ausgabe).
/// Dadurch koennen Kampf, Upgrade-Panel und spaeteres Save/Load dieselben
/// Werte berechnen, ohne dass einer den anderen kennen muss.
///
/// Balancing laeuft ueber die Konstanten hier oben und ueber
/// baseDamage / growthPerLevel im jeweiligen WeaponSO.
/// </summary>
public static class WeaponStats
{
    // ── Balancing-Konstanten ──────────────────────────────

    /// <summary>Maximale Anzahl Sterne (Ascension-Stufen). Harter Stopp.</summary>
    public const int MaxStars = 5;

    /// <summary>Wie viele Level ein Stern freischaltet.</summary>
    public const int LevelsPerStar = 10;

    /// <summary>EXP-Bedarf fuer den Sprung von Level 1 auf 2.</summary>
    public const int BaseExpPerLevel = 100;

    /// <summary>Um wie viel der EXP-Bedarf pro Level steigt.</summary>
    public const int ExpGrowthPerLevel = 25;

    // ── Level und Cap ─────────────────────────────────────

    /// <summary>Hoechstes erreichbares Level bei der gegebenen Sternzahl.</summary>
    public static int GetMaxLevel(int stars)
    {
        return Mathf.Clamp(stars, 1, MaxStars) * LevelsPerStar;
    }

    /// <summary>Absolutes Level-Maximum im Spiel (5 Sterne).</summary>
    public static int AbsoluteMaxLevel => MaxStars * LevelsPerStar;

    public static bool IsAtLevelCap(int level, int stars)
    {
        return level >= GetMaxLevel(stars);
    }

    public static bool CanAscend(int level, int stars)
    {
        return stars < MaxStars && IsAtLevelCap(level, stars);
    }

    // ── EXP ───────────────────────────────────────────────

    /// <summary>EXP-Bedarf, um von <paramref name="level"/> auf das naechste Level zu kommen.</summary>
    public static int GetExpForNextLevel(int level)
    {
        int lvl = Mathf.Max(1, level);
        return BaseExpPerLevel + (lvl - 1) * ExpGrowthPerLevel;
    }

    /// <summary>
    /// Wendet EXP an und rechnet Level-Ups aus. Ueberschuss wandert ins naechste Level,
    /// mehrere Level-Ups auf einmal sind moeglich. Am Cap wird der Rest verworfen und
    /// der Balken bleibt voll stehen.
    ///
    /// Reine Rechnung — veraendert nichts, gibt das Ergebnis nur zurueck.
    /// So kann das UI dieselbe Methode fuer die Vorschau benutzen wie der Apply-Button.
    /// </summary>
    public static void ApplyExp(
        int currentLevel, int currentExp, int stars, int addedExp,
        out int newLevel, out int newExp, out int levelsGained)
    {
        newLevel = Mathf.Max(1, currentLevel);
        newExp = Mathf.Max(0, currentExp) + Mathf.Max(0, addedExp);

        int maxLevel = GetMaxLevel(stars);
        int startLevel = newLevel;

        while (newLevel < maxLevel)
        {
            int needed = GetExpForNextLevel(newLevel);
            if (newExp < needed) break;

            newExp -= needed;
            newLevel++;
        }

        // Am Cap: Rest verfaellt, Balken zeigt voll.
        // Hinweis: bei der Ascension muss newExp auf 0 zurueckgesetzt werden,
        // sonst gibt es sofort ein Gratis-Level. Kommt in Step 4.
        if (newLevel >= maxLevel)
            newExp = Mathf.Min(newExp, GetExpForNextLevel(maxLevel));

        levelsGained = newLevel - startLevel;
    }

    /// <summary>Fuellstand des EXP-Balkens, 0..1.</summary>
    public static float GetExpProgress(int level, int currentExp, int stars)
    {
        int needed = GetExpForNextLevel(level);
        if (needed <= 0) return 1f;

        if (IsAtLevelCap(level, stars))
            return 1f;

        return Mathf.Clamp01((float)currentExp / needed);
    }

    // ── Schaden ───────────────────────────────────────────

    /// <summary>
    /// Schaden der Waffe auf dem gegebenen Level.
    /// Level 1 entspricht immer exakt baseDamage.
    /// </summary>
    public static int GetDamage(WeaponSO weapon, int level)
    {
        if (weapon == null) return 0;

        int lvl = Mathf.Max(1, level);
        float raw = weapon.baseDamage * (1f + weapon.growthPerLevel * (lvl - 1));
        return Mathf.RoundToInt(raw);
    }

    /// <summary>
    /// Schaden nach dem naechsten Level-Up — fuer die gruene Vorschauzahl im Panel.
    /// Am Cap identisch zum aktuellen Wert.
    /// </summary>
    public static int GetDamageAtNextLevel(WeaponSO weapon, int level, int stars)
    {
        if (weapon == null) return 0;

        if (IsAtLevelCap(level, stars))
            return GetDamage(weapon, level);

        return GetDamage(weapon, level + 1);
    }
}