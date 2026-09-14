using UnityEngine;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform[] waypoints;
    public float patrolSpeed = 1.5f;
    public float waypointTolerance = 0.15f;
    public float waitTimeAtWaypoint = 1f;

    [Header("Detection")]
    public float detectionRadius = 4f;
    [Range(0f, 360f)]
    [Tooltip("360 = Rundumsicht, kleinerer Wert = echter Sichtkegel nach vorne")]
    public float detectionAngle = 90f;
    [Tooltip("Layer für Wände/Server-Racks, die Sichtlinien blockieren")]
    public LayerMask obstacleLayer;
    [Tooltip("Wie lange der Spieler unentdeckt bleiben muss, bevor zur Patrouille zurückgekehrt wird")]
    public float loseSightTime = 3f;

    [Header("References")]
    [Tooltip("Wird deaktiviert, solange patrolliert wird, und aktiviert sobald der Spieler entdeckt ist")]
    public EnemyChase enemyChase;

    [Header("Patrol Pathfinding")]
    public Vector2 cellSize = new Vector2(2.24f, 1.74f);   // GEÄNDERT: Vector2 statt float
    public float patrolRepathInterval = 0.5f;

    private List<Vector2> patrolPath = new List<Vector2>();
    private int patrolPathIndex = 0;
    private float patrolRepathTimer = 999f;

    private Transform player;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    public bool isAlerted = false;
    private float loseSightTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (enemyChase == null)
            enemyChase = GetComponent<EnemyChase>();

        if (enemyChase != null)
            enemyChase.enabled = false;

        if (obstacleLayer.value == 0)   // NEU
            Debug.LogWarning("EnemyPatrol: Obstacle Layer ist nicht gesetzt (Nothing) — Sichtlinie wird nie blockiert, Gegner sieht durch Wände!", this);
    }

    void Update()
    {
        if (player == null) return;

        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            isAlerted = true;
            loseSightTimer = 0f;
        }
        else if (isAlerted)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime)
                isAlerted = false;
        }

        if (isAlerted)
        {
            if (enemyChase != null && !enemyChase.enabled)
                enemyChase.enabled = true;
        }
        else
        {
            if (enemyChase != null && enemyChase.enabled)
            {
                enemyChase.enabled = false;
                patrolRepathTimer = 999f;   // NEU: sofort neu pathfinden beim Zurückwechseln
            }

            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToWaypoint <= waypointTolerance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                patrolPath.Clear();
            }
            return;
        }

        patrolRepathTimer += Time.deltaTime;
        if (patrolRepathTimer >= patrolRepathInterval || patrolPath.Count == 0)
        {
            patrolRepathTimer = 0f;
            patrolPath = GridAStar.FindPath(transform.position, targetWaypoint.position, cellSize, obstacleLayer);
            patrolPathIndex = 0;

            // NEU: letzten Gitter-Punkt durch die exakte Wegpunkt-Position ersetzen,
            // damit der Gegner wirklich am echten Wegpunkt ankommt statt an der gerundeten Zelle
            if (patrolPath.Count > 0)
                patrolPath[patrolPath.Count - 1] = targetWaypoint.position;
        }

        FollowPatrolPath();


        if (patrolPath.Count == 0)
        {
            Vector2 dir = (targetWaypoint.position - transform.position).normalized;
            transform.position += (Vector3)(dir * patrolSpeed * Time.deltaTime);
            FaceDirection(dir);
        }
    }
    void FollowPatrolPath()
    {
        if (patrolPath == null || patrolPathIndex >= patrolPath.Count) return;

        Vector2 wp = patrolPath[patrolPathIndex];
        Vector2 dir = wp - (Vector2)transform.position;

        if (dir.magnitude <= waypointTolerance)
        {
            patrolPathIndex++;
            return;
        }

        Vector2 normalizedDir = dir.normalized;   // NEU
        transform.position += (Vector3)(normalizedDir * patrolSpeed * Time.deltaTime);

        FaceDirection(normalizedDir);   // GEÄNDERT: normalisierte Richtung statt skalierter Bewegungsvektor
    }

    bool CanSeePlayer()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > detectionRadius) return false;

        if (detectionAngle < 360f)
        {
            Vector2 facing = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
            float angleToPlayer = Vector2.Angle(facing, toPlayer);
            if (angleToPlayer > detectionAngle / 2f) return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, distance, obstacleLayer);
        if (hit.collider != null) return false;   // Sichtlinie durch Rack/Wand blockiert

        return true;
    }

    void FaceDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);
                Transform next = waypoints[(i + 1) % waypoints.Length];
                if (next != null)
                    Gizmos.DrawLine(waypoints[i].position, next.position);
            }
        }
    }
}