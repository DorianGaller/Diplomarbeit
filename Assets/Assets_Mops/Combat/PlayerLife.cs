using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class PlayerLife : MonoBehaviour
{
    [Header("Health Settings")]
    public int baseMaxHP = 100;
    private int bonusHP;
    public int maxHP;
    private int currentHP;

    [Header("Health Bar")]
    public Image healthBarFill;
    public TMP_Text healthBarText;
    public float barFillSpeed = 4f;

    private Coroutine barCoroutine;
    private bool initialized = false;   // NEU

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private Color normalColor;
    private Coroutine flashCoroutine;

    public Action OnDeath;

    public float incomingDamageMultiplier = 1f;

    void Start()
    {
        maxHP = baseMaxHP;
        currentHP = maxHP;
        initialized = true;   // NEU

        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (playerSpriteRenderer != null)
            normalColor = playerSpriteRenderer.color;

        StartCoroutine(InitHealthBarNextFrame());
    }

    IEnumerator InitHealthBarNextFrame()
    {
        yield return null;
        UpdateHealthBar(true);
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHealthBar();
    }

    public void SetBonusHP(int newBonus)
    {
        bonusHP = newBonus;

        if (!initialized)   // NEU
        {
            // PlayerLife.Start() ist noch nicht gelaufen (Script-Reihenfolge unklar) -> frisch berechnen statt Delta anwenden
            maxHP = Mathf.Max(1, baseMaxHP + bonusHP);
            currentHP = maxHP;
        }
        else
        {
            int oldMax = maxHP;
            maxHP = Mathf.Max(1, baseMaxHP + bonusHP);
            currentHP += (maxHP - oldMax);
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }

        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * incomingDamageMultiplier);
        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHealthBar();

        if (finalDamage > 0)
            FlashDamage();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void FlashDamage()
    {
        if (playerSpriteRenderer == null) return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    IEnumerator FlashRedCoroutine()
    {
        playerSpriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        playerSpriteRenderer.color = normalColor;
    }

    void UpdateHealthBar(bool instant = false)
    {
        if (healthBarText != null)
            healthBarText.text = currentHP + " / " + maxHP;

        if (healthBarFill == null) return;

        float targetFill = maxHP > 0 ? (float)currentHP / maxHP : 0f;

        if (instant)
        {
            if (barCoroutine != null)
                StopCoroutine(barCoroutine);   // NEU – verhindert, dass eine laufende Animation den Wert wieder überschreibt

            healthBarFill.fillAmount = targetFill;
            healthBarFill.SetAllDirty();
            return;
        }

        if (barCoroutine != null)
            StopCoroutine(barCoroutine);

        barCoroutine = StartCoroutine(AnimateBar(targetFill));
    }

    IEnumerator AnimateBar(float target)
    {
        while (Mathf.Abs(healthBarFill.fillAmount - target) > 0.001f)
        {
            healthBarFill.fillAmount = Mathf.MoveTowards(healthBarFill.fillAmount, target, barFillSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        healthBarFill.fillAmount = target;
        healthBarFill.SetAllDirty();
    }

    void Die()
    {
        OnDeath?.Invoke();
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene("BaseScene");
        else
            SceneManager.LoadScene("BaseScene");
    }
}