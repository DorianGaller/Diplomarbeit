using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    public Transform player;

    [Header("Attack Settings")]
    public float attackRange = 1.2f;   // wie nah der Gegner sein muss um zuzuschlagen
    public int damage = 15;
    public float attackInterval = 1f;  // Zeit zwischen zwei Treffern
    private float attackTimer;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= attackRange)
            {
                Attack();
                attackTimer = 0f;
            }
        }
    }

    private void Attack()
    {
        PlayerLife playerLife = player.GetComponent<PlayerLife>();
        if (playerLife != null)
            playerLife.TakeDamage(damage);
    }
}