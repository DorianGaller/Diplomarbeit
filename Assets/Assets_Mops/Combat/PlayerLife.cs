using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("UI Hearts")]
    public Image[] hearts;         // Herz-Images aus dem Canvas (links → rechts)
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

        // Berechnet wie viele Herzen noch voll sind
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

                hearts[i].gameObject.SetActive(false);
            }
        }
    }

    IEnumerator FadeOut(Image img)
    {
        float t = 0f;
        Color c = img.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            img.color = c;
            yield return null;
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    void Die()
{
    OnDeath?.Invoke();
    StartCoroutine(RespawnAfterDelay());
}

IEnumerator RespawnAfterDelay()
{
    Time.timeScale = 0f;

    // Optional: UI Effekt oder Deathscreen aktivieren
    // z.B. canvasGameOver.SetActive(true);

    yield return new WaitForSecondsRealtime(4f);

    Time.timeScale = 1f;

    SceneManager.LoadScene("Base_v1");
}
}
