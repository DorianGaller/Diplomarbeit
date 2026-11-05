using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

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

    private PlayerEquipmentOrbit orbit;

    [Header("Nahschuss")]
    [Tooltip("Dicke der Pruefung zwischen Spieler und Muendung")]
    public float pointBlankRadius = 0.25f;

    void Awake()
    {
        gunshotAction = new InputAction("Gunshot", InputActionType.Button, "<Mouse>/leftButton");
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");

        weaponHolder = GetComponent<WeaponHolder>();

        orbit = GetComponent<PlayerEquipmentOrbit>();

        CacheTilemaps();
    }

    void CacheTilemaps()
    {
        GameObject[] tilemapObjects = GameObject.FindGameObjectsWithTag("Tilemap");

        if (tilemapObjects.Length == 0)
        {
            Debug.LogWarning("Keine GameObjects mit Tag 'Tilemap' gefunden!");
            interactTilemaps = null;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        EnableShooting(false);
        EnableInteraction(false);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheTilemaps();
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

        Vector2 baseDir = orbit != null ? orbit.AimDirection : (Vector2)shootPoint.right;

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

            if (HandlePointBlank(shootDir, weaponHolder.CurrentDamage))
                continue;

            GameObject shot = Instantiate(weapon.bulletPrefab, shootPoint.position, Quaternion.identity);

            GunshotEffect effect = shot.GetComponent<GunshotEffect>();
            if (effect != null)
            {
                effect.damage = weaponHolder.CurrentDamage;
                effect.fromPlayer = true;
                effect.Init(shootDir);
            }
        }
    }

    private bool HandlePointBlank(Vector2 dir, int damage)
    {
        Vector2 origin = transform.position;
        float distance = Vector2.Distance(origin, shootPoint.position);
        if (distance <= 0.01f) return false;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, pointBlankRadius, dir, distance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            Collider2D col = hit.collider;
            if (col == null || col.transform.IsChildOf(transform)) continue;   // eigener Spieler

            EnemyLife enemy = col.GetComponent<EnemyLife>();
            if (enemy != null) { enemy.TakeDamage(damage); return true; }

            BossLife boss = col.GetComponent<BossLife>();
            if (boss != null) { boss.TakeDamage(damage); return true; }

            // Feste Wand direkt vor der Muendung (Trigger-Zonen und Kugeln werden ignoriert)
            if (!col.isTrigger && col.GetComponent<GunshotEffect>() == null) return true;
        }
        return false;
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
            if (tilemap == null) continue;

            Vector3Int playerCell = tilemap.WorldToCell(playerPos);

            // NEU: Zellgröße berücksichtigen, damit interactDistance wirklich Weltraum-Einheiten entspricht
            Vector3 cellSize = tilemap.cellSize;
            int radiusX = Mathf.CeilToInt(interactDistance / Mathf.Max(cellSize.x, 0.0001f));
            int radiusY = Mathf.CeilToInt(interactDistance / Mathf.Max(cellSize.y, 0.0001f));

            for (int x = -radiusX; x <= radiusX; x++)
            {
                for (int y = -radiusY; y <= radiusY; y++)
                {
                    Vector3Int checkCell = new Vector3Int(playerCell.x + x, playerCell.y + y, playerCell.z);

                    // NEU: tatsächliche Distanz prüfen statt nur Quadrat
                    Vector3 cellWorldPos = tilemap.GetCellCenterWorld(checkCell);
                    if (Vector3.Distance(playerPos, cellWorldPos) > interactDistance)
                        continue;

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
