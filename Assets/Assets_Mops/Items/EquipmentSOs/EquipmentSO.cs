using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{
    public string itemName;
    public int attack, defense, agility, health, weight;   // NEU: weight

    [Header("Main Hand Anzeige")]
    [Tooltip("Groesse des Items in der Hand (1 = normal, 0.5 = halb so gross, 2 = doppelt so gross)")]
    [Min(0.01f)] public float heldScale = 1f;
    [Header("Schutz gegen Hacker")]
    [Tooltip("1 = keine Wirkung, 0.5 = Hack dauert halb so lang, 0 = immun (z.B. Firewall-Modul)")]
    [Range(0f, 1f)] public float hackDurationMultiplier = 1f;

    [SerializeField]
    private Sprite itemSprite;

    public void PreviewEquipment()
    {
        GameObject.Find("StatManager").GetComponent<PlayerStats>().
            PreviewStats(attack, defense, agility, health, weight, itemSprite);   // NEU
    }


    public void EquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.attack += attack;
        playerStats.defense += defense;
        playerStats.agility += agility;
        playerStats.health += health;
        playerStats.weight += weight;   // NEU

        if (hackDurationMultiplier < 1f)
            SetHackProtection(hackDurationMultiplier);

        playerStats.UpdateEquipmentStats();
    }

    public void UnequipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.attack -= attack;
        playerStats.defense -= defense;
        playerStats.agility -= agility;
        playerStats.health -= health;
        playerStats.weight -= weight;   // NEU
        if (hackDurationMultiplier < 1f)
            SetHackProtection(1f);

        playerStats.UpdateEquipmentStats();
    }

    private static void SetHackProtection(float multiplier)
    {
        GameObject player = GameObject.FindWithTag("Player");
        PlayerControlHack hack = player != null ? player.GetComponentInChildren<PlayerControlHack>() : null;
        if (hack != null) hack.hackDurationMultiplier = multiplier;
    }
}