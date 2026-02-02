using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

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
        OnDeath?.Invoke(); // optionales Death-Event
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("Base_v1");
    }
}
