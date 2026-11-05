using UnityEngine;
using System.Collections.Generic;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    public float shootRange = 10f;
    public float stopOffset = 2f;

    [Header("Waende / Tilemaps")]
    [Tooltip("Layer der Wand-Collider (Tilemap) und fester Deko")]
    public LayerMask obstacleLayer;
    [Tooltip("Radius des Gegners fuer die Kollisionspruefung")]
    public float bodyRadius = 0.35f;
    [Tooltip("Raster fuer die Wegsuche um Hindernisse (halbe Tilemap-Zelle)")]
    public Vector2 pathCellSize = new Vector2(0.75f, 0.75f);
    public float repathInterval = 0.4f;

    private List<Vector2> path = new List<Vector2>();
    private int pathIndex;
    private float repathTimer = 999f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        float stopDistance = shootRange - stopOffset;
        if (distance <= stopDistance) return;

        Vector2 target = player.position;

        if (HasLineOfSight())
        {
            // Freie Sicht -> direkt auf den Spieler zu
            path.Clear();
            repathTimer = 999f;   // bei Sichtverlust sofort neu suchen
        }
        else
        {
            // Wand dazwischen -> Weg drumherum suchen
            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval || pathIndex >= path.Count)
            {
                repathTimer = 0f;
                path = GridAStar.FindPath(transform.position, player.position, pathCellSize, obstacleLayer, 1500);
                pathIndex = 0;
            }

            while (pathIndex < path.Count && Vector2.Distance(transform.position, path[pathIndex]) < 0.1f)
                pathIndex++;

            if (pathIndex < path.Count)
                target = path[pathIndex];
        }

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        Move(dir * speed * Time.deltaTime);
    }

    bool HasLineOfSight()
    {
        if (obstacleLayer.value == 0) return true;

        Vector2 from = transform.position;
        Vector2 to = player.position;
        float dist = Vector2.Distance(from, to);
        if (dist < 0.01f) return true;

        return !Physics2D.CircleCast(from, bodyRadius, (to - from) / dist, dist, obstacleLayer);
    }

    // Bewegt den Gegner, gleitet aber an Waenden entlang statt hindurchzulaufen
    void Move(Vector2 delta)
    {
        float dist = delta.magnitude;
        if (dist <= 0f) return;

        if (obstacleLayer.value == 0)
        {
            transform.position += (Vector3)delta;
            return;
        }

        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.CircleCast(pos, bodyRadius, delta / dist, dist + 0.02f, obstacleLayer);

        // Frei, oder Gegner steckt schon im Collider (z.B. beim Spawnen) -> normal weiter
        if (hit.collider == null || hit.distance <= 0f)
        {
            transform.position += (Vector3)delta;
            return;
        }

        // An der Wand entlang gleiten
        Vector2 slide = delta - Vector2.Dot(delta, hit.normal) * hit.normal;
        float sd = slide.magnitude;
        if (sd > 0.0001f && !Physics2D.CircleCast(pos, bodyRadius, slide / sd, sd + 0.02f, obstacleLayer))
            transform.position += (Vector3)slide;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);

        if (path == null) return;
        Gizmos.color = Color.magenta;
        for (int i = pathIndex; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i], path[i + 1]);
    }
}