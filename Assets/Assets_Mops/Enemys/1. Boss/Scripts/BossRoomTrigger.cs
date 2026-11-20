using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossRoomTrigger : MonoBehaviour
{
    [Tooltip("Der Boss, der bei Betreten aktiviert werden soll")]
    [SerializeField] private BossController bossController;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (bossController != null)
            bossController.ActivateBoss();
    }
}