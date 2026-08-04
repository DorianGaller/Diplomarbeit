using UnityEngine;

[CreateAssetMenu(menuName = "RoomModifiers/Harte Gegner")]
public class Modifier_HardHittingEnemies : RoomModifierSO
{
    [Range(1f, 2f)] public float incomingDamageMultiplier = 1.5f;
    [Range(0f, 1f)] public float coinChanceBonus = 0.4f;   // Ausgleich fürs Risiko

    private PlayerLife playerLife;

    public override void Apply()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerLife = playerObj.GetComponent<PlayerLife>();

        if (playerLife != null)
            playerLife.incomingDamageMultiplier = incomingDamageMultiplier;

        EnemySpawn.OnEnemySpawned += BoostLoot;
    }

    public override void Remove()
    {
        if (playerLife != null)
            playerLife.incomingDamageMultiplier = 1f;

        EnemySpawn.OnEnemySpawned -= BoostLoot;
    }

    private void BoostLoot(GameObject enemy)
    {
        EnemyLife life = enemy.GetComponent<EnemyLife>();
        if (life != null)
            life.coinDropChance = Mathf.Clamp01(life.coinDropChance + coinChanceBonus);
    }
}