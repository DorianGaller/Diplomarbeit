using UnityEngine;
using TMPro;

public class PCInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("References")]
    public PCTerminalUI terminalUI;
    public GameObject interactPrompt; // Ein UI-Element "E drücken"

    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        // Spieler automatisch finden (Tag "Player" muss gesetzt sein)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        // Prompt anzeigen/verstecken
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange && !terminalUI.IsOpen);

        // Interaktion starten
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            terminalUI.OpenTerminal();
        }
    }

    // Gizmo zum Visualisieren der Range im Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}