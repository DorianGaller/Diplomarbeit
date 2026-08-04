using UnityEngine;

[CreateAssetMenu(menuName = "RoomModifiers/Schnelle Gegner")]
public class Modifier_FastEnemies : RoomModifierSO
{
    [Range(1f, 2.5f)] public float speedMultiplier = 1.5f;

    public override void Apply()
    {
        EnemySpawn.OnEnemySpawned += SpeedUp;
    }

    public override void Remove()
    {
        EnemySpawn.OnEnemySpawned -= SpeedUp;
    }

    private void SpeedUp(GameObject enemy)
    {
        EnemyFollow follow = enemy.GetComponent<EnemyFollow>();
        if (follow != null)
            follow.speed *= speedMultiplier;
    }
}