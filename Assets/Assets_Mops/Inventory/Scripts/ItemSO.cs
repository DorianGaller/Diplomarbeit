using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    [TextArea] public string itemDescription;

    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributeToChange attributeToChange = new AttributeToChange();
    public int amountToChangeAttribute;

    public ItemType itemType;

    public bool UseItem()
    {
        GameObject player = GameObject.FindWithTag("Player");
        PlayerLife playerLife = player != null ? player.GetComponent<PlayerLife>() : null;
        if (playerLife == null) return false;

        if (statToChange == StatToChange.health)
        {
            playerLife.Heal(amountToChangeStat);
        }
        else if (statToChange == StatToChange.maxHealth)
        {
            playerLife.IncreaseMaxHP(amountToChangeStat);

            // HP-Anzeige im Stat-Fenster aktualisieren
            GameObject statManager = GameObject.Find("StatManager");
            if (statManager != null)
                statManager.GetComponent<PlayerStats>()?.UpdateEquipmentStats();
        }
        return true;
    }

    public enum StatToChange
    {
        none,       
        health,     
        maxHealth,  
    }

    public enum AttributeToChange
    {
        none,
        strength,
        defense,
        agility,
    }
}