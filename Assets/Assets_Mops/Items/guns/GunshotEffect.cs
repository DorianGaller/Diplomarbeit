using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunshotEffect : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;

    [Header("Damage")]
    public int damage = 25;


    [Header("Piercing")]
    [Tooltip("Wie viele Gegner diese Kugel treffen kann (1 = normal, 2 = geht durch den ersten Gegner durch)")]
    [Min(1)] public int maxTargets = 1;

    private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    [HideInInspector]
    public bool fromPlayer = false;

    [Header("Lifetime & Fade")]
    public float lifeTime = 0.25f;
    public float fadeSpeed = 6f;

    private Vector2 direction = Vector2.up;
    private SpriteRenderer sr;
    private bool hasHit = false;

        public void Init(Vector2 dir)
    {
        if (dir == Vector2.zero)
            dir = Vector2.up;

        direction = dir.normalized;

        // Sprite zeigt bei Rotation 0,0,0 nach oben -> 90° abziehen
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
    }

    void Update()
    {
        if (hasHit) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

            
                EnemyLife enemy = collision.GetComponent<EnemyLife>();
        if (enemy != null)
        {
            // Kein Friendly Fire: Gegner-/Boss-Kugeln fliegen durch Gegner durch
            if (!fromPlayer) return;

            HitTarget(enemy.gameObject, () => enemy.TakeDamage(damage));
            return;
        }

        BossLife boss = collision.GetComponent<BossLife>();
        if (boss != null)
        {
            if (!fromPlayer) return;

            HitTarget(boss.gameObject, () => boss.TakeDamage(damage));
            return;
        }

                PlayerLife player = collision.GetComponent<PlayerLife>();
        if (player != null)
        {
            // Eigene Kugeln fliegen durch den Spieler durch
            if (fromPlayer) return;

            player.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // Feste Collider (Wände, Tische, Kisten, Deko ...) stoppen jede Kugel.
        // Trigger-Zonen (FightRoomTrigger, Interaktions-Bereiche ...) lassen sie durch.
        if (!collision.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }


    // Schaden einmal pro Ziel. Kugel verschwindet erst, wenn maxTargets erreicht sind.
    private void HitTarget(GameObject target, System.Action applyDamage)
    {
        if (!hitTargets.Add(target)) return;   // diesen Gegner hat die Kugel schon getroffen

        applyDamage();

        if (hitTargets.Count >= maxTargets)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(lifeTime);

        Color c = sr.color;
        while (c.a > 0f)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
