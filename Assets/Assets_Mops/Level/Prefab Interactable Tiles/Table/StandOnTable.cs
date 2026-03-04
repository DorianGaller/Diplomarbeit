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

    public GameObject extraObject;   // Empty GameObject

    private bool isOnTable = false;
    private bool lastTableState = false;   // Wichtig für B-Lösung

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

        yield return null;

        playerCollider.enabled = true;

        isOnTable = onTable;

        if (tableCollider != null)
            tableCollider.enabled = !isOnTable;

        // ✅ Nur beim Zustandswechsel reagieren
        if (extraObject != null && isOnTable != lastTableState)
        {
            if (isOnTable)
                extraObject.SetActive(true);
            // Beim Runtergehen NICHT automatisch deaktivieren

            lastTableState = isOnTable;
        }

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