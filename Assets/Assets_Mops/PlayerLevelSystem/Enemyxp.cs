using UnityEngine;

public class Enemyxp : MonoBehaviour
{
    public int xpAmount = 1;

    [Header("Magnet")]
    public float magnetRange = 3f;
    public float magnetSpeed = 6f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= magnetRange)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerLevel level = other.GetComponent<PlayerLevel>();
        if (level != null)
            level.AddXP(xpAmount);

        Destroy(gameObject);
    }
}
