using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    // Diese Methode kannst du von außen aufrufen
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Optional: Animation, Sound, Loot etc.
        Destroy(gameObject);
    }
}
