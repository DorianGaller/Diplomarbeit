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
    }

    public ChestItem[] chestItems;
    public GameObject chestCanvas;       // Das ChestCanvas GameObject
    private ChestUI chestUI;
    private bool isOpen = false;

    void Start()
    {
        chestUI = chestCanvas.GetComponent<ChestUI>();
        chestCanvas.SetActive(false);
    }

    // Wird vom Button "Pick up" / "Shelf" etc. aufgerufen
    public void OpenChest()
    {
        if (isOpen) return;

        isOpen = true;
        Time.timeScale = 0f;
        chestCanvas.SetActive(true);
        chestUI.LoadChest(this);
    }

    public void CloseChest()
    {
        isOpen = false;
        Time.timeScale = 1f;
        chestCanvas.SetActive(false);
    }

    // Entfernt ein Item aus der Truhe nachdem es ins Inventar übertragen wurde
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= chestItems.Length) return;
        chestItems[index].quantity = 0;
    }
}