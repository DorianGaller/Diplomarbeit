sssusing UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PlayerLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Heart Sprites")]
    public SpriteRenderer[] hearts;   // SpriteRenderer statt Image
    public float fadeDuration = 0.25f;

    public Action OnDeath;

    void Start()
    {
        currentHP = maxHP;
        UpdateHearts();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHearts();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHearts()
    {
        int maxHearts = hearts.Length;

        // Berechnet wie viele Herzen sichtbar bleiben
        int heartsToShow = Mathf.CeilToInt((float)currentHP / maxHP * maxHearts);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < heartsToShow)
            {
                // Herz sichtbar
                hearts[i].gameObject.SetActive(true);
                SetAlpha(hearts[i], 1f);
            }
            else
            {
                // Herz ausfaden
                if (hearts[i].gameObject.activeSelf)
                {
                    StartCoroutine(FadeOut(hearts[i]));
                }
            }
        }
    }

    IEnumerator FadeOut(SpriteRenderer sprite)
    {
        float t = 0f;
        Color c = sprite.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sprite.color = c;
            yield return null;
        }

        sprite.gameObject.SetActive(false);
    }

    void SetAlpha(SpriteRenderer sprite, float a)
    {
        Color c = sprite.color;
        c.a = a;
        sprite.color = c;
    }

    void Die()
    {
        OnDeath?.Invoke();
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(4f);

        Time.timeScale = 1f;

        SceneManager.LoadScene("Base_v1");
    }
}
