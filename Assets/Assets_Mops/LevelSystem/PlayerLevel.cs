gitusing UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Scaling")]
    public float xpMultiplier = 1.5f; // wie stark XP pro Level steigt

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);

        Debug.Log("LEVEL UP! Neues Level: " + level);

        // Optional:
        // Mehr Leben
        // Mehr Schaden
        // Effekt abspielen
    }
}
