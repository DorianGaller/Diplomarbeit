using System.Collections;
using UnityEngine;

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


    private int enemiesAlive;
    private int enemiesSpawned;
    private int wave = 1;

    public System.Action OnAllWavesCompleted;

    void Start()
    {
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
    StartCoroutine(WaveLoop());
}

}
