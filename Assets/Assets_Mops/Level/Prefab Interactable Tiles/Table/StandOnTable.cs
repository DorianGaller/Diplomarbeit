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

    //public GameObject extraObject;

    // 🔹 Movement Script hier reinziehen (z.B. PlayerMove)
    public MonoBehaviour movementScript;

    private bool isOnTable = false;
    private bool lastTableState = false;

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
        // Bewegung stoppen
        playerRb.linearVelocity = Vector2.zero;
        playerCollider.enabled = false;

        // Position setzen
        playerRb.position = targetPos;

        yield return null;

        playerCollider.enabled = true;

        isOnTable = onTable;

        // 🔹 Movement deaktivieren/aktivieren
        if (movementScript != null)
            movementScript.enabled = !isOnTable;

        // 🔹 Tisch-Kollision deaktivieren wenn drauf
        if (tableCollider != null)
            tableCollider.enabled = !isOnTable;

        // 🔹 Extra-Objekt nur beim Zustandswechsel aktivieren
        //if (extraObject != null && isOnTable != lastTableState)
        //{
            //if (isOnTable)
                //extraObject.SetActive(true);

            //lastTableState = isOnTable;
        //}

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