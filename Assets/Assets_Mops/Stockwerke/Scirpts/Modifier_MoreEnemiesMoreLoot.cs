using UnityEngine;

[CreateAssetMenu(menuName = "RoomModifiers/Mehr Gegner mehr Loot")]
public class Modifier_MoreEnemiesMoreLoot : RoomModifierSO
{
    [Range(1f, 3f)] public float enemyCountMultiplier = 1.5f;
    [Range(0f, 1f)] public float coinChanceBonus = 0.3f;

    private EnemySpawn spawner;

    public override void Apply()
    {
        spawner = GameObject.FindFirstObjectByType<EnemySpawn>();
        if (spawner != null)
            spawner.enemyCountMultiplier = enemyCountMultiplier;

        EnemySpawn.OnEnemySpawned += BoostLoot;
    }

    public override void Remove()
    {
        if (spawner != null)
            spawner.enemyCountMultiplier = 1f;

        EnemySpawn.OnEnemySpawned -= BoostLoot;
    }

    private void BoostLoot(GameObject enemy)
    {
        EnemyLife life = enemy.GetComponent<EnemyLife>();
        if (life != null)
            life.coinDropChance = Mathf.Clamp01(life.coinDropChance + coinChanceBonus);
    }
}