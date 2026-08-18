using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{
    public string itemName;
    public int attack, defense, agility, health, weight;   // NEU: weight

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