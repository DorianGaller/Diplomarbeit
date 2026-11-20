using UnityEngine;

/// <summary>
/// Strom-Knoten des KERNEL. Der eigentliche Knoten ist ein inaktives Kind-Objekt ("NodeTemplate")
/// mit Sprite, Collider und EnemyLife. Revive() erzeugt davon eine frische Kopie, Spieler-Kugeln
/// treffen sie wie einen normalen Gegner. Darunter zeigt der Sockel das "zerstoert"-Sprite.
/// </summary>
public class KernelPowerNode : MonoBehaviour
{
    [Tooltip("Inaktives Kind: kernel_node_on + CircleCollider2D + EnemyLife (XP Prefab eintragen, sonst Fehler!)")]
    [SerializeField] private GameObject nodeTemplate;
    [SerializeField] private AudioClip destroyedSound;
    [SerializeField] private AudioClip reviveSound;

    public bool IsAlive => current != null;
    public System.Action<KernelPowerNode> OnDestroyed;

    private GameObject current;

    private void Awake()
    {
        if (nodeTemplate != null) nodeTemplate.SetActive(false);
    }

    public void Revive()
    {
        if (current != null || nodeTemplate == null) return;

        current = Instantiate(nodeTemplate, nodeTemplate.transform.position, nodeTemplate.transform.rotation, transform);
        current.SetActive(true);

        EnemyLife life = current.GetComponent<EnemyLife>();
        if (life != null) life.OnDeath += HandleDeath;

        if (reviveSound != null) EnemySound.GetSource(this).PlayOneShot(reviveSound, 0.6f);
    }

    public void Shutdown()
    {
        if (current != null) Destroy(current);
        current = null;
    }

    private void HandleDeath()
    {
        current = null;
        if (destroyedSound != null) EnemySound.GetSource(this).PlayOneShot(destroyedSound, 0.8f);
        OnDestroyed?.Invoke(this);
    }
}