using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public BossLife bossLife;
    public SpriteRenderer spriteRenderer;

    private Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float preferredRangedDistance = 6f;
    public float meleeApproachRange = 1.3f;

    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public int phase1BurstShots = 3;
    public int phase2BurstShots = 5;
    public float shotInterval = 0.2f;

    [Header("Dash Attack")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.25f;
    public GameObject dashGhostPrefab;
    public float ghostSpawnRate = 0.04f;

    [Header("Melee Attack")]
    public int meleeDamage = 20;
    public int phase1ComboHits = 2;
    public int phase2ComboHits = 3;
    public float meleeHitInterval = 0.35f;

    [Header("Phase Transition / Add Waves")]
    public Transform arenaCenter;
    public GameObject[] wave1Enemies;
    public GameObject[] wave2Enemies;
    public Vector3 addSpawnAreaSize = new Vector3(8, 0, 8);
    public float minAddDistanceFromPlayer = 3f;
    public float timeBetweenAddWaves = 2f;
    public GameObject invulnerableVFX;

    [Header("Invulnerable Feedback")]
    public Color invulnerableTint = new Color(1f, 1f, 1f, 0.5f);   // NEU
    private Color normalTint;

    private int aliveAdds = 0;
    private bool inTransition = false;
    private bool isActive = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (bossLife == null) bossLife = GetComponent<BossLife>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        bossLife.OnPhaseTransitionStart += () => StartCoroutine(PhaseTransitionSequence());

        if (spriteRenderer != null)
            normalTint = spriteRenderer.color;

        // KEIN automatischer AttackLoop-Start mehr — wartet auf ActivateBoss()
    }

    void Update()
    {
        if (!isActive) return;
        FaceLookDirection();
    }

    public void ActivateBoss()
    {
        if (isActive) return;
        isActive = true;

        if (bossLife != null)
            bossLife.ShowHealthBar();

        StartCoroutine(AttackLoop());
    }

    void FaceLookDirection()
    {
        if (player == null || spriteRenderer == null) return;
        float dirX = player.position.x - transform.position.x;
        if (Mathf.Abs(dirX) > 0.05f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (dirX < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            if (inTransition || player == null)
            {
                yield return null;
                continue;
            }

            yield return StartCoroutine(MoveToPreferredRange());
            if (inTransition) continue;

            yield return StartCoroutine(RangedBurst());
            yield return new WaitForSeconds(0.4f);
            if (inTransition) continue;

            yield return StartCoroutine(DashStrike());
            if (inTransition) continue;

            yield return StartCoroutine(MeleeCombo());

            yield return new WaitForSeconds(bossLife.CurrentPhase == 1 ? 1f : 0.4f);
        }
    }

    IEnumerator MoveToPreferredRange()
    {
        float timeout = 3f;
        float t = 0f;

        while (t < timeout)
        {
            if (inTransition || player == null) yield break;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance < preferredRangedDistance - 0.5f)
            {
                Vector3 away = (transform.position - player.position).normalized;
                transform.position += away * moveSpeed * Time.deltaTime;
            }
            else if (distance > preferredRangedDistance + 0.5f)
            {
                Vector3 toward = (player.position - transform.position).normalized;
                transform.position += toward * moveSpeed * Time.deltaTime;
            }
            else
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator RangedBurst()
    {
        int shots = bossLife.CurrentPhase == 1 ? phase1BurstShots : phase2BurstShots;

        for (int i = 0; i < shots; i++)
        {
            if (inTransition) yield break;
            FireShot();
            yield return new WaitForSeconds(shotInterval);
        }
    }

    void FireShot()
    {
        if (bulletPrefab == null || shootPoint == null || player == null) return;

        Vector2 dir = (player.position - shootPoint.position).normalized;
        GameObject shot = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        shot.GetComponent<GunshotEffect>()?.Init(dir);
    }

    IEnumerator DashStrike()
    {
        if (player == null) yield break;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < meleeApproachRange) yield break;

        Vector3 dashDir = (player.position - transform.position).normalized;
        StartCoroutine(SpawnDashGhosts());

        float t = 0f;
        while (t < dashDuration)
        {
            if (inTransition) yield break;
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator SpawnDashGhosts()
    {
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            SpawnGhost();
            yield return new WaitForSeconds(ghostSpawnRate);
            elapsed += ghostSpawnRate;
        }
    }

    void SpawnGhost()
    {
        if (dashGhostPrefab == null || spriteRenderer == null) return;

        GameObject ghost = Instantiate(dashGhostPrefab, transform.position, transform.rotation);
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
        if (ghostSR != null)
        {
            ghostSR.sprite = spriteRenderer.sprite;
            ghostSR.flipX = spriteRenderer.flipX;
            Color c = ghostSR.color;
            c.a = 0.5f;
            ghostSR.color = c;
        }
    }

    IEnumerator MeleeCombo()
    {
        int hits = bossLife.CurrentPhase == 1 ? phase1ComboHits : phase2ComboHits;

        for (int i = 0; i < hits; i++)
        {
            if (inTransition || player == null) yield break;

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= meleeApproachRange + 0.3f)
            {
                PlayerLife pl = player.GetComponent<PlayerLife>();
                if (pl != null)
                    pl.TakeDamage(meleeDamage);
            }

            yield return new WaitForSeconds(meleeHitInterval);
        }
    }

    IEnumerator PhaseTransitionSequence()
    {
        inTransition = true;

        if (spriteRenderer != null)
            spriteRenderer.color = invulnerableTint;

        if (invulnerableVFX != null)
            invulnerableVFX.SetActive(true);

        if (arenaCenter != null)
        {
            float t = 0f;
            Vector3 start = transform.position;
            while (t < 1f)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, arenaCenter.position, t);
                yield return null;
            }
        }

        yield return SpawnAddWave(wave1Enemies);
        yield return new WaitForSeconds(timeBetweenAddWaves);
        yield return SpawnAddWave(wave2Enemies);

        if (invulnerableVFX != null)
            invulnerableVFX.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.color = normalTint;

        inTransition = false;
        bossLife.EnterPhase2();
    }

    IEnumerator SpawnAddWave(GameObject[] enemies)
    {
        if (enemies == null || enemies.Length == 0) yield break;

        aliveAdds = 0;

        foreach (GameObject prefab in enemies)
        {
            if (prefab == null) continue;

            Vector3 pos = GetValidAddSpawnPosition();
            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
            aliveAdds++;

            EnemyLife life = enemy.GetComponent<EnemyLife>();
            if (life != null)
                life.OnDeath += () => aliveAdds--;
        }

        while (aliveAdds > 0)
            yield return null;
    }

    Vector3 GetValidAddSpawnPosition()
    {
        Vector3 center = arenaCenter != null ? arenaCenter.position : transform.position;
        Vector3 pos = center;
        int attempts = 0;

        do
        {
            pos = center + new Vector3(
                Random.Range(-addSpawnAreaSize.x / 2, addSpawnAreaSize.x / 2),
                Random.Range(-addSpawnAreaSize.y / 2, addSpawnAreaSize.y / 2),
                0
            );
            attempts++;
        }
        while (player != null
               && Vector3.Distance(pos, player.position) < minAddDistanceFromPlayer
               && attempts < 20);

        return pos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, preferredRangedDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeApproachRange);

        if (arenaCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(arenaCenter.position, addSpawnAreaSize);
        }
    }
}