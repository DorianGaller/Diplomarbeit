using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerStats : MonoBehaviour
{
    public int attack, defense, agility, health;

    public int coins;

    [SerializeField]
    private TMP_Text attackText, defenseText, agilityText, healthText;

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, agilityPreText, healthPreText;

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selcteedItemImage;

    [SerializeField]
    private PlayerLife playerLife;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Start()
    {
        UpdateEquipmentStats();
        UpdateCoinsDisplay();
    }

    public void UpdateEquipmentStats()
    {
        attackText.text = attack.ToString();
        defenseText.text = defense.ToString();
        agilityText.text = agility.ToString();

        if (playerLife != null)
        {
            playerLife.SetBonusHP(health);
            if (healthText != null)
                healthText.text = playerLife.maxHP.ToString();   // zeigt aktuellen maxHP-Wert
        }
    }

    public void UpdateCoinsDisplay()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsDisplay();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        UpdateCoinsDisplay();
        return true;
    }

    public void PreviewStats(int attack, int defense, int agility, int health, Sprite itemSprite)
    {
        attackPreText.text = attack.ToString();
        defensePreText.text = defense.ToString();
        agilityPreText.text = agility.ToString();
        if (healthPreText != null)
        healthPreText.text = health.ToString();
        previewImage.sprite = itemSprite;
        selectedItemStats.SetActive(true);
        selcteedItemImage.SetActive(true);
    }

    public void TurnOffPreviewStats()
    {
        selectedItemStats.SetActive(false);
        selcteedItemImage.SetActive(false);
    }
}