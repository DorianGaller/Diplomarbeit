using UnityEngine;
using System.Collections.Generic;

public class EnemyChase : MonoBehaviour
{
    [Header("Grid / Obstacles")]
    [Tooltip("Größe einer Gitterzelle in Weltraum-Einheiten")]
    public Vector2 cellSize = new Vector2(2.24f, 1.74f);
    [Tooltip("Layer eurer Wand-/Tilemap-Collider, die umgangen werden sollen")]
    public LayerMask obstacleLayer;
    public float repathInterval = 0.4f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float waypointTolerance = 0.15f;
    public float stopDistanceFromPlayer = 1.5f;

    private Transform player;
    private List<Vector2> currentPath = new List<Vector2>();
    private int pathIndex = 0;
    private float repathTimer = 999f;

    void OnEnable()
    {
        repathTimer = 999f;   // beim Aktivieren sofort neu pathfinden, nicht erst nach repathInterval warten
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathInterval)
        {
            repathTimer = 0f;
            currentPath = GridAStar.FindPath(transform.position, player.position, cellSize, obstacleLayer);
            pathIndex = 0;
        }

        if (distanceToPlayer <= stopDistanceFromPlayer)
            return;   // nah genug dran, Nahkampf/Fernkampf-Skripte übernehmen die Attacke

        FollowPath();
    }

    void FollowPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        Vector2 waypoint = currentPath[pathIndex];
        Vector2 dir = waypoint - (Vector2)transform.position;

        if (dir.magnitude <= waypointTolerance)
        {
            pathIndex++;
            return;
        }

        Vector2 normalizedDir = dir.normalized;   // NEU
        transform.position += (Vector3)(normalizedDir * moveSpeed * Time.deltaTime);

        FaceDirection(normalizedDir);   // GEÄNDERT: normalisierte Richtung statt skalierter Bewegungsvektor
    }

    void FaceDirection(Vector3 dir)   // NEU
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
        if (currentPath == null) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < currentPath.Count - 1; i++)
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
    }
}