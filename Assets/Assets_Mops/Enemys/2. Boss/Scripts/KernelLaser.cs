using UnityEngine;

/// <summary>
/// Ein Laser des KERNEL: erst rote Warnlinie, dann Strahl. Gedreht wird er vom KernelBoss.
/// Waende und Saeulen (obstacleLayer) stoppen den Strahl, dahinter ist man sicher.
/// Haengt an einem leeren Objekt in der Augen-Mitte, Warnlinie und Strahl sind dessen Kinder (Pivot links).
/// </summary>
public class KernelLaser : MonoBehaviour
{
    [Header("Grafik (Kind-Objekte, Sprites mit Pivot links)")]
    [SerializeField] private SpriteRenderer warningRenderer;
    [SerializeField] private SpriteRenderer beamRenderer;

    [Header("Strahl")]
    [SerializeField] private float maxLength = 20f;
    [Tooltip("Halbe Breite des Strahls fuer den Treffer-Test")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private int damagePerTick = 12;
    [SerializeField] private float tickInterval = 0.35f;
    [Tooltip("Waende und Saeulen (Layer Tilemap), die den Strahl stoppen")]
    [SerializeField] private LayerMask obstacleLayer;

    private bool warning;
    private bool firing;
    private float tickTimer;

    private void Awake() => Hide();

    public void SetAngle(float degrees)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, degrees);
        UpdateLength();
    }

    public void ShowWarning(float degrees)
    {
        warning = true;
        firing = false;
        if (warningRenderer != null) warningRenderer.enabled = true;
        if (beamRenderer != null) beamRenderer.enabled = false;
        SetAngle(degrees);
    }

    public void Fire()
    {
        warning = false;
        firing = true;
        tickTimer = 0f;
        if (warningRenderer != null) warningRenderer.enabled = false;
        if (beamRenderer != null) beamRenderer.enabled = true;
        UpdateLength();
    }

    public void Hide()
    {
        warning = false;
        firing = false;
        if (warningRenderer != null) warningRenderer.enabled = false;
        if (beamRenderer != null) beamRenderer.enabled = false;
    }

    private void Update()
    {
        if (!warning && !firing) return;
        UpdateLength();
        if (!firing) return;

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;

        foreach (RaycastHit2D hit in Physics2D.CircleCastAll(transform.position, hitRadius, transform.right, CurrentLength()))
        {
            PlayerLife player = hit.collider.GetComponentInParent<PlayerLife>();
            if (player == null) continue;

            player.TakeDamage(damagePerTick);
            tickTimer = tickInterval;
            break;
        }
    }

    private float CurrentLength()
    {
        if (obstacleLayer.value == 0) return maxLength;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, maxLength, obstacleLayer);
        return hit.collider != null ? hit.distance : maxLength;
    }

    private void UpdateLength()
    {
        float length = CurrentLength();
        Stretch(warningRenderer, length);
        Stretch(beamRenderer, length);
    }

    private static void Stretch(SpriteRenderer r, float length)
    {
        if (r == null || r.sprite == null) return;
        Vector3 s = r.transform.localScale;
        s.x = length / r.sprite.bounds.size.x;
        r.transform.localScale = s;
    }
}