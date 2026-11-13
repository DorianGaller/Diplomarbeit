using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bewegung des Hacker-Gegners: haelt Abstand zum Spieler.
/// Zu nah -> flieht, zu weit weg -> kommt naeher (mit Wegsuche um Waende), dazwischen bleibt er stehen.
/// </summary>
public class HackerMovement : MonoBehaviour
{
    public Transform player;
    public float speed = 2.5f;
    [Tooltip("So viel schneller ist er, wenn er flieht")]
    public float fleeSpeedMultiplier = 1.3f;

    [Header("Abstand zum Spieler")]
    [Tooltip("Naeher als das -> weglaufen")]
    public float fleeDistance = 4f;
    [Tooltip("So viel weiter laeuft er weg, bevor er stehen bleibt (fleeDistance + das hier muss kleiner als approachDistance sein)")]
    public float fleeExtraDistance = 2f;
    [Tooltip("Weiter weg als das -> naeher kommen")]
    public float approachDistance = 8f;

    [Header("Waende / Tilemaps")]
    [Tooltip("Layer der Wand-Collider (Tilemap) und fester Deko")]
    public LayerMask obstacleLayer;
    [Tooltip("Radius des Gegners fuer die Kollisionspruefung")]
    public float bodyRadius = 0.35f;
    public Vector2 pathCellSize = new Vector2(0.75f, 0.75f);
    public float repathInterval = 0.4f;
    [Tooltip("So weit schaut er beim Fliehen voraus, ob eine Wand im Weg ist")]
    public float fleeLookAhead = 1.2f;

    public float DistanceToPlayer => player != null ? Vector2.Distance(transform.position, player.position) : Mathf.Infinity;
    public bool IsTooClose => isFleeing || DistanceToPlayer < fleeDistance;

    private List<Vector2> path = new List<Vector2>();
    private int pathIndex;
    private float repathTimer = 999f;
    private Vector2 lastFleeDir;
    private bool isFleeing;   // bleibt an, bis er fleeDistance + fleeExtraDistance weit weg ist

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

        float distance = DistanceToPlayer;

        // Flucht startet unter fleeDistance und endet erst bei fleeDistance + fleeExtraDistance
        if (distance < fleeDistance)
            isFleeing = true;
        else if (distance >= fleeDistance + fleeExtraDistance)
            isFleeing = false;

        if (isFleeing)
        {
            bool moved = Flee();

            // Eingekesselt, aber schon ausserhalb vom Radius -> Flucht beenden (damit er wieder hacken kann)
            if (!moved && distance >= fleeDistance)
                isFleeing = false;
            return;
        }

        lastFleeDir = Vector2.zero;

        if (distance > approachDistance)
        {
            Approach();
            return;
        }

        // Im Wohlfuehl-Abstand: stehen bleiben und den Spieler beobachten
        FaceDirection(player.position - transform.position);
    }

    // ---------- Weglaufen ----------
    bool Flee()
    {
        Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;

        Vector2 best = Vector2.zero;
        float bestScore = -999f;

        // 16 Richtungen pruefen, die freie nehmen, die am meisten vom Spieler weg fuehrt
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI * 2f / 16f;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            if (IsBlocked(dir, fleeLookAhead)) continue;

            float score = Vector2.Dot(dir, away);
            if (lastFleeDir != Vector2.zero)
                score += 0.25f * Vector2.Dot(dir, lastFleeDir);   // Richtung beibehalten, nicht hin und her zappeln

            if (score > bestScore)
            {
                bestScore = score;
                best = dir;
            }
        }

        // Eingekesselt (jede freie Richtung fuehrt zum Spieler hin) -> stehen bleiben
        if (best == Vector2.zero || bestScore < -0.3f)
        {
            FaceDirection(player.position - transform.position);
            return false;
        }

        lastFleeDir = best;
        Move(best * speed * fleeSpeedMultiplier * Time.deltaTime);
        FaceDirection(best);
        return true;
    }

    // ---------- Naeher kommen (wie EnemyFollow) ----------
    void Approach()
    {
        Vector2 target = player.position;

        if (HasLineOfSight())
        {
            path.Clear();
            repathTimer = 999f;
        }
        else
        {
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
        FaceDirection(dir);
    }

    // ---------- Hilfen ----------
    // nach rechts = positive Scale, nach links = negative (wie die anderen Gegner)
    public void FaceDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    bool IsBlocked(Vector2 dir, float distance)
    {
        if (obstacleLayer.value == 0) return false;
        return Physics2D.CircleCast(transform.position, bodyRadius, dir, distance, obstacleLayer).collider != null;
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

        if (hit.collider == null || hit.distance <= 0f)
        {
            transform.position += (Vector3)delta;
            return;
        }

        Vector2 slide = delta - Vector2.Dot(delta, hit.normal) * hit.normal;
        float sd = slide.magnitude;
        if (sd > 0.0001f && !Physics2D.CircleCast(pos, bodyRadius, slide / sd, sd + 0.02f, obstacleLayer))
            transform.position += (Vector3)slide;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, approachDistance);
    }
}