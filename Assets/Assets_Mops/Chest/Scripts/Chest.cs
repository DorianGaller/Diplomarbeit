using UnityEngine;
public class Chest : MonoBehaviour
{
    [System.Serializable]
    public class ChestItem
    {
        public string itemName;
        public int quantity;
        public Sprite itemSprite;
        [TextArea] public string itemDescription;
        public ItemSO itemSO;
        public ItemType itemType;
    }

    [Header("Items manuell befüllen")]
    public ChestItem[] chestItems;
    [Header("ODER: ItemSO Assets reinziehen")]
    public ItemSO[] itemSOs;
    public int[] itemSOQuantities;
    public ChestUI chestUI;

    private bool isOpen = false;

    private void Awake()
    {
        if (itemSOs != null && itemSOs.Length > 0
        && (chestItems == null || chestItems.Length == 0))
        {
            chestItems = new ChestItem[itemSOs.Length];
            for (int i = 0; i < itemSOs.Length; i++)
            {
                if (itemSOs[i] == null) continue;
                chestItems[i] = new ChestItem
                {
                    itemName        = itemSOs[i].itemName,
                    quantity        = (itemSOQuantities != null && i < itemSOQuantities.Length)
                                      ? itemSOQuantities[i] : 1,
                    itemSprite      = itemSOs[i].itemSprite,
                    itemDescription = itemSOs[i].itemDescription,
                    itemSO          = itemSOs[i],
                    itemType        = itemSOs[i].itemType
                };
            }
        }
    }

    public void OpenChest()
    {
        if (isOpen) return;
        isOpen = true;
        chestUI.LoadAndOpen(this);
    }

    public void CloseChest()
    {
        isOpen = false;
        GameObject.Find("InventoryCanvas")
            .GetComponent<InventoryManager>()
            .CloseChestView();
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= chestItems.Length) return;
        chestItems[index].quantity = 0;
    }
}