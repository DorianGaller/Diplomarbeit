using UnityEngine;

/// <summary>
/// Füge diesen Script dem Wire-GameObject hinzu.
/// Der Wire kann nur entfernt werden, wenn die Stahlschere im MainHand-Slot
/// ausgerüstet ist und der Spieler in der Nähe ist und E drückt.
/// </summary>
public class WireInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Name des Items das benötigt wird (muss mit itemName im EquippedSlot übereinstimmen)")]
    [SerializeField] private string requiredItemName = "Stahlschere";

    [Tooltip("Maximale Distanz zum Wire damit E gedrückt werden kann")]
    [SerializeField] private float interactionRange = 2f;

    [Header("References")]
    [Tooltip("Der MainHand EquippedSlot aus dem InventoryCanvas")]
    [SerializeField] private EquippedSlot mainHandSlot;

    // Optional: Hinweis-UI (z.B. "E - Wire entfernen")
    [SerializeField] private GameObject interactionHint;

    private Transform player;
    private bool playerInRange = false;

    private void Start()
    {
        // Spieler per Tag finden
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("WireInteraction: Kein GameObject mit Tag 'Player' gefunden!");

        // MainHand-Slot automatisch suchen falls nicht im Inspector gesetzt
        if (mainHandSlot == null)
        {
            // Suche alle EquippedSlots und finde den MainHand-Slot
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(FindObjectsSortMode.None);
            foreach (EquippedSlot slot in allSlots)
            {
                // Slot-Name im GameObject-Namen suchen (z.B. "MainHandSlot")
                if (slot.gameObject.name.ToLower().Contains("mainhand"))
                {
                    mainHandSlot = slot;
                    break;
                }
            }

            if (mainHandSlot == null)
                Debug.LogError("WireInteraction: MainHand EquippedSlot nicht gefunden! " +
                               "Bitte im Inspector manuell zuweisen.");
        }

        // Hint zu Beginn ausblenden
        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null || mainHandSlot == null) return;

        // Distanz zum Spieler prüfen
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        // Stahlschere im MainHand ausgerüstet?
        bool hasSchere = HasRequiredItem();

        // Hint anzeigen wenn Spieler in Reichweite UND Schere ausgerüstet
        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && hasSchere);

        // E gedrückt + in Reichweite + Schere ausgerüstet → Wire entfernen
        if (playerInRange && hasSchere && Input.GetKeyDown(KeyCode.F))
        {
            RemoveWire();
        }
    }

    /// <summary>
    /// Prüft ob das benötigte Item im MainHand-Slot ausgerüstet ist.
    /// Greift auf das private itemName-Feld über Reflection zu,
    /// da EquippedSlot kein public Property dafür hat.
    /// </summary>
    private bool HasRequiredItem()
    {
        // Reflection um das private "itemName"-Feld aus EquippedSlot zu lesen
        var field = typeof(EquippedSlot).GetField(
            "itemName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            Debug.LogError("WireInteraction: Feld 'itemName' in EquippedSlot nicht gefunden!");
            return false;
        }

        string equippedName = field.GetValue(mainHandSlot) as string;
        return equippedName == requiredItemName;
    }

    private void RemoveWire()
    {
        Debug.Log("Wire entfernt mit: " + requiredItemName);
        Destroy(gameObject);
    }

    // Reichweite im Editor visualisieren
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}