using UnityEngine;
using System.Collections;

/// <summary>
/// Haengt am Root vom Tisch.
/// G = Tisch aufheben / abstellen (wird beim Tragen kleiner und kreist Richtung Maus um den Spieler)
/// T = auf den Tisch steigen / runterklettern
/// </summary>
public class TableCarry : MonoBehaviour
{
    [Header("Tasten")]
    [SerializeField] private KeyCode carryKey = KeyCode.G;
    [SerializeField] private KeyCode climbKey = KeyCode.T;

    [Header("Tragen")]
    [SerializeField] private float interactRange = 2f;
    [Tooltip("Abstand zum Spieler beim Tragen")]
    [SerializeField] private float carryDistance = 1.2f;
    [Tooltip("Abstand zum Spieler beim Abstellen (gross genug, damit der Tisch nicht im Spieler landet)")]
    [SerializeField] private float dropDistance = 1.8f;
    [Range(0.1f, 1f)]
    [SerializeField] private float carryScale = 0.5f;
    [Tooltip("Abstand nach aussen zum Equipment-Orbit, damit der Tisch nicht ueber der Waffe haengt")]
    [SerializeField] private float gapToOrbit = 0.8f;

    [Header("Klettern (gleiche Werte wie bei StandOnTable)")]
    [SerializeField] private Transform tablePosition;
    [SerializeField] private Transform groundPosition;
    [SerializeField] private Collider2D tableCollider;

    [Header("Layout umschalten beim Klettern")]
    [Tooltip("Wird AN geschaltet, wenn man auf dem Tisch steht (z.B. 'on Table')")]
    [SerializeField] private GameObject[] activeOnTable;
    [Tooltip("Wird AN geschaltet, wenn man am Boden steht (z.B. 'Layout Escape Room')")]
    [SerializeField] private GameObject[] activeOnGround;

    // Nur ein Tisch gleichzeitig tragen / betreten
    private static TableCarry carriedTable;
    private static TableCarry tableStoodOn;
    private static int lastActionFrame = -1;

    private Transform player;
    private Rigidbody2D playerRb;
    private BoxCollider2D playerCollider;
    private PlayerMovement playerMovement;
    private PlayerEquipmentOrbit orbit;
    private Camera cam;

    private Collider2D[] colliders;
    private bool[] colliderStates;
    private Vector3 normalScale;
    private Vector2 lastDir = Vector2.right;
    private bool isCarried;
    private bool isClimbing;

    private void Start()
    {
        normalScale = transform.localScale;
        colliders = GetComponentsInChildren<Collider2D>(true);
        colliderStates = new bool[colliders.Length];

        StartCoroutine(ResolvePlayerNextFrame());
    }

    // Wartet einen Frame, damit DontDestroy-Duplikate bereinigt sind
    private IEnumerator ResolvePlayerNextFrame()
    {
        yield return null;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("TableCarry: Kein GameObject mit Tag 'Player' gefunden!");
            yield break;
        }

        player = playerObj.transform;
        playerRb = playerObj.GetComponent<Rigidbody2D>();
        playerCollider = playerObj.GetComponent<BoxCollider2D>();
        playerMovement = playerObj.GetComponent<PlayerMovement>();
        orbit = playerObj.GetComponentInChildren<PlayerEquipmentOrbit>();
    }

    private void Update()
    {
        if (player == null || TutorialManager.IsRunning) return;

        if (isCarried)
        {
            FollowPlayer();

            if (Input.GetKeyDown(carryKey) && CanAct())
                Drop();
            return;
        }

        if (isClimbing) return;

        if (tableStoodOn == this && tablePosition != null
            && Vector2.Distance(tablePosition.position, player.position) > interactRange)
            tableStoodOn = null;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > interactRange) return;

        bool playerOnThisTable = tableStoodOn == this;

        // G: aufheben (nicht wenn man drauf steht oder schon was traegt)
        if (Input.GetKeyDown(carryKey) && carriedTable == null
            && tableStoodOn == null && CanAct())
        {
            PickUp();
            return;
        }

        // T: rauf / runter (nicht beim Tragen, nicht auf einen anderen Tisch)
        if (Input.GetKeyDown(climbKey) && carriedTable == null
            && (tableStoodOn == null || playerOnThisTable) && CanAct())
        {
            if (playerOnThisTable)
                StartCoroutine(MovePlayer(groundPosition, false));
            else
                StartCoroutine(MovePlayer(tablePosition, true));
        }
    }

    // Verhindert, dass zwei Tische im selben Frame auf denselben Tastendruck reagieren
    private bool CanAct()
    {
        if (lastActionFrame == Time.frameCount) return false;
        lastActionFrame = Time.frameCount;
        return true;
    }

    private float CurrentCarryDistance()
    {
        return orbit != null ? Mathf.Max(carryDistance, orbit.orbitRadius + gapToOrbit) : carryDistance;
    }

    // ── Tragen ────────────────────────────────────────────

    private void PickUp()
    {
        isCarried = true;
        carriedTable = this;

        transform.localScale = normalScale * carryScale;

        // Collider aus, damit der Tisch beim Tragen nirgends haengen bleibt
        for (int i = 0; i < colliders.Length; i++)
        {
            colliderStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }

        FollowPlayer();
    }

    private void Drop()
    {
        Vector3 dropPos = player.position + (Vector3)(lastDir * Mathf.Max(dropDistance, CurrentCarryDistance()));
        dropPos.z = transform.position.z;
        transform.position = dropPos;

        transform.localScale = normalScale;

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = colliderStates[i];

        isCarried = false;
        carriedTable = null;
    }

    private void FollowPlayer()
    {
        if (cam == null)
            cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rawOffset = mouseWorldPos - player.position;

        Vector2 dir = rawOffset.sqrMagnitude > 0.0001f
            ? rawOffset.normalized
            : lastDir;

        lastDir = dir;

        Vector3 pos = player.position + (Vector3)(dir * CurrentCarryDistance());
        pos.z = transform.position.z;
        transform.position = pos;
    }

    // ── Klettern (wie StandOnTable.MovePlayer) ────────────

    private IEnumerator MovePlayer(Transform target, bool onTable)
    {
        if (target == null || playerRb == null || playerCollider == null)
        {
            Debug.LogWarning("TableCarry: Table/Ground Position oder Player-Komponenten fehlen!");
            yield break;
        }

        isClimbing = true;
        playerRb.linearVelocity = Vector2.zero;

        if (tableCollider != null && onTable)
            tableCollider.enabled = false;

        playerCollider.enabled = false;
        playerRb.position = (Vector2)target.position + new Vector2(0, 0.05f);

        yield return null;

        playerCollider.enabled = true;

        if (tableCollider != null && !onTable)
            tableCollider.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = !onTable;

        tableStoodOn = onTable ? this : null;
        SetLayout(onTable);
        isClimbing = false;
    }

    private void SetLayout(bool onTable)
    {
        if (activeOnTable != null)
            foreach (GameObject obj in activeOnTable)
                if (obj != null) obj.SetActive(onTable);

        if (activeOnGround != null)
            foreach (GameObject obj in activeOnGround)
                if (obj != null) obj.SetActive(!onTable);
    }

    private void OnDestroy()
    {
        if (carriedTable == this) carriedTable = null;
        if (tableStoodOn == this) tableStoodOn = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}