using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopEntryUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image iconImage;
    public TMP_Text nameLabel;
    public TMP_Text priceLabel;
    public Button buyButton;

    private ShopEntryData entry;
    private ShopApp shop;

    public void Setup(ShopEntryData entryData, ShopApp shopApp)
    {
        entry = entryData;
        shop = shopApp;

        if (iconImage != null) iconImage.sprite = entry.item.itemSprite;
        if (nameLabel != null) nameLabel.text = entry.item.itemName;
        if (priceLabel != null) priceLabel.text = entry.price.ToString();

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => shop.TryBuy(entry));
        }
    }
}