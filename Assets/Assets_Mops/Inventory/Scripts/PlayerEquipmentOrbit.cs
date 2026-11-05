using UnityEngine;

public class PlayerEquipmentOrbit : MonoBehaviour
{
    [Header("References")]
    public EquippedSlot[] equippedSlots;
    public Transform shootPoint;
    public WeaponHolder weaponHolder;

    [Header("Orbit Settings")]
    public float orbitRadius = 1.5f;
    public GameObject iconPrefab;

    [Header("Waffen-Icon")]
    public bool rotateWeaponIcon = true;
    public float defaultMuzzleOffset = 0.3f;

    private SpriteRenderer[] icons;
    private Camera cam;
    private Vector2 lastAimDir = Vector2.right;
    public Vector2 AimDirection => lastAimDir;

    private Vector3 iconBaseScale = Vector3.one;
    private EquipmentSOLibary equipmentSOLibary;

    void Start()
    {
        cam = Camera.main;

        if (weaponHolder == null)
            weaponHolder = GetComponent<WeaponHolder>();

        if (iconPrefab != null)
            iconBaseScale = iconPrefab.transform.localScale;


        icons = new SpriteRenderer[equippedSlots.Length];

        for (int i = 0; i < equippedSlots.Length; i++)
            icons[i] = CreateIcon();
    }

    void Update()
    {
        if (equippedSlots == null || icons == null) return;

        int staticIndex = 0;
        int staticCount = CountNonWeaponSlots();

        for (int i = 0; i < equippedSlots.Length; i++)
        {
            EquippedSlot slot = equippedSlots[i];
             if (slot == null) continue;

            // Sicherheitsnetz: falls ein Icon doch zerstört wurde, neu erzeugen
            if (icons[i] == null)
                icons[i] = CreateIcon();

            bool hasItem = slot.HasItem();
            icons[i].gameObject.SetActive(hasItem);

            if (!hasItem) continue;

            icons[i].sprite = slot.GetItemSprite();

            if (slot.GetItemType() == ItemType.mainHand)
            {
                icons[i].transform.localScale = iconBaseScale * GetHeldScale(slot.GetItemName());
                PositionWeaponIcon(icons[i].transform, icons[i]);
            }
            else
            {
                icons[i].transform.localScale = iconBaseScale;

                float angle = staticIndex * (360f / Mathf.Max(1, staticCount)) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
                icons[i].transform.position = transform.position + offset;
                staticIndex++;
            }
        }
    }

    SpriteRenderer CreateIcon()
    {
        GameObject icon = Instantiate(iconPrefab);
        DontDestroyOnLoad(icon);
        icon.SetActive(false);
        return icon.GetComponent<SpriteRenderer>();
    }

    // Wenn dieser Player zerstört wird (z.B. das Duplikat aus der neuen Szene),
    // seine Icons mit aufräumen, damit keine verwaisten Icons übrig bleiben
    void OnDestroy()
    {
        if (icons == null) return;

        foreach (SpriteRenderer icon in icons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
    }

    // Holt heldScale aus dem EquipmentSO mit gleichem itemName (1 wenn keins gefunden).
    // Absichtlich jedes Frame, damit man den Wert im Play-Mode live anpassen kann.
    float GetHeldScale(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 1f;

        if (equipmentSOLibary == null)
        {
            GameObject canvas = GameObject.Find("InventoryCanvas");
            if (canvas != null)
                equipmentSOLibary = canvas.GetComponent<EquipmentSOLibary>();
        }

        if (equipmentSOLibary == null || equipmentSOLibary.equipmentSO == null) return 1f;

        foreach (EquipmentSO so in equipmentSOLibary.equipmentSO)
        {
            if (so != null && so.itemName == itemName)
                return so.heldScale;
        }

        return 1f;
    }

    int CountNonWeaponSlots()
    {
        int count = 0;
        foreach (var slot in equippedSlots)
        {
            if (slot != null && slot.HasItem() && slot.GetItemType() != ItemType.mainHand)
                count++;
        }
        return count;
    }

    void PositionWeaponIcon(Transform iconTransform, SpriteRenderer iconRenderer)
    {
        if (cam == null) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;

        Vector2 rawOffset = mouseWorldPos - transform.position;

        // NEU: nur bei EXAKT Null (extrem seltener Randfall) die letzte Richtung nehmen,
        // ansonsten IMMER live der Maus folgen — auch bei sehr kleinem Abstand
        Vector2 dir = rawOffset.sqrMagnitude > 0.0001f
            ? rawOffset.normalized
            : lastAimDir;

        lastAimDir = dir;

        float muzzleOffset = defaultMuzzleOffset;
        if (weaponHolder != null && weaponHolder.CurrentWeapon != null)
            muzzleOffset = weaponHolder.CurrentWeapon.muzzleOffset;

        // Maus zwischen Muendung und Spieler -> Gun dreht sich zum Spieler
        bool pointAtPlayer = rawOffset.magnitude < orbitRadius + muzzleOffset;
        Vector2 facing = pointAtPlayer ? -dir : dir;

        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

        iconTransform.position = transform.position + (Vector3)(dir * orbitRadius);

        if (rotateWeaponIcon)
            iconTransform.rotation = Quaternion.Euler(0, 0, angle);

        if (iconRenderer != null)
            iconRenderer.flipY = angle > 90f || angle < -90f;

        if (shootPoint != null)
        {
            Vector3 muzzlePos = iconTransform.position + (Vector3)(facing * muzzleOffset);
            shootPoint.position = muzzlePos;
            shootPoint.rotation = iconTransform.rotation;
        }
    }
}