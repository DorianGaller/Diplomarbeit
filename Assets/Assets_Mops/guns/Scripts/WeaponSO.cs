using UnityEngine;

public enum FiringMode
{
    SemiAuto,
    FullAuto
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponSprite;

    [Header("Combat")]
    [Tooltip("Schaden auf Level 1.")]
    public int baseDamage = 25;

    [Tooltip("Schadenszuwachs pro Level, relativ zum baseDamage. " +
            "0.2 = jedes Level gibt 20% des Grundschadens dazu.")]
    [Range(0f, 1f)]
    public float growthPerLevel = 0.2f;

    [Header("Firing Mode")]
    public FiringMode firingMode = FiringMode.SemiAuto;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Visual")]   // NEU
    [Tooltip("Abstand von der Waffen-Icon-Mitte bis zur Laufspitze — kurze Waffen kleiner, lange Waffen größer")]
    public float muzzleOffset = 0.3f;   // NEU
}