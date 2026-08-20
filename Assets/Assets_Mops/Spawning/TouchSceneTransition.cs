using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Wie DoorInteraction, aber ohne F-Taste: sobald der Spieler dieses
/// (meist unsichtbare) GameObject berührt, wird direkt die Zielszene geladen.
/// Braucht einen Collider2D mit "Is Trigger" aktiviert.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TouchSceneTransition : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name der Scene die geladen werden soll")]
    [SerializeField] private string targetSceneName = "EscapeScene";
    [Tooltip("ID des SpawnPoints in der Zielszene, an dem der Spieler ankommen soll")]
    [SerializeField] private string targetSpawnID = "";

    [Header("Optional: Required Item")]
    [Tooltip("Leer lassen wenn kein Item benötigt wird")]
    [SerializeField] private string requiredItemName = "";

    [Tooltip("Der MainHand EquippedSlot – nur nötig wenn requiredItemName gesetzt ist")]
    [SerializeField] private EquippedSlot mainHandSlot;

    [Header("Elevator (Stockwerk-Eingang)")]
    [Tooltip("Aktivieren, wenn dieser Übergang ein neues Stockwerk betritt")]
    [SerializeField] private bool isFloorEntrance = false;
    [Tooltip("Muss mit der Floor ID der RoomModifierManager auf diesem Stockwerk übereinstimmen")]
    [SerializeField] private string floorID = "";

    private bool triggered = false;

    private void Start()
    {
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
                Debug.LogError("TouchSceneTransition: MainHand EquippedSlot nicht gefunden! " +
                               "Bitte im Inspector manuell zuweisen oder requiredItemName leer lassen.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (requiredItemName != "" && !HasRequiredItem())
            return;

        triggered = true;
        Transition();
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
            Debug.LogError("TouchSceneTransition: Feld 'itemName' in EquippedSlot nicht gefunden!");
            return false;
        }

        string equippedName = field.GetValue(mainHandSlot) as string;
        return equippedName == requiredItemName;
    }

    private void Transition()
    {
        if (requiredItemName != "" && mainHandSlot != null)
            mainHandSlot.ClearEquippedItem();

        if (isFloorEntrance && !string.IsNullOrEmpty(floorID))
        {
            int newVisitCount = FightRoomProgress.RegisterElevatorEntry(floorID);
            Debug.Log($"Touch-Transition: Stockwerk {floorID} wird betreten (Besuch #{newVisitCount})");
        }

        DoorTransition.nextSpawnID = targetSpawnID;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
}