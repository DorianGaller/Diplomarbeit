using UnityEngine;
using System.Collections;

public class StandOnTable : MonoBehaviour
{
    public Rigidbody2D playerRb;
    public BoxCollider2D playerCollider;

    public Collider2D tableCollider;   // 👈 NEU

    public Transform tablePosition;
    public Transform groundPosition;

    public GameObject uiOnTable;
    public GameObject uiOnGround;

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
        playerRb.linearVelocity = Vector2.zero;
        playerCollider.enabled = false;

        playerRb.position = targetPos;

        yield return null; // 1 Frame warten

        playerCollider.enabled = true;

        isOnTable = onTable;

        // 👇 Tisch Collider steuern
        if (tableCollider != null)
            tableCollider.enabled = !isOnTable;

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