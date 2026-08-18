using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class BossLife : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 500;
    private int currentHP;

    [Range(0, 100)]
    public int phase2ThresholdPercent = 50;
    private bool phase2Triggered = false;

    public int CurrentPhase { get; private set; } = 1;
    public bool IsInvulnerable { get; private set; } = false;

    [Header("UI")]
    public BossHealthBarUI healthBarUI;

    [Header("Loot Drop")]
    public GameObject xpPrefab;
    public int xpAmount = 200;
    public int xpOrbCount = 6;

    public GameObject coinPrefab;
    public int coinDropCount = 5;
    public int minCoinsPerDrop = 8;
    public int maxCoinsPerDrop = 15;

    [Header("Exit nach dem Sieg")]
    [Tooltip("Die Wand-Tilemap, in der die Exit-Tiles entfernt werden")]
    public Tilemap exitTilemap;
    [Tooltip("Position, ab der die Tiles entfernt werden (untere linke Ecke des Bereichs)")]
    public Transform exitWorldPosition;
    public int exitWidth = 3;
    public int exitHeight = 2;

    public Action OnDeath;
    public Action OnPhaseTransitionStart;
    public Action OnPhaseTransitionEnd;

    void Start()
    {
        currentHP = maxHP;
    }

    public void ShowHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.ShowBossBar(true);
            UpdateHealthBar();
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsInvulnerable || currentHP <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHealthBar();

        if (!phase2Triggered && currentHP <= maxHP * phase2ThresholdPercent / 100f)
        {
            phase2Triggered = true;
            BeginPhaseTransition();
            return;
        }

        if (currentHP <= 0)
            Die();
    }

    void BeginPhaseTransition()
    {
        IsInvulnerable = true;
        OnPhaseTransitionStart?.Invoke();
    }

    public void EnterPhase2()
    {
        CurrentPhase = 2;
        IsInvulnerable = false;
        OnPhaseTransitionEnd?.Invoke();
        UpdateHealthBar();
    }

    void Die()
    {
        if (healthBarUI != null)
            healthBarUI.ShowBossBar(false);

        DropLoot();
        OpenExit();

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (xpPrefab != null)
        {
            int xpPerOrb = Mathf.Max(1, xpAmount / xpOrbCount);

            for (int i = 0; i < xpOrbCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.3f;
                Vector3 spawnPos = transform.position + (Vector3)offset;
                spawnPos.z = -2.5f;

                GameObject xp = Instantiate(xpPrefab, spawnPos, Quaternion.identity);
                Enemyxp xpScript = xp.GetComponent<Enemyxp>();
                if (xpScript != null)
                    xpScript.xpAmount = xpPerOrb;
            }
        }

        if (coinPrefab != null)
        {
            for (int i = 0; i < coinDropCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * 1.3f;
                Vector3 coinPos = transform.position + (Vector3)offset;
                coinPos.z = -2.5f;

                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                CoinPickup coinScript = coin.GetComponent<CoinPickup>();
                if (coinScript != null)
                    coinScript.value = UnityEngine.Random.Range(minCoinsPerDrop, maxCoinsPerDrop + 1);
            }
        }
    }

    void OpenExit()
    {
        if (exitTilemap == null || exitWorldPosition == null)
        {
            Debug.LogWarning("BossLife: Exit Tilemap oder Exit World Position nicht gesetzt!");
            return;
        }

        Vector3Int centerCell = exitTilemap.WorldToCell(exitWorldPosition.position);

        for (int x = 0; x < exitWidth; x++)
        {
            for (int y = 0; y < exitHeight; y++)
            {
                Vector3Int cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                exitTilemap.SetTile(cellPos, null);
            }
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHP, maxHP, CurrentPhase);
    }
}