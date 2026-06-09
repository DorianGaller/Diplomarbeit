using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int attack, defense, agility;

    [SerializeField]
    private TMP_Text attackText, defenseText, agilityText;

    [SerializeField]
    private TMP_Text attackPreText, defensePreText, agilityPreText;

    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private GameObject selectedItemStats;

    [SerializeField]
    private GameObject selcteedItemImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateEquipmentStats();
    }

    public void UpdateEquipmentStats()
    {
        attackText.text = attack.ToString();
        defenseText.text = defense.ToString();
        agilityText.text = agility.ToString();
    }

    public void PreviewStats(int attack, int defense, int agility, Sprite itemSprite)
    {
        attackPreText.text = attack.ToString();
        defensePreText.text = defense.ToString();
        agilityPreText.text = agility.ToString();

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