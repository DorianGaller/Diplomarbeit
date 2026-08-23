using UnityEngine;

/// <summary>
/// Basis fuer alle interaktiven Objekte: Radius-Check auf den Spieler,
/// Prompt-Text ueber PlayerInteractPrompt ein-/ausblenden, Taste abfragen.
/// Konkrete Objekte (Interactable, ElevatorInteractable, ...) erben davon
/// und ueberschreiben nur was sie wirklich unterscheidet.
/// </summary>
public abstract class InteractableBase : MonoBehaviour
{
    [Header("Interaktion")]
    [SerializeField] protected float interactionRadius = 2.5f;
    [SerializeField] protected KeyCode interactionKey = KeyCode.F;
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] protected bool canInteract = true;

    [Header("Prompt")]
    [SerializeField] protected string promptText = "Press F to Interact";

    [Header("Debug")]
    [SerializeField] protected bool showDebugInfo = false;

    protected Transform player;
    protected bool playerInRange;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[{GetType().Name}] Kein GameObject mit Tag '{playerTag}' gefunden!");
    }

    protected virtual void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(interactionKey))
            OnInteractPressed();
    }

    protected void CheckPlayerDistance()
    {
        bool wasInRange = playerInRange;
        playerInRange = canInteract &&
            Vector2.Distance(transform.position, player.position) <= interactionRadius;

        if (playerInRange == wasInRange) return;

        if (playerInRange && ShouldShowPrompt())
            DGInteractPrompt.Instance?.Show(promptText);
        else if (!playerInRange)
            DGInteractPrompt.Instance?.Hide();

        if (showDebugInfo)
            Debug.Log($"[{GetType().Name}] ({gameObject.name}) Player in range: {playerInRange}");
    }

    /// <summary>Erlaubt Subklassen, den Prompt zu unterdruecken (z.B. Elevator waehrend Panel offen).</summary>
    protected virtual bool ShouldShowPrompt() => true;

    /// <summary>Wird beim Tastendruck im Radius aufgerufen.</summary>
    protected abstract void OnInteractPressed();

    public void SetCanInteract(bool value)
    {
        canInteract = value;
        if (!canInteract && playerInRange)
        {
            playerInRange = false;
            PlayerInteractPrompt.Instance?.Hide();
        }
    }

    public void SetPromptText(string newText) => promptText = newText;

    protected void RefreshPrompt()
    {
        if (playerInRange && ShouldShowPrompt())
            PlayerInteractPrompt.Instance?.Show(promptText);
        else
            PlayerInteractPrompt.Instance?.Hide();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}