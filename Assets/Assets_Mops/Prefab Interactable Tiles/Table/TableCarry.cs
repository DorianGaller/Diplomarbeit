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

    [Header("Abstellen (nicht in Waende / ausserhalb der Map)")]
    [Tooltip("An = nur Tilemap-Collider blockieren (Waende, Deko-Hitbox). Aus = jeder feste Collider")]
    [SerializeField] private bool onlyTilemapsBlock = true;
    [Tooltip("So weit (Grad) darf die Richtung abweichen, wenn direkt vor dir eine Wand ist")]
    [SerializeField] private float dropAngleTolerance = 45f;

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

    private Vector2 footprintSize = Vector2.one;
    private Vector2 footprintOffset;

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

        CacheFootprint();
        transform.localScale = normalScale * carryScale;

        CacheFootprint();

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
        if (!FindDropPosition(Mathf.Max(dropDistance, CurrentCarryDistance()), out Vector3 dropPos))
        {
            StartCoroutine(FlashBlocked());   // kein Platz -> Tisch bleibt in der Hand
            return;
        }
        dropPos.z = transform.position.z;
        transform.position = dropPos;

        transform.localScale = normalScale;

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = colliderStates[i];

        isCarried = false;
        carriedTable = null;
    }

    // Groesse des Tisches (feste Collider, volle Groesse) merken
    private void CacheFootprint()
    {
        bool has = false;
        Bounds b = new Bounds(transform.position, Vector3.zero);
        foreach (Collider2D c in colliders)
        {
            if (c == null || !c.enabled || c.isTrigger) continue;
            if (!has) { b = c.bounds; has = true; }
            else b.Encapsulate(c.bounds);
        }
        footprintSize = has ? (Vector2)b.size : Vector2.one;
        footprintOffset = has ? (Vector2)(b.center - transform.position) : Vector2.zero;
    }

    // Sucht einen freien Platz: erst geradeaus, dann abwechselnd links/rechts in 15-Grad-Schritten
    private bool FindDropPosition(float dist, out Vector3 pos)
    {
        for (float a = 0f; a <= dropAngleTolerance + 0.01f; a += 15f)
        {
            for (int s = a == 0f ? 1 : -1; s <= 1; s += 2)
            {
                Vector2 dir = Rotate(lastDir, a * s);
                Vector3 p = player.position + (Vector3)(dir * dist);
                if (IsFree(p)) { pos = p; return true; }
            }
        }
        pos = Vector3.zero;
        return false;
    }

    private bool IsFree(Vector3 tablePos)
    {
        Vector2 center = (Vector2)tablePos + footprintOffset;

        // Platz fuer den Tisch frei?
        foreach (Collider2D c in Physics2D.OverlapBoxAll(center, footprintSize * 0.95f, 0f))
            if (Blocks(c)) return false;

        // Keine Wand zwischen Spieler und Tisch (sonst landet er auf der anderen Seite / ausserhalb)
        foreach (RaycastHit2D hit in Physics2D.LinecastAll(player.position, center))
            if (Blocks(hit.collider)) return false;

        return true;
    }

    private bool Blocks(Collider2D c)
    {
        if (c == null || c.isTrigger) return false;
        if (c.transform.IsChildOf(transform) || c.transform.IsChildOf(player)) return false;
        if (!onlyTilemapsBlock) return true;

        // TilemapCollider2D oder z.B. ein CompositeCollider2D auf einer Tilemap
        return c is UnityEngine.Tilemaps.TilemapCollider2D
            || c.GetComponent<UnityEngine.Tilemaps.Tilemap>() != null;
    }

    private static Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad, cs = Mathf.Cos(r), sn = Mathf.Sin(r);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
    }

    // Kurz rot aufblitzen, wenn der Tisch hier nicht hin kann
    private IEnumerator FlashBlocked()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        Color[] old = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
        {
            old[i] = srs[i].color;
            srs[i].color = new Color(1f, 0.35f, 0.35f, old[i].a);
        }
        yield return new WaitForSeconds(0.15f);
        for (int i = 0; i < srs.Length; i++)
            if (srs[i] != null) srs[i].color = old[i];
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