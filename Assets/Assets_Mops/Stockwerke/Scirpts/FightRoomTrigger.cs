using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FightRoomTrigger : MonoBehaviour
{
    [Header("Zugehöriger Fight Room")]
    [Tooltip("Der EnemySpawn, der bei Betreten gestartet werden soll")]
    [SerializeField] private EnemySpawn enemySpawn;

    [Tooltip("Der RoomModifierManager dieses Raums (optional, kann leer bleiben)")]
    [SerializeField] private RoomModifierManager roomModifierManager;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (roomModifierManager != null)
            roomModifierManager.ActivateRoom();   // ZUERST Modifikator setzen

        if (enemySpawn != null)
            enemySpawn.StartRoom();               // DANN Wellen starten
    }
}