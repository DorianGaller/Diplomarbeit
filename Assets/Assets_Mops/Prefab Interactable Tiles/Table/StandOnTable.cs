using UnityEngine;
using System.Collections;

public class StandOnTable : MonoBehaviour
{
    public Rigidbody2D playerRb;
    public BoxCollider2D playerCollider;

    public Collider2D tableCollider;

    public Transform tablePosition;
    public Transform groundPosition;

    public GameObject uiOnTable;
    public GameObject uiOnGround;

    [Header("Haupt-UI-Panel")]
    [Tooltip("Das Panel, das Table.cs öffnet — wird beim Exit mit geschlossen")]
    public GameObject tableUI;   // NEU

    public MonoBehaviour movementScript;

    private bool isOnTable = false;

    private void Start()
    {
        StartCoroutine(ResolvePlayerNextFrame());
    }

    private IEnumerator ResolvePlayerNextFrame()
    {
        yield return null;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            playerCollider = playerObj.GetComponent<BoxCollider2D>();

            PlayerMovement liveMovement = playerObj.GetComponent<PlayerMovement>();
            if (liveMovement != null)
                movementScript = liveMovement;
        }
        else
        {
            Debug.LogWarning("StandOnTable: Kein GameObject mit Tag 'Player' gefunden!");
        }

        if (playerRb == null)
            Debug.LogError("StandOnTable: Rigidbody2D nicht gefunden!");

        if (playerCollider == null)
            Debug.LogError("StandOnTable: BoxCollider2D nicht gefunden!");

        if (movementScript == null)
            Debug.LogError("StandOnTable: Movement Script (PlayerMovement) nicht gefunden!");
    }

    public void MovePlayerToTable()
    {
        StartCoroutine(MovePlayer(tablePosition.position, true));
    }

    public void MovePlayerToGround()
    {
        StartCoroutine(MovePlayer(groundPosition.position, false));
    }

    // NEU: Diese Methode hängt jetzt an den "Exit"-Button statt UIButtonDeactivate
    public void ExitTable()
    {
        if (isOnTable)
        {
            // Sicherheitsnetz: falls "Climb down" übersprungen wurde,
            // wird die Bewegung trotzdem zuverlässig wiederhergestellt
            StartCoroutine(MovePlayer(groundPosition.position, false));
        }

        if (tableUI != null)
            tableUI.SetActive(false);
    }

    private IEnumerator MovePlayer(Vector2 targetPos, bool onTable)
    {
        playerRb.linearVelocity = Vector2.zero;

        if (tableCollider != null && onTable)
            tableCollider.enabled = false;

        playerCollider.enabled = false;

        Vector2 offset = new Vector2(0, 0.05f);
        playerRb.position = targetPos + offset;

        yield return null;

        playerCollider.enabled = true;

        isOnTable = onTable;

        if (tableCollider != null && !onTable)
            tableCollider.enabled = true;

        if (movementScript != null)
            movementScript.enabled = !isOnTable;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uiOnTable != null)
            uiOnTable.SetActive(isOnTable);

        if (uiOnGround != null)
            uiOnGround.SetActive(!isOnTable);
    }
}