using UnityEngine;

[System.Serializable]
public class ShopEntryData
{
    [Tooltip("Bestehendes ItemSO, das gekauft werden kann.")]
    public ItemSO item;

    [Tooltip("Preis in Coins.")]
    public int price = 50;

    [Tooltip("Menge pro Kauf.")]
    public int quantity = 1;
}