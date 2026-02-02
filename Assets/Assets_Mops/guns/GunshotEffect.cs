using UnityEngine;
using System.Collections;

public class GunshotEffect : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;

    [Header("Damage")]
    public int damage = 25;

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

    // Enemy treffen
    EnemyLife enemy = collision.GetComponent<EnemyLife>();
    if (enemy != null)
    {
        enemy.TakeDamage(damage);
        hasHit = true;
        Destroy(gameObject);
        return;
    }

    
    PlayerLife player = collision.GetComponent<PlayerLife>();
    if (player != null)
    {
        player.TakeDamage(damage);
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
