using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{
    public string itemName;
    public int attack, defense, agility, health, weight;   // NEU: weight

    [Header("Main Hand Anzeige")]
    [Tooltip("Groesse des Items in der Hand (1 = normal, 0.5 = halb so gross, 2 = doppelt so gross)")]
    [Min(0.01f)] public float heldScale = 1f;

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

        playerStats.UpdateEquipmentStats();
    }
}