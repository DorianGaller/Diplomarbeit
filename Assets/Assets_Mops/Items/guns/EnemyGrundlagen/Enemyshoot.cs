using UnityEngine;
using System.Collections;

public class EnemyShoot : MonoBehaviour
{
    public Transform player;
    public GameObject gunshotPrefab;
    public Transform shootPoint;

    public float shootRange = 10f;
    public float shootInterval = 2f;
    private float shootTimer;

    [Header("Line of Sight")]
    [Tooltip("Layer für Wände/Server-Racks, die den Schuss blockieren")]
    public LayerMask obstacleLayer;

    private EnemyAnimator enemyAnimator;
    private EnemyFollow enemyFollow;
    private EnemyPatrol enemyPatrol;   // NEU, optional

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        enemyAnimator = GetComponent<EnemyAnimator>();
        enemyFollow = GetComponent<EnemyFollow>();
        enemyPatrol = GetComponent<EnemyPatrol>();   // NEU
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            if (CanShootPlayer())   // NEU statt reinem Distanz-Check
            {
                Shoot();
                shootTimer = 0f;
            }
        }
    }

    bool CanShootPlayer()   // NEU
    {
        if (player == null) return false;

        // Nur schießen wenn kein EnemyPatrol vorhanden ist, ODER der Gegner laut Patrol alarmiert ist
        if (enemyPatrol != null && !enemyPatrol.isAlerted) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > shootRange) return false;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distance, obstacleLayer);
        if (hit.collider != null) return false;   // Sichtlinie durch Wand/Rack blockiert

        return true;
    }

    private void Shoot()
    {
        if (!player || !gunshotPrefab || !shootPoint) return;

        if (enemyAnimator != null)
        {
            enemyAnimator.TriggerShoot();
            StartCoroutine(ShootSequence(enemyAnimator.GetShootAnimationDuration()));
        }
        else
        {
            FireBullet();
        }
    }

    IEnumerator ShootSequence(float delay)
    {
        if (enemyFollow != null)
            enemyFollow.enabled = false;

        yield return new WaitForSeconds(delay);

        FireBullet();

        if (enemyFollow != null)
            enemyFollow.enabled = true;
    }

    void FireBullet()
    {
        if (!player || !gunshotPrefab || !shootPoint) return;

        Vector2 shootDir = (player.position - shootPoint.position).normalized;

        GameObject shot = Instantiate(
            gunshotPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        shot.GetComponent<GunshotEffect>()?.Init(shootDir);
    }
}