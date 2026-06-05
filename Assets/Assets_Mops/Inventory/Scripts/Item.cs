using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    public string itemName;

    [SerializeField]
    public int quantity;

    [SerializeField]
    public Sprite sprite;

    [TextArea]
    [SerializeField]
    public string itemDescription;

    private InventoryManager inventoryManager;

    public ItemType itemType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
{
    inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InventoryManager>();
    
    if(inventoryManager == null)
        Debug.LogError("InventoryManager nicht gefunden!");
}

    private void OnCollisionEnter2D(Collision2D collision)
{
    if(collision.gameObject.tag == "Player")
    {
        int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription, itemType);
        if (leftOverItems <= 0)
            Destroy(gameObject);
        else
            quantity = leftOverItems;
    }
}
}
