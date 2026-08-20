using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int attack, defense, agility, health, weight;

    public int coins;

    [SerializeField]
    private TMP_Text attackText, defenseText, agilityText, healthText, weightText;

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, agilityPreText, healthPreText, weightPreText;

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selcteedItemImage;

    [SerializeField]
    private PlayerLife playerLife;

    [Header("Weight Penalty")]
    [SerializeField] private PlayerMovement playerMovement;
    [Range(0f, 0.05f)]
    public float speedPenaltyPerWeight = 0.005f;
    [Range(0f, 1f)]
    public float maxSpeedPenalty = 0.3f;

    [Header("Agility Bonus")]   // NEU
    [Tooltip("Wie stark 1 Agility-Punkt die Geschwindigkeit prozentual erhöht")]
    [Range(0f, 0.05f)]
    public float speedBonusPerAgility = 0.01f;
    [Tooltip("Maximaler Geschwindigkeitsbonus durch Agility")]
    [Range(0f, 1f)]
    public float maxSpeedBonus = 0.5f;

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

        if (weightText != null)
            weightText.text = weight.ToString();

        if (playerLife != null)
        {
            playerLife.SetBonusHP(health);
            if (healthText != null)
                healthText.text = playerLife.maxHP.ToString();
        }

        ApplyWeightPenalty();
        ApplyAgilityBonus();   // NEU
    }

    void ApplyWeightPenalty()
    {
        if (playerMovement == null) return;

        float penalty = Mathf.Clamp(weight * speedPenaltyPerWeight, 0f, maxSpeedPenalty);
        float multiplier = 1f - penalty;

        playerMovement.SetWeightPenalty(multiplier);
    }

    void ApplyAgilityBonus()   // NEU
    {
        if (playerMovement == null) return;

        float bonus = Mathf.Clamp(agility * speedBonusPerAgility, 0f, maxSpeedBonus);
        float multiplier = 1f + bonus;

        playerMovement.SetAgilityBonus(multiplier);
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

    public void PreviewStats(int attack, int defense, int agility, int health, int weight, Sprite itemSprite)
    {
        attackPreText.text = attack.ToString();
        defensePreText.text = defense.ToString();
        agilityPreText.text = agility.ToString();
        if (healthPreText != null)
            healthPreText.text = health.ToString();
        if (weightPreText != null)
            weightPreText.text = weight.ToString();
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