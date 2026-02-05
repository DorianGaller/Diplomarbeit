using UnityEngine;
using UnityEngine.UI; // wichtig!

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Scaling")]
    public float xpMultiplier = 1.5f;

    [Header("UI")]
    public Image xpBar; // <- deine LevelProgress Image hier reinziehen

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateXPBar(); // <- Bar aktualisieren
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);

        Debug.Log("LEVEL UP! Neues Level: " + level);
    }

    void UpdateXPBar()
    {
        xpBar.fillAmount = (float)currentXP / xpToNextLevel;
    }
}
