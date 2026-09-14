using UnityEngine;
 
/// <summary>
/// Haengt am Player. Beobachtet den MainHand-Slot und stellt die aktuell
/// ausgeruestete Waffe samt ihrem Fortschritt bereit.
///
/// Der Reflection-Zugriff auf EquippedSlot.itemName ist weggefallen —
/// der Slot liefert Name und instanceId jetzt oeffentlich.
/// </summary>
public class WeaponHolder : MonoBehaviour
{
    [Header("Slot Reference")]
    [Tooltip("MainHand EquippedSlot aus dem InventoryCanvas")]
    [SerializeField] private EquippedSlot mainHandSlot;
 
    [Header("Fallback")]
    [Tooltip("Wird nur benutzt, wenn die Waffe nicht in der ItemDatabase steht. " +
             "Auf Dauer sollte die Datenbank die einzige Quelle sein.")]
    [SerializeField] private WeaponSO[] weapons;
 
    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;
 
    // ── Oeffentlicher Zustand ─────────────────────────────
 
    /// <summary>Die Waffenklasse. Null wenn nichts ausgeruestet ist.</summary>
    public WeaponSO CurrentWeapon { get; private set; }
 
    /// <summary>Das konkrete Exemplar. Null bei Altbestand ohne Instanz.</summary>
    public WeaponInstance CurrentInstance { get; private set; }
 
    /// <summary>Level der ausgeruesteten Waffe. Ohne Instanz: 1.</summary>
    public int CurrentWeaponLevel =>
        CurrentInstance != null ? CurrentInstance.level : 1;
 
    /// <summary>Schaden inkl. Level. 0 wenn nichts ausgeruestet ist.</summary>
    public int CurrentDamage =>
        CurrentWeapon == null ? 0 : WeaponStats.GetDamage(CurrentWeapon, CurrentWeaponLevel);
 
    // ── Intern ────────────────────────────────────────────
 
    private InteractKeys interactKeys;
    private string lastName;
    private string lastInstanceId;
 
    private void Start()
    {
        interactKeys = GetComponent<InteractKeys>();
        if (interactKeys == null)
            interactKeys = GetComponentInChildren<InteractKeys>();
 
        if (interactKeys == null)
            Debug.LogError("WeaponHolder: InteractKeys nicht gefunden!");
 
        if (mainHandSlot == null)
            Debug.LogError("WeaponHolder: Kein mainHandSlot zugewiesen!");
 
        UpdateWeapon();
    }
 
    private void Update()
    {
        if (mainHandSlot == null) return;
 
        string currentName = mainHandSlot.GetItemName();
        string currentId = mainHandSlot.GetInstanceId();
 
        // Auch auf die ID reagieren: zwei gleichnamige Waffen zu tauschen
        // aendert den Namen nicht, aber sehr wohl das Level.
        if (currentName == lastName && currentId == lastInstanceId) return;
 
        lastName = currentName;
        lastInstanceId = currentId;
        UpdateWeapon();
    }
 
    private void UpdateWeapon()
    {
        CurrentWeapon = null;
        CurrentInstance = null;
 
        if (!string.IsNullOrEmpty(lastInstanceId))
            CurrentInstance = WeaponRegistry.Get(lastInstanceId);
 
        if (!string.IsNullOrEmpty(lastName))
        {
            CurrentWeapon = ItemDatabase.GetWeapon(lastName);
 
            if (CurrentWeapon == null)
                CurrentWeapon = FindInFallbackArray(lastName);
        }
 
        if (interactKeys != null)
            interactKeys.canShoot = CurrentWeapon != null;
 
        if (showDebugLog)
        {
            Debug.Log($"WeaponHolder: '{lastName ?? "keine"}' " +
                      $"id='{lastInstanceId ?? "-"}' " +
                      $"Lv{CurrentWeaponLevel} dmg={CurrentDamage}" +
                      (CurrentInstance == null && !string.IsNullOrEmpty(lastName)
                          ? "  (keine Instanz — Altbestand?)" : ""));
        }
    }
 
    private WeaponSO FindInFallbackArray(string weaponKey)
    {
        if (weapons == null) return null;
 
        foreach (var weapon in weapons)
        {
            if (weapon != null && weapon.weaponName == weaponKey)
            {
                if (showDebugLog)
                    Debug.LogWarning($"WeaponHolder: '{weaponKey}' steht nicht in der " +
                                     "ItemDatabase, Fallback-Array benutzt.");
                return weapon;
            }
        }
        return null;
    }
}