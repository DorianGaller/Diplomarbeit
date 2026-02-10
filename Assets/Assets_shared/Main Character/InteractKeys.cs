using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InteractKeys : MonoBehaviour
{
    private InputAction gunshotAction;
    private InputAction interactAction;

    public GameObject gunshotPrefab;
    public Transform shootPoint;

    [Header("Tilemap Interaction")]
    public float interactDistance = 1f;

    [Tooltip("Zieh hier das Empty-Object rein, das alle Tilemaps enthält")]
    public Transform tilemapRoot;

    private Tilemap[] interactTilemaps;

    void Awake()
    {
        gunshotAction = new InputAction(
            name: "Gunshot",
            type: InputActionType.Button,
            binding: "<Mouse>/leftButton"
        );

        interactAction = new InputAction(
            name: "Interact",
            type: InputActionType.Button,
            binding: "<Keyboard>/e"
        );

        CacheTilemaps();
    }

    void CacheTilemaps()
    {
        if (tilemapRoot == null)
        {
            Debug.LogError("Tilemap-Root ist nicht gesetzt! Zieh dein Empty-Object mit den Tilemaps in den Inspector.");
            return;
        }

        interactTilemaps = tilemapRoot.GetComponentsInChildren<Tilemap>();
    }

    void OnEnable()
    {
        gunshotAction.Enable();
        interactAction.Enable();
    }

    void OnDisable()
    {
        gunshotAction.Disable();
        interactAction.Disable();
    }

    void Update()
    {
        if (gunshotAction.WasPressedThisFrame())
        {
            OnGunshot();
        }

        if (interactAction.WasPressedThisFrame())
        {
            OnInteract();
        }
    }

    private void OnGunshot()
    {
        if (!gunshotPrefab || !shootPoint) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 shootDir = (mouseWorldPos - shootPoint.position).normalized;

        GameObject shot = Instantiate(gunshotPrefab, shootPoint.position, Quaternion.identity);
        shot.GetComponent<GunshotEffect>()?.Init(shootDir);
    }

    private void OnInteract()
    {
        if (interactTilemaps == null || interactTilemaps.Length == 0)
        {
            Debug.LogWarning("Keine Tilemaps gefunden!");
            return;
        }

        Vector3 direction = Vector3.right; // TODO: später dynamisch
        Vector3 worldPos = transform.position + direction * interactDistance;

        foreach (var tilemap in interactTilemaps)
        {
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            TileBase tile = tilemap.GetTile(cellPos);

            if (tile is InteractableTile interactableTile)
            {
                interactableTile.OnInteract(cellPos, tilemap, gameObject);
                return;
            }
        }

        Debug.Log("Kein interagierbares Tile gefunden.");
    }
}
