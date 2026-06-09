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

    [Header("GameObjects reinziehen")]
    public GameObject[] itemObjects;        // ← GameObjects statt ItemSOs
    public int[] itemObjectQuantities;

    public ChestUI chestUI;
    private bool isOpen = false;

    private void Awake()
    {
        if (itemObjects != null && itemObjects.Length > 0
            && (chestItems == null || chestItems.Length == 0))
        {
            chestItems = new ChestItem[itemObjects.Length];
            for (int i = 0; i < itemObjects.Length; i++)
            {
                if (itemObjects[i] == null) continue;

                // ItemSO vom GameObject holen (falls vorhanden)
                ItemSO so = itemObjects[i].GetComponent<ItemSO>();

                chestItems[i] = new ChestItem
                {
                    itemName        = so != null ? so.itemName : itemObjects[i].name,
                    quantity        = (itemObjectQuantities != null && i < itemObjectQuantities.Length)
                                      ? itemObjectQuantities[i] : 1,
                    itemSprite      = so != null ? so.itemSprite : null,
                    itemDescription = so != null ? so.itemDescription : "",
                    itemSO          = so,
                    itemType        = so != null ? so.itemType : default
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