using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name der Scene die geladen werden soll")]
    [SerializeField] private string targetSceneName = "EscapeScene";
    [Tooltip("ID des SpawnPoints in der Zielszene, an dem der Spieler ankommen soll")]
    [SerializeField] private string targetSpawnID = "";

    [Tooltip("Maximale Distanz zur Tür damit F gedrückt werden kann")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Optional: Required Item")]
    [Tooltip("Leer lassen wenn kein Item benötigt wird")]
    [SerializeField] private string requiredItemName = "";

    [Tooltip("Der MainHand EquippedSlot – nur nötig wenn requiredItemName gesetzt ist")]
    [SerializeField] private EquippedSlot mainHandSlot;

    [Header("References")]
    [Tooltip("Optionaler Hinweis (z.B. 'F - Tür öffnen')")]
    [SerializeField] private GameObject interactionHint;

    private Transform player;

    private void Start()
    {
        StartCoroutine(ResolveReferencesNextFrame());   // NEU
    }

    // NEU: wartet einen Frame, damit DontDestroy-Duplikate bereinigt sind,
    // löst dann Player und MainHand-Slot dynamisch neu auf
    private IEnumerator ResolveReferencesNextFrame()
    {
        yield return null;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("DoorInteraction: Kein GameObject mit Tag 'Player' gefunden!");

        if (requiredItemName != "" && mainHandSlot == null)
        {
            // NEU: FindObjectsInactive.Include, damit auch der (noch geschlossene) Equipment-Slot gefunden wird
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (EquippedSlot slot in allSlots)
            {
                if (slot.gameObject.name.ToLower().Contains("mainhand"))
                {
                    mainHandSlot = slot;
                    break;
                }
            }

            if (mainHandSlot == null)
                Debug.LogError("DoorInteraction: MainHand EquippedSlot nicht gefunden! " +
                               "Bitte im Inspector manuell zuweisen oder requiredItemName leer lassen.");
        }

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool playerInRange = distance <= interactionRange;

        bool hasRequiredItem = requiredItemName == "" || HasRequiredItem();

        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && hasRequiredItem);

        if (playerInRange && hasRequiredItem && Input.GetKeyDown(KeyCode.F))
        {
            OpenDoor();
        }
    }

    private bool HasRequiredItem()
    {
        if (mainHandSlot == null) return false;

        var field = typeof(EquippedSlot).GetField(
            "itemName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            Debug.LogError("DoorInteraction: Feld 'itemName' in EquippedSlot nicht gefunden!");
            return false;
        }

        string equippedName = field.GetValue(mainHandSlot) as string;
        return equippedName == requiredItemName;
    }

    private void OpenDoor()
    {
        if (requiredItemName != "" && mainHandSlot != null)
            mainHandSlot.ClearEquippedItem();

        DoorTransition.nextSpawnID = targetSpawnID;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}