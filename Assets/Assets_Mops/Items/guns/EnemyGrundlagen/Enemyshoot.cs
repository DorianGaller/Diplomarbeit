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
        [Tooltip("Dicke des Schusses fuer die Sichtpruefung (etwas groesser als die Kugel)")]
    public float bulletRadius = 0.15f;


    [Header("Bewegung")]
    [Tooltip("Aus = Gegner laeuft beim Schiessen weiter")]
    public bool stopWhileShooting = false;

    [Header("Sound")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 0.5f;

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

                if (!HasClearShot()) return false;   // Wand/Rack zwischen Muendung und Spieler
        return true;
    }

        /// <summary>Wuerde ein Schuss von der Muendung aus den Spieler erreichen? (auch fuer EnemyFollow)</summary>
    public bool HasClearShot()
    {
        if (player == null) return false;
        if (obstacleLayer.value == 0) return true;

        Vector2 from = shootPoint != null ? (Vector2)shootPoint.position : (Vector2)transform.position;
        Vector2 to = player.position;
        float dist = Vector2.Distance(from, to);
        if (dist < 0.01f) return true;

        // Steckt die Muendung schon in der Wand, trifft der CircleCast bei Distanz 0 -> auch blockiert
        return !Physics2D.CircleCast(from, bulletRadius, (to - from) / dist, dist, obstacleLayer);
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
        bool stopped = stopWhileShooting && enemyFollow != null && enemyFollow.enabled;
        if (stopped)
            enemyFollow.enabled = false;

        yield return new WaitForSeconds(delay);

        FireBullet();

        if (stopped)
            enemyFollow.enabled = true;
    }

    void FireBullet()
    {
        if (!player || !gunshotPrefab || !shootPoint) return;

                // Spieler ist waehrend der Schuss-Animation hinter Deckung gegangen -> nicht schiessen
        if (!HasClearShot()) return;

        Vector2 shootDir = (player.position - shootPoint.position).normalized;

        GameObject shot = Instantiate(
            gunshotPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        shot.GetComponent<GunshotEffect>()?.Init(shootDir);

        EnemySound.Play(this, shootSound, shootVolume);
    }
}