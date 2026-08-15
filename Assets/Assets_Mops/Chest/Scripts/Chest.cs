using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour, IInteractable
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
    public GameObject[] itemObjects;
    public int[] itemObjectQuantities;

    [Header("Chest UI (wird automatisch gefunden, falls leer)")]
    public ChestUI chestUI;

    private bool isOpen = false;

    [Header("Interaction Hint")]
    [SerializeField] private GameObject interactionHint;
    [SerializeField] private float hintRange = 2f;

    private Transform player;

    private void Awake()
    {
        if (itemObjects != null && itemObjects.Length > 0
            && (chestItems == null || chestItems.Length == 0))
        {
            chestItems = new ChestItem[itemObjects.Length];
            for (int i = 0; i < itemObjects.Length; i++)
            {
                if (itemObjects[i] == null) continue;

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

    private void Start()
    {
        StartCoroutine(InitNextFrame());
    }

    // NEU: wartet einen Frame, damit DontDestroy.cs eventuelle Duplikate
    // aus dieser Szene schon bereinigt hat, BEVOR wir nach der echten,
    // dauerhaften InventoryCanvas/ChestUI suchen
    private IEnumerator InitNextFrame()
    {
        yield return null;

        // Immer zur Laufzeit neu auflösen statt der evtl. veralteten Inspector-Referenz zu vertrauen
        GameObject canvas = GameObject.Find("InventoryCanvas");
        if (canvas != null)
        {
            ChestUI foundUI = canvas.GetComponentInChildren<ChestUI>(true);
            if (foundUI != null)
                chestUI = foundUI;
        }

        if (chestUI == null)
            Debug.LogError("Chest: Keine ChestUI gefunden! Weder Inspector-Referenz noch automatische Suche erfolgreich.");

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null || interactionHint == null) return;

        if (isOpen)
        {
            interactionHint.SetActive(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        interactionHint.SetActive(distance <= hintRange);
    }

    public void Interact(GameObject player)
    {
        if (isOpen)
            CloseChest();
        else
            OpenChest();
    }

    public void OpenChest()
    {
        if (isOpen) return;
        if (chestUI == null)
        {
            Debug.LogError("Chest: Kann nicht öffnen, keine ChestUI zugewiesen!");
            return;
        }
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