using UnityEngine;

public enum FiringMode
{
    SemiAuto,   // Ein Schuss pro Klick
    FullAuto    // Schießt solange Maustaste gehalten wird
}

/// <summary>
/// ScriptableObject für eine Waffe.
/// Erstellen: Rechtsklick → Create → Weapon
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public Sprite weaponSprite;

    [Header("Firing Mode")]
    [Tooltip("SemiAuto = ein Schuss pro Klick, FullAuto = Dauerfeuer beim Halten")]
    public FiringMode firingMode = FiringMode.SemiAuto;

    [Header("Bullet")]
    [Tooltip("Das Bullet-Prefab das diese Waffe verschießt")]
    public GameObject bulletPrefab;

    [Tooltip("Schussrate in Sekunden (0 = so schnell wie man klickt)")]
    public float fireRate = 0.2f;

    [Tooltip("Anzahl Kugeln pro Schuss (z.B. 3 für Shotgun)")]
    public int bulletsPerShot = 1;

    [Tooltip("Streuungswinkel in Grad (0 = kein Spread)")]
    public float spreadAngle = 0f;
}