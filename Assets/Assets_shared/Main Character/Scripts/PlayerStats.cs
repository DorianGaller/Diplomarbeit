using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int attack, defense, agility, health, weight;   // NEU: weight

    public int coins;

    [SerializeField]
    private TMP_Text attackText, defenseText, agilityText, healthText, weightText;   // NEU: weightText

    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, agilityPreText, healthPreText, weightPreText;   // NEU

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selcteedItemImage;

    [SerializeField]
    private PlayerLife playerLife;

    [Header("Weight Penalty")]
    [SerializeField] private PlayerMovement playerMovement;   // NEU
    [Tooltip("Wie stark 1 Gewichtspunkt die Geschwindigkeit prozentual verringert")]
    [Range(0f, 0.05f)]
    public float speedPenaltyPerWeight = 0.005f;   // 0.5% pro Gewichtspunkt
    [Tooltip("Maximale Verlangsamung, egal wie schwer die Ausrüstung insgesamt wird")]
    [Range(0f, 1f)]
    public float maxSpeedPenalty = 0.3f;   // nie mehr als 30% langsamer

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
            weightText.text = weight.ToString();   // NEU

        if (playerLife != null)
        {
            playerLife.SetBonusHP(health);
            if (healthText != null)
                healthText.text = playerLife.maxHP.ToString();
        }

        ApplyWeightPenalty();   // NEU
    }

    void ApplyWeightPenalty()   // NEU
    {
        if (playerMovement == null) return;

        float penalty = Mathf.Clamp(weight * speedPenaltyPerWeight, 0f, maxSpeedPenalty);
        float multiplier = 1f - penalty;

        playerMovement.SetWeightPenalty(multiplier);
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

    public void PreviewStats(int attack, int defense, int agility, int health, int weight, Sprite itemSprite)   // NEU: weight-Parameter
    {
        attackPreText.text = attack.ToString();
        defensePreText.text = defense.ToString();
        agilityPreText.text = agility.ToString();
        if (healthPreText != null)
            healthPreText.text = health.ToString();
        if (weightPreText != null)
            weightPreText.text = weight.ToString();   // NEU
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