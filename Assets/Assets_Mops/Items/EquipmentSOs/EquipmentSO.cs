using UnityEngine;

[CreateAssetMenu]
public class EquipmentSO : ScriptableObject
{

    public string itemName;
    public int attack, defense, agility;
    
    [SerializeField]
    private Sprite itemSprite;


    public void PreviewEquipment()
    {
        GameObject.Find("StatManager").GetComponent<PlayerStats>().
            PreviewStats(attack, defense, agility, itemSprite);
    }

    public void EquipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.attack += attack;
        playerStats.defense += defense;
        playerStats.agility += agility;

        playerStats.UpdateEquipmentStats();
    }

    public void UnequipItem()
    {
        PlayerStats playerStats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
        playerStats.attack -= attack;
        playerStats.defense -= defense;
        playerStats.agility -= agility;

        playerStats.UpdateEquipmentStats();
    }
}

