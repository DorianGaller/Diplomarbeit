using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public Camera mainCamera;
    public float cameraPanDuration = 2f;
    public float exitZoomSize = 4f;
    public bool returnCameraAfterPan = true;

    [Header("Player Freeze")]
    public PlayerMovement playerMovement;


    private int enemiesAlive;
    private int enemiesSpawned;
    private int wave = 1;

    public System.Action OnAllWavesCompleted;

    void Start()
    {
        OnAllWavesCompleted += RemoveExitTile;
        OnAllWavesCompleted += StartExitCameraPan;
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (endlessWaves || wave <= maxWaves)
        {
            enemiesSpawned = 0;
            Debug.Log("Wave " + wave + " startet");

            while (enemiesSpawned < enemiesPerWave)
            {
                if (enemiesAlive < maxEnemiesAlive)
                {
                    SpawnEnemy();
                    enemiesSpawned++;
                    enemiesAlive++;
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            // Warten bis alle Gegner tot sind
            while (enemiesAlive > 0)
                yield return null;

            wave++;
            enemiesPerWave += 2; // Schwierigkeit erhöhen
            if (endlessWaves || wave <= maxWaves)
                 yield return new WaitForSeconds(timeBetweenWaves);

        }
        Debug.Log("ALLE WAVES GESCHAFFT!");
        OnAllWavesCompleted?.Invoke();
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

    int halfW = exitWidth / 2;
    int halfH = exitHeight / 2;

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
    if (playerMovement != null)
        playerMovement.enabled = false;

    Transform camTransform = mainCamera.transform;

    Vector3 startPos = camTransform.position;
    Vector3 targetPos = new Vector3(
        exitWorldPosition.position.x,
        exitWorldPosition.position.y,
        camTransform.position.z // Z beibehalten!
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

    // Optional: kurze Pause
    yield return new WaitForSeconds(1f);

    // Optional: Kamera wieder zurück zum Spieler
    if (returnCameraAfterPan)
    {
        camTransform.position = startPos;

        if (mainCamera.orthographic)
            mainCamera.orthographicSize = startSize;
    }
    yield return new WaitForSeconds(1f);

    if (playerMovement != null)
        playerMovement.enabled = true;
}

}
