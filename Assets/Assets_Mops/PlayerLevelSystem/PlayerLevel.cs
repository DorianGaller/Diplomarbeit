using UnityEngine;
using UnityEngine.UI;
using TMPro; // <-- TextMeshPro

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Scaling")]
    public float xpMultiplier = 1.5f;

    [Header("UI")]
    public Image xpBar;                 // Progress-Bar (Image mit Fill)
    public TMP_Text currentXPText;      // Aktuelle XP (TMP Text)         // Optional: Level-Anzeige

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpMultiplier);

        Debug.Log("LEVEL UP! Neues Level: " + level);
    }

    void UpdateUI()
    {
        UpdateXPBar();
        UpdateXPText();
    }

    void UpdateXPBar()
    {
        if (xpBar != null)
            xpBar.fillAmount = (float)currentXP / xpToNextLevel;
    }

    void UpdateXPText()
    {
        if (currentXPText != null)
            currentXPText.text = $"{currentXP} / {xpToNextLevel} XP";
    }

    // Optional: Beim Start direkt UI korrekt setzen
    void Start()
    {
        UpdateUI();
    }
}
