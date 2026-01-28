using UnityEngine;
using System;

public class EnemyLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("XP Drop")]
    public GameObject xpPrefab;
    public int minXP = 5;
    public int maxXP = 15;
    public int xpOrbCount = 3;

    public Action OnDeath;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        OnDeath?.Invoke();

        // ✅ FIX 1
        int totalXP = UnityEngine.Random.Range(minXP, maxXP + 1);
        int xpPerOrb = Mathf.Max(1, totalXP / xpOrbCount);

        for (int i = 0; i < xpOrbCount; i++)
        {
            // ✅ FIX 2 (SEHR WICHTIG)
            Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.5f;

            Vector3 spawnPos = transform.position;
            spawnPos.x += offset.x;
            spawnPos.y += offset.y;
            spawnPos.z = -2.5f;

            GameObject xp = Instantiate(xpPrefab, spawnPos, Quaternion.identity);

            Enemyxp xpScript = xp.GetComponent<Enemyxp>();
            if (xpScript != null)
                xpScript.xpAmount = xpPerOrb;
        }

        Destroy(gameObject);
    }
}
