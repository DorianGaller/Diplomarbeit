using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class EnemySpawn : MonoBehaviour
{
    [System.Serializable]
    public class EnemyTypeCount
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [System.Serializable]
    public class WaveConfig
    {
        public EnemyTypeCount[] enemyCounts;
    }

    [Header("Wave Configuration")]
    [Tooltip("Ein Eintrag pro Welle. Index 0 = Welle 1, Index 1 = Welle 2, usw.")]
    public WaveConfig[] waveConfigs;

    [Header("Spawn Settings")]
    public int maxEnemiesAlive = 5;
    public float spawnDelay = 1.2f;
    public float timeBetweenWaves = 4f;

    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    [Header("Waves Settings")]
    public int maxWaves = 5;
    public bool endlessWaves = false;

    [Header("Exit Tile")]
    public Tilemap exitTilemap;
    public Transform exitWorldPosition;

    [Header("Exit Area Size")]
    public int exitWidth = 3;
    public int exitHeight = 2;

    [Header("Exit Camera Pan")]
    public float cameraPanDuration = 2f;
    public float exitZoomSize = 4f;
    public bool returnCameraAfterPan = true;
    public Transform cameraPanTarget;

    [Header("HUD")]
    public Transform hudRoot;

    private TextMeshProUGUI wavesText;
    private TextMeshProUGUI enemiesText;

    private PlayerMovement playerMovement;
    private GameObject uiRoot;
    private Camera mainCamera;

    private int enemiesAlive;
    private int enemiesSpawned;
    private int wave = 1;

    private List<GameObject> spawnQueue = new List<GameObject>();

    public System.Action OnAllWavesCompleted;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("Keine Kamera mit Tag 'MainCamera' gefunden!");

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        else
            Debug.LogWarning("Kein GameObject mit Tag 'Player' gefunden!");

        uiRoot = GameObject.FindGameObjectWithTag("PlayerUI");
        if (uiRoot == null)
            Debug.LogWarning("Kein GameObject mit Tag 'PlayerUI' gefunden!");

        if (hudRoot != null)
        {
            TextMeshProUGUI[] texts = hudRoot.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                wavesText = texts[0];
                enemiesText = texts[1];
            }
            else
                Debug.LogWarning("hudRoot braucht mindestens 2 TMP-Kinder!");
        }
        else
            Debug.LogWarning("HUD Root nicht gesetzt!");

        OnAllWavesCompleted += RemoveExitTile;
        OnAllWavesCompleted += StartExitCameraPan;
        StartCoroutine(WaveLoop());
        UpdateHUD();
    }

    // ── NEU: baut die Spawn-Liste für die aktuelle Welle ──
    void BuildSpawnQueue()
    {
        spawnQueue.Clear();

        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            Debug.LogWarning("Keine Wave Configs im Inspector gesetzt!");
            return;
        }

        // Falls mehr Wellen laufen als konfiguriert (z.B. bei endlessWaves),
        // wird einfach die letzte definierte Welle wiederverwendet
        int configIndex = Mathf.Min(wave - 1, waveConfigs.Length - 1);
        WaveConfig config = waveConfigs[configIndex];

        foreach (EnemyTypeCount entry in config.enemyCounts)
        {
            for (int i = 0; i < entry.count; i++)
                spawnQueue.Add(entry.enemyPrefab);
        }

        // Reihenfolge mischen, damit nicht erst alle Melees und dann alle Normalen kommen
        for (int i = 0; i < spawnQueue.Count; i++)
        {
            int rnd = Random.Range(i, spawnQueue.Count);
            (spawnQueue[i], spawnQueue[rnd]) = (spawnQueue[rnd], spawnQueue[i]);
        }
    }

    IEnumerator WaveLoop()
    {
        while (endlessWaves || wave <= maxWaves)
        {
            enemiesSpawned = 0;
            BuildSpawnQueue();

            Debug.Log("Wave " + wave + " startet mit " + spawnQueue.Count + " Gegnern");
            UpdateHUD();

            while (enemiesSpawned < spawnQueue.Count)
            {
                if (enemiesAlive < maxEnemiesAlive)
                {
                    SpawnEnemy(spawnQueue[enemiesSpawned]);
                    enemiesSpawned++;
                    enemiesAlive++;
                    UpdateHUD();
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            while (enemiesAlive > 0)
                yield return null;

            wave++;
            if (endlessWaves || wave <= maxWaves)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("ALLE WAVES GESCHAFFT!");
        OnAllWavesCompleted?.Invoke();

        if (hudRoot != null)
            hudRoot.gameObject.SetActive(false);
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            0
        );

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        EnemyLife life = enemy.GetComponent<EnemyLife>();
        if (life != null)
            life.OnDeath += EnemyDied;
    }

    void EnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        UpdateHUD();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();
        enemiesAlive = 0;
        enemiesSpawned = 0;
        wave = 1;
        exitOpened = false;

        if (hudRoot != null)
            hudRoot.gameObject.SetActive(true);

        UpdateHUD();
        StartCoroutine(WaveLoop());
    }

    private bool exitOpened = false;

    void RemoveExitTile()
    {
        if (exitOpened) return;

        if (exitTilemap == null || exitWorldPosition == null)
        {
            Debug.LogWarning("Exit Tilemap oder Exit Position nicht gesetzt!");
            return;
        }

        Vector3Int centerCell = exitTilemap.WorldToCell(exitWorldPosition.position);

        for (int x = 0; x < exitWidth; x++)
        {
            for (int y = 0; y < exitHeight; y++)
            {
                Vector3Int cellPos = new Vector3Int(
                    centerCell.x + x,
                    centerCell.y + y,
                    centerCell.z
                );

                exitTilemap.SetTile(cellPos, null);
            }
        }

        exitOpened = true;
    }

    void StartExitCameraPan()
    {
        if (mainCamera == null || exitWorldPosition == null)
        {
            Debug.LogWarning("Kamera oder ExitWorldPosition nicht gesetzt!");
            return;
        }

        StartCoroutine(PanCameraToExit());
    }

    IEnumerator PanCameraToExit()
    {
        if (uiRoot != null)
            uiRoot.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        Transform camTransform = mainCamera.transform;

        Vector3 startPos = camTransform.position;
        Transform target = cameraPanTarget != null ? cameraPanTarget : exitWorldPosition;

        Vector3 targetPos = new Vector3(
            target.position.x,
            target.position.y,
            camTransform.position.z
        );

        float startSize = mainCamera.orthographicSize;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / cameraPanDuration;

            camTransform.position = Vector3.Lerp(startPos, targetPos, t);

            if (mainCamera.orthographic)
                mainCamera.orthographicSize = Mathf.Lerp(startSize, exitZoomSize, t);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        if (returnCameraAfterPan)
        {
            camTransform.position = startPos;

            if (mainCamera.orthographic)
                mainCamera.orthographicSize = startSize;
        }

        yield return new WaitForSeconds(1f);

        if (uiRoot != null)
            uiRoot.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    void UpdateHUD()
    {
        if (wavesText != null)
            wavesText.text = Mathf.Max(0, maxWaves - wave) + " Waves\nRemaining";

        if (enemiesText != null)
            enemiesText.text = Mathf.Max(0, spawnQueue.Count - enemiesSpawned + enemiesAlive) + " Enemys\nRemaining";
    }
}