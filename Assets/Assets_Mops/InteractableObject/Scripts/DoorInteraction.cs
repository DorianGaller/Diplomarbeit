using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Füge diesen Script dem Door-GameObject hinzu.
/// Die Tür kann nur geöffnet werden, wenn der Spieler in der Nähe ist
/// und F drückt. Danach wird eine neue Scene geladen.
/// </summary>
public class DoorInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name der Scene die geladen werden soll")]
    [SerializeField] private string targetSceneName = "EscapeScene";

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
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("DoorInteraction: Kein GameObject mit Tag 'Player' gefunden!");

        // MainHand-Slot nur suchen wenn ein Item benötigt wird
        if (requiredItemName != "" && mainHandSlot == null)
        {
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(FindObjectsSortMode.None);
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

        // Item-Check: wenn kein Item benötigt wird, immer true
        bool hasRequiredItem = requiredItemName == "" || HasRequiredItem();

        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && hasRequiredItem);

        if (playerInRange && hasRequiredItem && Input.GetKeyDown(KeyCode.F))
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Prüft ob das benötigte Item im MainHand-Slot ausgerüstet ist.
    /// Wird nur aufgerufen wenn requiredItemName nicht leer ist.
    /// </summary>
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
        Debug.Log("Tür geöffnet – lade Scene: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}