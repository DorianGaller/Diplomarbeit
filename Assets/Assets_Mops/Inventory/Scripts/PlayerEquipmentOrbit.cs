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

    void Start()
    {
        cam = Camera.main;

        if (weaponHolder == null)
            weaponHolder = GetComponent<WeaponHolder>();

        icons = new SpriteRenderer[equippedSlots.Length];

        for (int i = 0; i < equippedSlots.Length; i++)
        {
            GameObject icon = Instantiate(iconPrefab);
            icons[i] = icon.GetComponent<SpriteRenderer>();
            icon.SetActive(false);
        }
    }

    void Update()
    {
        if (equippedSlots == null || icons == null) return;

        int staticIndex = 0;
        int staticCount = CountNonWeaponSlots();

        for (int i = 0; i < equippedSlots.Length; i++)
        {
            EquippedSlot slot = equippedSlots[i];
            if (slot == null || icons[i] == null) continue;

            bool hasItem = slot.HasItem();
            icons[i].gameObject.SetActive(hasItem);

            if (!hasItem) continue;

            icons[i].sprite = slot.GetItemSprite();

            if (slot.GetItemType() == ItemType.mainHand)
            {
                PositionWeaponIcon(icons[i].transform, icons[i]);
            }
            else
            {
                float angle = staticIndex * (360f / Mathf.Max(1, staticCount)) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
                icons[i].transform.position = transform.position + offset;
                staticIndex++;
            }
        }
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

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        iconTransform.position = transform.position + (Vector3)(dir * orbitRadius);

        if (rotateWeaponIcon)
            iconTransform.rotation = Quaternion.Euler(0, 0, angle);

        if (iconRenderer != null)
            iconRenderer.flipY = angle > 90f || angle < -90f;

        if (shootPoint != null)
        {
            float muzzleOffset = defaultMuzzleOffset;
            if (weaponHolder != null && weaponHolder.CurrentWeapon != null)
                muzzleOffset = weaponHolder.CurrentWeapon.muzzleOffset;

            Vector3 muzzlePos = iconTransform.position + (Vector3)(dir * muzzleOffset);
            shootPoint.position = muzzlePos;
            shootPoint.rotation = iconTransform.rotation;
        }
    }
}