using UnityEngine;
using System.Collections;

public class GunshotEffect : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 14f;

    [Header("Lifetime & Fade")]
    public float lifeTime = 0.25f;
    public float fadeSpeed = 6f;

    private Vector2 direction = Vector2.up;   // 🔥 Fallback-Richtung
    private SpriteRenderer sr;

    // Wird DIREKT nach Instantiate aufgerufen
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
        // Bewegung
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            -1f
        );
    }

    IEnumerator Start()
    {
        // Erst fliegen lassen, dann faden
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
