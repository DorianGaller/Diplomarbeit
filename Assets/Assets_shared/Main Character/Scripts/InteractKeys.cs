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
    public Transform tilemapRoot;

    private Tilemap[] interactTilemaps;

    public LayerMask interactLayer; // im Inspector setzen

    // 🔹 Flags für zusätzliche Kontrolle
    public bool canShoot = true;
    public bool canInteract = true;

    void Awake()
    {
        gunshotAction = new InputAction("Gunshot", InputActionType.Button, "<Mouse>/leftButton");
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");

        CacheTilemaps();
    }

    void CacheTilemaps()
    {
        if (tilemapRoot == null)
        {
            Debug.LogError("Tilemap-Root fehlt!");
            return;
        }

        interactTilemaps = tilemapRoot.GetComponentsInChildren<Tilemap>(true);
    }

    void OnEnable()
    {
        EnableShooting(true);
        EnableInteraction(true);
    }

    void OnDisable()
    {
        EnableShooting(false);
        EnableInteraction(false);
    }

    // 🔹 Separate Steuerung
    public void EnableShooting(bool enable)
    {
        if (enable)
            gunshotAction.Enable();
        else
            gunshotAction.Disable();
    }

    public void EnableInteraction(bool enable)
    {
        if (enable)
            interactAction.Enable();
        else
            interactAction.Disable();
    }

    void Update()
    {
        if (canShoot && gunshotAction.WasPressedThisFrame())
            OnGunshot();

        if (canInteract && interactAction.WasPressedThisFrame())
            OnInteract();
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
        // 🔹 1. GameObjects prüfen
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactDistance, interactLayer);

        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(gameObject);
                return; // Wichtig! Tilemap nicht zusätzlich triggern
            }
        }

        // 🔹 2. Tilemap prüfen
        if (interactTilemaps == null) return;

        Vector3 playerPos = transform.position;

        foreach (var tilemap in interactTilemaps)
        {
            Vector3Int playerCell = tilemap.WorldToCell(playerPos);
            int radius = Mathf.CeilToInt(interactDistance);

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector3Int checkCell = new Vector3Int(playerCell.x + x, playerCell.y + y, playerCell.z);

                    TileBase tile = tilemap.GetTile(checkCell);

                    if (tile is InteractableTile interactableTile)
                    {
                        interactableTile.OnInteract(checkCell, tilemap, gameObject);
                        return;
                    }
                }
            }
        }
    }
}