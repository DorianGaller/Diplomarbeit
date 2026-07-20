using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Health Bar")]
    public Image healthFill;   // das grüne Fill-Image

    [Header("XP Drop")]
    public GameObject xpPrefab;
    public int minXP = 5;
    public int maxXP = 15;
    public int xpOrbCount = 3;

    public Action OnDeath;

    void Start()
    {
        currentHP = maxHP;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Treffer! Schaden: " + damage);
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHealthBar();

        if (currentHP <= 0)
            Die();
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHP / maxHP;
        }
    }

    void Die()
    {
        OnDeath?.Invoke();

        int totalXP = UnityEngine.Random.Range(minXP, maxXP + 1);
        int xpPerOrb = Mathf.Max(1, totalXP / xpOrbCount);

        for (int i = 0; i < xpOrbCount; i++)
        {
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
