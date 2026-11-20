using UnityEngine;

/// <summary>Startet den KERNEL-Kampf, sobald der Spieler den Trigger betritt (wie BossRoomTrigger).</summary>
[RequireComponent(typeof(Collider2D))]
public class KernelRoomTrigger : MonoBehaviour
{
    [SerializeField] private KernelBoss kernel;
    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        if (kernel != null) kernel.ActivateBoss();
    }
}