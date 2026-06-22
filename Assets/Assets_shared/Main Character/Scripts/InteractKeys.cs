using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InteractKeys : MonoBehaviour
{
    private InputAction gunshotAction;
    private InputAction interactAction;

    // gunshotPrefab entfernt — kommt jetzt vom WeaponHolder
    public Transform shootPoint;

    [Header("Tilemap Interaction")]
    public float interactDistance = 1f;
    //public Transform tilemapRoot;

    private Tilemap[] interactTilemaps;
    public LayerMask interactLayer;

    // 🔹 Flags für zusätzliche Kontrolle
    public bool canShoot = true;
    public bool canInteract = true;

    // 🔹 Schussrate-Tracking
    private float nextFireTime = 0f;

    // 🔹 Referenz auf WeaponHolder (selbes GameObject)
    private WeaponHolder weaponHolder;

    void Awake()
    {
        gunshotAction = new InputAction("Gunshot", InputActionType.Button, "<Mouse>/leftButton");
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");

        weaponHolder = GetComponent<WeaponHolder>();

        CacheTilemaps();
    }

    void CacheTilemaps()
{
    GameObject[] tilemapObjects = GameObject.FindGameObjectsWithTag("Tilemap");

    if (tilemapObjects.Length == 0)
    {
        Debug.LogWarning("Keine GameObjects mit Tag 'Tilemap' gefunden!");
        return;
    }

    var tilemapList = new System.Collections.Generic.List<Tilemap>();

    foreach (var go in tilemapObjects)
    {
        // Tilemap auf dem GameObject selbst oder in Kindern suchen
        tilemapList.AddRange(go.GetComponentsInChildren<Tilemap>(true));
    }

    interactTilemaps = tilemapList.ToArray();
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

    public void EnableShooting(bool enable)
    {
        if (enable) gunshotAction.Enable();
        else gunshotAction.Disable();
    }

    public void EnableInteraction(bool enable)
    {
        if (enable) interactAction.Enable();
        else interactAction.Disable();
    }

    void Update()
    {
        if (canShoot)
        {
            WeaponSO weapon = weaponHolder?.CurrentWeapon;

            bool shouldShoot = weapon != null && weapon.firingMode == FiringMode.FullAuto
                ? gunshotAction.IsPressed()           // FullAuto: Halten
                : gunshotAction.WasPressedThisFrame(); // SemiAuto: einmal pro Klick

            if (shouldShoot)
                OnGunshot();
        }

        if (canInteract && interactAction.WasPressedThisFrame())
            OnInteract();
    }

    private void OnGunshot()
    {
        if (!shootPoint) return;

        // 🔹 Waffe vom WeaponHolder holen
        WeaponSO weapon = weaponHolder?.CurrentWeapon;
        if (weapon == null || weapon.bulletPrefab == null) return;

        // 🔹 Schussrate prüfen
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + weapon.fireRate;

        // Richtung berechnen
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        Vector2 baseDir = (mouseWorldPos - shootPoint.position).normalized;

        // 🔹 Mehrere Bullets (Shotgun etc.) mit optionalem Spread
        for (int i = 0; i < weapon.bulletsPerShot; i++)
        {
            Vector2 shootDir = baseDir;

            if (weapon.spreadAngle > 0f)
            {
                float halfSpread = weapon.spreadAngle / 2f;
                // Bullets gleichmäßig verteilen wenn bulletsPerShot > 1, sonst random
                float angle = weapon.bulletsPerShot > 1
                    ? Mathf.Lerp(-halfSpread, halfSpread, (float)i / (weapon.bulletsPerShot - 1))
                    : Random.Range(-halfSpread, halfSpread);

                shootDir = Quaternion.Euler(0, 0, angle) * baseDir;
            }

            GameObject shot = Instantiate(weapon.bulletPrefab, shootPoint.position, Quaternion.identity);
            shot.GetComponent<GunshotEffect>()?.Init(shootDir);
        }
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
                return;
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