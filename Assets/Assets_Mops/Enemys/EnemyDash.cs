using UnityEngine;
using System.Collections;

public class EnemyDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2.5f;

    [Header("Dash Trigger")]
    [Tooltip("Ab welcher Distanz zum Spieler ein Dash ausgelöst werden kann")]
    public float dashTriggerRange = 5f;
    [Tooltip("Unter dieser Distanz wird nicht mehr gedasht (schon nah genug für Nahkampf)")]
    public float minDashRange = 1.5f;

    [Header("Ghost Trail")]
    public GameObject dashGhostPrefab;
    public float ghostSpawnRate = 0.05f;

    private bool isDashing = false;
    private bool canDash = true;
    private float cooldownTimer = 0f;

    private Transform player;
    private EnemyFollow follow;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        follow = GetComponent<EnemyFollow>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || isDashing) return;

        if (!canDash)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= dashCooldown)
            {
                canDash = true;
                cooldownTimer = 0f;
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= dashTriggerRange && distance >= minDashRange)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        if (follow != null)
            follow.enabled = false;   // normales Verfolgen pausieren, damit sich beide Bewegungen nicht überschneiden

        Vector3 dashDirection = (player.position - transform.position).normalized;

        StartCoroutine(SpawnGhosts());

        float t = 0f;
        while (t < dashDuration)
        {
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        if (follow != null)
            follow.enabled = true;   // normales Verfolgen läuft weiter
    }

    IEnumerator SpawnGhosts()
    {
        while (isDashing)
        {
            SpawnGhost();
            yield return new WaitForSeconds(ghostSpawnRate);
        }
    }

    void SpawnGhost()
    {
        if (dashGhostPrefab == null || spriteRenderer == null) return;

        GameObject ghost = Instantiate(
            dashGhostPrefab,
            transform.position,
            transform.rotation
        );

        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();

        if (ghostSR != null)
        {
            ghostSR.sprite = spriteRenderer.sprite;
            ghostSR.flipX = spriteRenderer.flipX;
            ghostSR.flipY = spriteRenderer.flipY;

            Color c = ghostSR.color;
            c.a = 0.5f;
            ghostSR.color = c;
        }
    }
}