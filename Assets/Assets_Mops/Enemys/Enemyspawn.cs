using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class EnemySpawn : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;
    public int maxEnemiesAlive = 5;

    [Header("Wave")]
    public int enemiesPerWave = 10;
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

    public System.Action OnAllWavesCompleted;

    void Start()
    {
        // MainCamera per Tag suchen
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogWarning("Keine Kamera mit Tag 'MainCamera' gefunden!");

        // Player per Tag suchen
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        else
            Debug.LogWarning("Kein GameObject mit Tag 'Player' gefunden!");

        // UI per Tag suchen
        uiRoot = GameObject.FindGameObjectWithTag("PlayerUI");
        if (uiRoot == null)
            Debug.LogWarning("Kein GameObject mit Tag 'PlayerUI' gefunden!");

        // HUD zuerst initialisieren
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

        // Erst danach die Coroutine starten
        OnAllWavesCompleted += RemoveExitTile;
        OnAllWavesCompleted += StartExitCameraPan;
        StartCoroutine(WaveLoop());
        UpdateHUD();
    }

    IEnumerator WaveLoop()
    {
        while (endlessWaves || wave <= maxWaves)
        {
            enemiesSpawned = 0;
            Debug.Log("Wave " + wave + " startet");

            UpdateHUD();

            while (enemiesSpawned < enemiesPerWave)
            {
                if (enemiesAlive < maxEnemiesAlive)
                {
                    SpawnEnemy();
                    enemiesSpawned++;
                    enemiesAlive++;
                    UpdateHUD();
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            // Warten bis alle Gegner tot sind
            while (enemiesAlive > 0)
                yield return null;

            wave++;
            enemiesPerWave += 2;
            if (endlessWaves || wave <= maxWaves)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("ALLE WAVES GESCHAFFT!");
        OnAllWavesCompleted?.Invoke();

        if (hudRoot != null)
            hudRoot.gameObject.SetActive(false);
    }

    void SpawnEnemy()
    {
        Vector3 pos = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            0
        );

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

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
            enemiesText.text = Mathf.Max(0, enemiesPerWave - enemiesSpawned + enemiesAlive) + " Enemys\nRemaining";
    }
}
