using UnityEngine;


[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributeToChange attributeToChange = new AttributeToChange();
    public int amountToChangeAttribute;

    public bool UseItem()
    {
        if(statToChange == StatToChange.health)
        {

            Debug.Log("Health um " + amountToChangeStat + " erhöhen");
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
