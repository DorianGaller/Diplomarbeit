using UnityEngine;

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
    public EnemyFollow enemyFollow;

    private Transform player;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isAlerted = false;
    private float loseSightTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (enemyFollow == null)
            enemyFollow = GetComponent<EnemyFollow>();

        if (enemyFollow != null)
            enemyFollow.enabled = false;   // Patrol übernimmt zunächst die Bewegung
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
            if (enemyFollow != null && !enemyFollow.enabled)
                enemyFollow.enabled = true;
        }
        else
        {
            if (enemyFollow != null && enemyFollow.enabled)
                enemyFollow.enabled = false;

            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= waypointTolerance)
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
            }
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * patrolSpeed * Time.deltaTime;

        FaceDirection(dir);
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