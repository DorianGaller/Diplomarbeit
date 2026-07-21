using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Distanz, ab der [E to interact] erscheint und Interaktion moeglich ist.")]
    public float interactionRadius = 2.5f;

    [Tooltip("Taste zum Interagieren.")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Text der im Prompt angezeigt wird.")]
    public string promptText = "[E to interact]";

    [Tooltip("Kann aktuell interagiert werden? Zum Sperren (z.B. Workbench noch nicht freigeschaltet).")]
    public bool canInteract = true;

    [Header("Prompt")]
    [Tooltip("Welt-Prompt der ueber dem Objekt schwebt. Optional.")]
    public GameObject promptWorldUI;

    [Tooltip("Offset des Prompts ueber dem Objekt.")]
    public Vector3 promptOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Events")]
    [Tooltip("Wird ausgeloest wenn der Spieler interagiert (z.B. UI oeffnen).")]
    public UnityEvent onInteract;

    private Transform player;
    private bool playerInRange;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (promptWorldUI != null)
        {
            promptWorldUI.transform.position = transform.position + promptOffset;
            promptWorldUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= interactionRadius && canInteract;

        if (promptWorldUI != null)
            promptWorldUI.SetActive(playerInRange);

        if (playerInRange && Input.GetKeyDown(interactKey))
            onInteract?.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}