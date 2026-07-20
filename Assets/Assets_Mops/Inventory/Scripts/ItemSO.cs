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
        if (statToChange == StatToChange.health)
        {
            PlayerLife playerLife = GameObject.FindWithTag("Player").GetComponent<PlayerLife>();
            if (playerLife != null)
                playerLife.Heal(amountToChangeStat);
        }
        return true;
    }

    public enum StatToChange
    {
        none,
        health,
        hearts,
    }

    public enum AttributeToChange
    {
        none,
        strength,
        defense,
        agility,
    }
}