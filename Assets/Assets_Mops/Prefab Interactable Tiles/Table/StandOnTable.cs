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

    public MonoBehaviour movementScript;

    private bool isOnTable = false;

    public void MovePlayerToTable()
    {
        StartCoroutine(MovePlayer(tablePosition.position, true));
    }

    public void MovePlayerToGround()
    {
        StartCoroutine(MovePlayer(groundPosition.position, false));
    }

    private IEnumerator MovePlayer(Vector2 targetPos, bool onTable)
    {
        // 🔹 Bewegung stoppen
        playerRb.linearVelocity = Vector2.zero;

        // 🔹 Tisch-Kollision VORHER deaktivieren
        if (tableCollider != null && onTable)
            tableCollider.enabled = false;

        // 🔹 Spieler Collider kurz aus
        playerCollider.enabled = false;

        // 🔹 Kleine Offset gegen Überschneidung
        Vector2 offset = new Vector2(0, 0.05f);
        playerRb.position = targetPos + offset;

        yield return null;

        // 🔹 Collider wieder aktivieren
        playerCollider.enabled = true;

        isOnTable = onTable;

        // 🔹 Beim Runtergehen Tisch wieder aktivieren
        if (tableCollider != null && !onTable)
            tableCollider.enabled = true;

        // 🔹 Movement deaktivieren auf Tisch
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