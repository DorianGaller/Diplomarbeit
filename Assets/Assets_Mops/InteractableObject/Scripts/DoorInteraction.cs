using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string targetSceneName = "EscapeScene";
    [SerializeField] private string targetSpawnID = "";
    [SerializeField] private float interactionRange = 2f;

    [Header("Optional: Required Item")]
    [Tooltip("Leer lassen wenn kein Item benoetigt wird")]
    [SerializeField] private string requiredItemName = "";
    [SerializeField] private EquippedSlot mainHandSlot;

    [Header("Hinweise")]
    [Tooltip("Erscheint in der Naehe, wenn die Tuer offen ist (z.B. 'F - Durchgehen')")]
    [SerializeField] private GameObject interactionHint;
    [Tooltip("Erscheint in der Naehe, solange die Tuer gesperrt ist (z.B. 'Keycard ausruesten + Tuer anklicken')")]
    [SerializeField] private GameObject unlockHint;

    [Header("Optional: Tuer-Sprites (gesperrt / entsperrt)")]
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private Transform player;
    private Camera cam;
    private Collider2D[] clickColliders;
    private string saveId;

    private bool NeedsItem => requiredItemName != "";
    private bool IsUnlocked => !NeedsItem || SaveSystem.IsCleared(saveId);

    private void Start()
    {
        if (doorRenderer == null)
            doorRenderer = GetComponent<SpriteRenderer>();

        clickColliders = GetComponentsInChildren<Collider2D>(true);
        saveId = SaveSystem.GetObjectId(gameObject);

        StartCoroutine(ResolveReferencesNextFrame());
    }

    private IEnumerator ResolveReferencesNextFrame()
    {
        yield return null;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("DoorInteraction: Kein GameObject mit Tag 'Player' gefunden!");

        if (NeedsItem && mainHandSlot == null)
        {
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (EquippedSlot slot in allSlots)
            {
                if (slot.gameObject.name.ToLower().Contains("mainhand"))
                {
                    mainHandSlot = slot;
                    break;
                }
            }

            if (mainHandSlot == null)
                Debug.LogError("DoorInteraction: MainHand EquippedSlot nicht gefunden!");
        }

        if (interactionHint != null) interactionHint.SetActive(false);
        if (unlockHint != null) unlockHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        bool playerInRange = Vector2.Distance(transform.position, player.position) <= interactionRange;
        bool unlocked = IsUnlocked;

        UpdateDoorSprite(unlocked);

        if (interactionHint != null) interactionHint.SetActive(playerInRange && unlocked);
        if (unlockHint != null) unlockHint.SetActive(playerInRange && !unlocked);

        if (!playerInRange) return;

        // 1. Gesperrt: mit der Keycard in der Main Hand auf die Tuer klicken
        if (!unlocked)
        {
            if (Input.GetMouseButtonDown(0) && IsMouseOver() && HasRequiredItem())
                Unlock();
            return;
        }

        // 2. Offen: mit F durchgehen
        if (Input.GetKeyDown(KeyCode.F))
            GoThrough();
    }

    private void Unlock()
    {
        if (mainHandSlot != null)
            mainHandSlot.ClearEquippedItem();   // Keycard wird verbraucht

        SaveSystem.MarkCleared(saveId);          // Tuer bleibt dauerhaft offen
        UpdateDoorSprite(true);
    }

    private void GoThrough()
    {
        DoorTransition.nextSpawnID = targetSpawnID;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }

    private void UpdateDoorSprite(bool unlocked)
    {
        if (doorRenderer == null) return;

        Sprite target = unlocked ? unlockedSprite : lockedSprite;
        if (target != null && doorRenderer.sprite != target)
            doorRenderer.sprite = target;
    }

    private bool IsMouseOver()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        foreach (Collider2D col in clickColliders)
            if (col != null && col.enabled && col.OverlapPoint(mouse))
                return true;

        return doorRenderer != null &&
               doorRenderer.bounds.Contains(new Vector3(mouse.x, mouse.y, doorRenderer.bounds.center.z));
    }

    private bool HasRequiredItem()
    {
        if (mainHandSlot == null) return false;

        var field = typeof(EquippedSlot).GetField(
            "itemName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field != null && (field.GetValue(mainHandSlot) as string) == requiredItemName;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}