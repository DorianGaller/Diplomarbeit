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
    public float minDistanceFromPlayer = 3f;

    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    [Header("Waves Settings")]
    public int maxWaves = 5;
    public bool endlessWaves = false;

    [Header("Room Modifier")]
    [Tooltip("Wird von Raum-Modifikatoren gesetzt, 1 = normal")]
    public float enemyCountMultiplier = 1f;

    [Header("Exit Tile")]
    public Tilemap exitTilemap;
    public Transform exitWorldPosition;

    [Header("Exit Area Size")]
    public int exitWidth = 3;
    public int exitHeight = 2;

    [Header("Exit Camera Pan")]
    [Tooltip("Dauer der Fahrt hin bzw. zurueck (Sekunden)")]
    public float cameraPanDuration = 1f;
    [Tooltip("Zoom beim Ausgang (Orthographic Size, Spiel-Kamera hat 8). 0 = nicht zoomen")]
    public float exitZoomSize = 6f;
    [Tooltip("So lange bleibt die Kamera beim Ausgang")]
    public float cameraHoldTime = 1.5f;
    public bool returnCameraAfterPan = true;
    public Transform cameraPanTarget;

    [Header("Sound")]
    [Tooltip("Kurzer Jingle, wenn alle Wellen geschafft sind (leer = kein Sound)")]
    public AudioClip clearedSound;
    [Range(0f, 1f)] public float clearedVolume = 0.8f;

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

    private bool roomActive = false;

    public bool IsCleared { get; private set; }   // NEU – Raum startet nicht mehr automatisch

    public System.Action OnAllWavesCompleted;
    public static event System.Action<GameObject> OnEnemySpawned;   // NEU

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
            // true = auch finden, wenn das WaveUI gerade aus ist
            TextMeshProUGUI[] texts = hudRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
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

        OnAllWavesCompleted += StartExitCameraPan;
        OnAllWavesCompleted += MarkRoomCleared;

        // Laut Spielstand schon geschafft -> nicht nochmal starten, Ausgang gleich offen
        if (SaveSystem.IsCleared(SaveSystem.GetObjectId(gameObject)))
        {
            IsCleared = true;
            roomActive = true;   // StartRoom() macht dann nichts mehr
            RemoveExitTile();
        }
        UpdateHUD();

        // WaveUI erst zeigen, wenn ein Raum wirklich startet
        if (hudRoot != null)
            hudRoot.gameObject.SetActive(false);

        // KEIN automatischer Start mehr — wartet auf StartRoom()
    }

    // NEU: wird vom FightRoomTrigger aufgerufen, sobald der Spieler den Raum betritt
    public void StartRoom()
    {
        if (roomActive) return;
        roomActive = true;

        // Jeder neue Angriff zeigt das WaveUI wieder an (auch wenn ein vorheriger Raum es ausgeschaltet hat)
        if (hudRoot != null)
            hudRoot.gameObject.SetActive(true);

        UpdateHUD();
        StartCoroutine(WaveLoop());
    }

    void BuildSpawnQueue()
    {
        spawnQueue.Clear();

        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            Debug.LogWarning("Keine Wave Configs im Inspector gesetzt!");
            return;
        }

        int configIndex = Mathf.Min(wave - 1, waveConfigs.Length - 1);
        WaveConfig config = waveConfigs[configIndex];

        foreach (EnemyTypeCount entry in config.enemyCounts)
        {
            int adjustedCount = Mathf.CeilToInt(entry.count * enemyCountMultiplier);
            for (int i = 0; i < adjustedCount; i++)
                spawnQueue.Add(entry.enemyPrefab);
        }

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

        Vector3 pos = GetValidSpawnPosition();   // NEU

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        EnemyLife life = enemy.GetComponent<EnemyLife>();
        if (life != null)
            life.OnDeath += EnemyDied;

        OnEnemySpawned?.Invoke(enemy);
    }

    Vector3 GetValidSpawnPosition()   // NEU
    {
        Vector3 pos = transform.position;
        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            pos = transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                0
            );
            attempts++;
        }
        while (playerMovement != null
               && Vector3.Distance(pos, playerMovement.transform.position) < minDistanceFromPlayer
               && attempts < maxAttempts);

        return pos;
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
        roomActive = true;

        if (hudRoot != null)
            hudRoot.gameObject.SetActive(true);

        UpdateHUD();
        StartCoroutine(WaveLoop());
    }

    private bool exitOpened = false;

    // Merkt sich die entfernten Exit-Tiles, damit die Tuer wieder zugehen kann
    private readonly List<(Vector3Int cell, TileBase tile)> removedExitTiles = new List<(Vector3Int, TileBase)>();

    void MarkRoomCleared()
    {
        IsCleared = true;
        SaveSystem.MarkCleared(SaveSystem.GetObjectId(gameObject));
    }


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

                removedExitTiles.Add((cellPos, exitTilemap.GetTile(cellPos)));
                exitTilemap.SetTile(cellPos, null);
            }
        }

        exitOpened = true;
    }

    /// <summary>
    /// Tuer wieder zu und Raum gilt als nicht geschafft (z.B. beim Verlassen durch den Exit).
    /// Kommt der Spieler zurueck (Tod, Elevator), muss er den Raum neu kaempfen.
    /// </summary>
    public void CloseRoom()
    {
        IsCleared = false;   // DoorTilemapVisual schaltet dadurch das rote Tor wieder an
        SaveSystem.Unclear(SaveSystem.GetObjectId(gameObject));

        if (exitTilemap != null)
            foreach (var t in removedExitTiles)
                exitTilemap.SetTile(t.cell, t.tile);
        removedExitTiles.Clear();
        exitOpened = false;
    }

    void StartExitCameraPan()
    {
        if (clearedSound != null)
            EnemySound.GetSource(this).PlayOneShot(clearedSound, clearedVolume);

        if (mainCamera == null) mainCamera = Camera.main;

        // Ohne Kamera oder Ausgang: Ausgang trotzdem oeffnen, nur ohne Fahrt
        if (mainCamera == null || exitWorldPosition == null)
        {
            RemoveExitTile();
            return;
        }

        StartCoroutine(PanCameraToExit());
    }

    IEnumerator PanCameraToExit()
    {
        if (uiRoot != null) uiRoot.SetActive(false);
        SetPlayerLocked(true);

        Transform camT = mainCamera.transform;
        Vector3 localStart = camT.localPosition;   // Kamera haengt am Spieler
        float startSize = mainCamera.orthographicSize;
        Transform target = cameraPanTarget != null ? cameraPanTarget : exitWorldPosition;

        // Hin
        Vector3 from = camT.position;
        Vector3 to = new Vector3(target.position.x, target.position.y, from.z);
        float zoom = exitZoomSize > 0f ? exitZoomSize : startSize;
        yield return MoveCamera(from, to, startSize, zoom);

        // Erst jetzt geht der Ausgang auf, waehrend die Kamera hinschaut
        RemoveExitTile();
        yield return new WaitForSeconds(cameraHoldTime);

        // Zurueck zum Spieler
        if (returnCameraAfterPan)
        {
            Vector3 back = camT.parent != null ? camT.parent.TransformPoint(localStart) : from;
            yield return MoveCamera(camT.position, back, mainCamera.orthographicSize, startSize);
            camT.localPosition = localStart;
            mainCamera.orthographicSize = startSize;
        }

        SetPlayerLocked(false);
        if (uiRoot != null) uiRoot.SetActive(true);
    }

    IEnumerator MoveCamera(Vector3 from, Vector3 to, float sizeFrom, float sizeTo)
    {
        float duration = Mathf.Max(0.01f, cameraPanDuration);
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            float e = Mathf.SmoothStep(0f, 1f, t);
            mainCamera.transform.position = Vector3.Lerp(from, to, e);
            if (mainCamera.orthographic) mainCamera.orthographicSize = Mathf.Lerp(sizeFrom, sizeTo, e);
            yield return null;
        }

        mainCamera.transform.position = to;
        if (mainCamera.orthographic) mainCamera.orthographicSize = sizeTo;
    }

    void SetPlayerLocked(bool locked)
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        PlayerMovement movement = p.GetComponentInChildren<PlayerMovement>();
        if (movement != null) movement.SetMovementInputLocked(locked);

        InteractKeys keys = p.GetComponentInChildren<InteractKeys>();
        if (keys != null) keys.EnableShooting(!locked);
    }

    void UpdateHUD()
    {
        if (wavesText != null)
            wavesText.text = Mathf.Max(0, maxWaves - wave) + " Waves \n remaining";

        if (enemiesText != null)
            enemiesText.text = Mathf.Max(0, spawnQueue.Count - enemiesSpawned + enemiesAlive) + " Enemys";
    }
}