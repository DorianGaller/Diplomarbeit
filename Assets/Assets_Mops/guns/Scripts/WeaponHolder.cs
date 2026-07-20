using UnityEngine;
using System.Reflection;

/// <summary>
/// Hängt am Player (oder GameManager).
/// Beobachtet den MainHand-Slot und gibt InteractKeys immer die aktuelle Waffe.
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    [Header("Slot Reference")]
    [Tooltip("MainHand EquippedSlot aus dem InventoryCanvas")]
    [SerializeField] private EquippedSlot mainHandSlot;

    [Header("Weapon Library")]
    [Tooltip("Alle verfügbaren WeaponSOs — Name muss mit itemName im Slot übereinstimmen")]
    [SerializeField] private WeaponSO[] weapons;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    // Aktuell ausgerüstete Waffe — von InteractKeys gelesen
    public WeaponSO CurrentWeapon { get; private set; }

    private InteractKeys interactKeys;
    private FieldInfo itemNameField;
    private string lastEquippedName = null;

    private void Start()
    {
        interactKeys = GetComponent<InteractKeys>();
        if (interactKeys == null)
            interactKeys = GetComponentInChildren<InteractKeys>();

        if (interactKeys == null)
            Debug.LogError("WeaponHolder: InteractKeys nicht gefunden!");

        if (mainHandSlot == null)
            Debug.LogError("WeaponHolder: Kein mainHandSlot zugewiesen!");

        // Reflection einmalig cachen
        itemNameField = typeof(EquippedSlot).GetField(
            "itemName",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        UpdateWeapon();
    }

    private void Update()
    {
        if (mainHandSlot == null || itemNameField == null) return;

        string currentName = itemNameField.GetValue(mainHandSlot) as string;
        if (currentName == lastEquippedName) return;

        lastEquippedName = currentName;
        UpdateWeapon();
    }

    private void UpdateWeapon()
    {
        CurrentWeapon = null;

        if (!string.IsNullOrEmpty(lastEquippedName))
        {
            foreach (var weapon in weapons)
            {
                if (weapon.weaponName == lastEquippedName)
                {
                    CurrentWeapon = weapon;
                    break;
                }
            }
        }

        // canShoot nur erlauben wenn eine Waffe ausgerüstet ist
        if (interactKeys != null)
            interactKeys.canShoot = CurrentWeapon != null;

        if (showDebugLog)
            Debug.Log($"WeaponHolder: Waffe gewechselt → '{CurrentWeapon?.weaponName ?? "keine"}'");
    }
}