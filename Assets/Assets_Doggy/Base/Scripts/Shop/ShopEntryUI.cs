using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopEntryUI : MonoBehaviour
{
    [Header("UI Refs")]
    public Image iconImage;
    public TMP_Text nameLabel;
    public TMP_Text priceLabel;

    [Tooltip("Optional: zeigt an, wie viele Stueck der Spieler besitzt.")]
    public TMP_Text ownedLabel;

    public Button buyButton;
    public Button sellButton;

    private ShopEntryData entry;
    private ShopApp shop;

    public void Setup(ShopEntryData entryData, ShopApp shopApp)
    {
        entry = entryData;
        shop = shopApp;

        if (iconImage != null)
        {
            iconImage.sprite = entry.item.itemSprite;
            // Ohne Sprite zeichnet Unity ein weisses Quad - lieber ganz ausblenden.
            iconImage.enabled = entry.item.itemSprite != null;
        }

        if (nameLabel != null) nameLabel.text = entry.item.itemName;
        if (priceLabel != null) priceLabel.text = entry.price.ToString();

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
        else
        {
            Debug.LogError("[ShopEntryUI] buyButton ist nicht zugewiesen.");
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }
        else
        {
            Debug.LogError("[ShopEntryUI] sellButton ist nicht zugewiesen " +
                           "-> Verkaufen kann nicht funktionieren.");
        }

        Refresh();
    }

    private void OnBuyClicked()
    {
        if (shop != null) shop.TryBuy(entry);
    }

    private void OnSellClicked()
    {
        if (shop != null) shop.TrySell(entry);
    }

    // Besitzanzeige und Button-Zustaende an die aktuelle Lage anpassen.
    public void Refresh()
    {
        if (shop == null || entry == null) return;

        int owned = shop.GetOwned(entry);

        if (ownedLabel != null)
            ownedLabel.text = owned > 0 ? owned.ToString() : "";

        if (buyButton != null)
            buyButton.interactable = shop.CanAfford(entry);

        if (sellButton != null)
            sellButton.interactable = shop.CanSell(entry);
    }
}