using UnityEngine;
using TMPro;

/// <summary>
/// Kleines Info-Fenster mit Item-Name und Beschreibung (z.B. Rechtsklick im Equipment-Inventar).
/// Haengt an einem IMMER aktiven Objekt im InventoryCanvas, das Fenster selbst ist ein Kind davon.
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("Referenzen")]
    [SerializeField] private RectTransform panel;          // das sichtbare Fenster
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Verhalten")]
    [SerializeField] private Vector2 offset = new Vector2(18f, 18f);
    [Tooltip("Fenster schliesst sich, sobald keines dieser Menues mehr offen ist (z.B. EquipmentMenu)")]
    [SerializeField] private GameObject[] onlyWhileOpen;

    private Canvas canvas;

     void Awake()
    {
        // Player-Duplikat beim Szenenwechsel ignorieren (siehe DragDropManager)
        if (Instance != null && Instance != this) return;

        Instance = this;

        canvas = GetComponentInParent<Canvas>();
        if (panel != null) panel.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string itemName, string fallbackDescription, Vector2 screenPos)
    {
        if (panel == null) return;

        string desc = ItemDatabase.GetDescription(itemName);
        if (string.IsNullOrEmpty(desc)) desc = fallbackDescription;

        if (nameText != null) nameText.text = itemName;
        if (descriptionText != null) descriptionText.text = desc;

        panel.gameObject.SetActive(true);
        transform.SetAsLastSibling();          // ueber allen Menues zeichnen
        PlaceAt(screenPos);
    }

    public void Hide()
    {
        if (panel != null) panel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (panel == null || !panel.gameObject.activeSelf) return;

        // Schliessen mit Linksklick, ESC oder wenn das Menue zugeht
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || !AnyMenuOpen())
            Hide();
    }

    bool AnyMenuOpen()
    {
        if (onlyWhileOpen == null || onlyWhileOpen.Length == 0) return true;
        foreach (GameObject go in onlyWhileOpen)
            if (go != null && go.activeInHierarchy) return true;
        return false;
    }

    // Fenster neben die Maus setzen und so ausrichten, dass es im Bild bleibt
    void PlaceAt(Vector2 screenPos)
    {
        bool right = screenPos.x > Screen.width * 0.6f;    // rechts am Rand -> Fenster nach links
        bool low = screenPos.y < Screen.height * 0.4f;     // unten am Rand -> Fenster nach oben

        panel.pivot = new Vector2(right ? 1f : 0f, low ? 0f : 1f);
        Vector2 pos = screenPos + new Vector2(right ? -offset.x : offset.x, low ? offset.y : -offset.y);

        RectTransform parent = panel.parent as RectTransform;
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, pos, cam, out Vector2 local))
            panel.localPosition = local;
    }
}