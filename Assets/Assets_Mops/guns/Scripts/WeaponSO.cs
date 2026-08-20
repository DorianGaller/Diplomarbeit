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