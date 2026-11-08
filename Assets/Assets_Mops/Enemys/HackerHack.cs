using UnityEngine;
using System.Collections;

/// <summary>
/// Hacker: setzt sich hin, tippt (Warnphase) und vertauscht danach eine Bewegungsachse des Spielers.
/// Ein Treffer waehrend Hinsetzen/Tippen bricht den Hack ab. Stirbt er, ist die Steuerung sofort wieder normal.
/// </summary>
[RequireComponent(typeof(EnemyLife))]
public class HackerHack : MonoBehaviour
{
    [Header("Wann hackt er?")]
    [Tooltip("Maximale Entfernung zum Spieler fuer einen Hack (geht auch durch Waende)")]
    public float hackRange = 10f;
    [Tooltip("Wartezeit nach dem Spawnen bis zum ersten Hack (zufaellig zwischen x und y)")]
    public Vector2 firstHackDelay = new Vector2(2f, 4f);
    [Tooltip("Pause zwischen zwei Hacks (zufaellig zwischen x und y)")]
    public Vector2 hackCooldown = new Vector2(6f, 8f);

    [Header("Ablauf (passend zu den Animationen)")]
    [Tooltip("Dauer der Hinsetz-Animation (6 Frames bei 10 FPS = 0.6)")]
    public float sitDownTime = 0.6f;
    [Tooltip("So lange tippt er = Warnphase. In dieser Zeit kann man ihn unterbrechen")]
    public float typingTime = 2f;
    [Tooltip("Dauer der Aufsteh-Animation")]
    public float standUpTime = 0.6f;

    [Header("Wirkung auf den Spieler")]
    [Tooltip("So lange ist eine Achse vertauscht")]
    public float hackDuration = 4f;
    [Tooltip("Treffer waehrend Hinsetzen/Tippen bricht den Hack ab")]
    public bool hitInterrupts = true;

    [Header("Referenzen")]
    [Tooltip("Warnsymbol ueber dem Kopf (Kind-Objekt), blinkt beim Tippen")]
    public GameObject warningIcon;
    public Animator animator;
    [SerializeField] private string hackingBool = "IsHacking";

    private Transform player;
    private PlayerControlHack playerHack;
    private HackerMovement movement;
    private EnemyLife life;

    private float cooldownTimer;
    private bool isHacking;
    private bool interrupted;

    void Start()
    {
        movement = GetComponent<HackerMovement>();
        life = GetComponent<EnemyLife>();
        if (animator == null) animator = GetComponent<Animator>();

        life.OnDamaged += OnHit;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHack = p.GetComponent<PlayerControlHack>();
        }
        if (playerHack == null)
            Debug.LogWarning("HackerHack: Der Spieler hat kein PlayerControlHack, Hacks wirken nicht.");

        cooldownTimer = Random.Range(firstHackDelay.x, firstHackDelay.y);
        if (warningIcon != null) warningIcon.SetActive(false);
    }

    void OnDestroy()
    {
        if (life != null) life.OnDamaged -= OnHit;

        // Hacker tot -> Steuerung sofort wieder normal
        if (playerHack != null) playerHack.ClearHackFrom(this);
    }

    void Update()
    {
        if (isHacking || player == null || playerHack == null) return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (CanStartHack())
            StartCoroutine(HackRoutine());
    }

    bool CanStartHack()
    {
        if (Vector2.Distance(transform.position, player.position) > hackRange) return false;
        if (movement != null && movement.IsTooClose) return false;   // erst weglaufen
        if (playerHack.IsHacked) return false;                      // mehrere Hacker stapeln sich nicht
        return true;
    }

    IEnumerator HackRoutine()
    {
        isHacking = true;
        interrupted = false;

        if (movement != null)
        {
            movement.FaceDirection(player.position - transform.position);
            movement.enabled = false;
        }
        SetHackingAnim(true);

        // 1. hinsetzen
        float t = 0f;
        while (t < sitDownTime && !interrupted)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 2. tippen = Warnphase, Symbol blinkt immer schneller
        t = 0f;
        while (t < typingTime && !interrupted)
        {
            t += Time.deltaTime;
            if (warningIcon != null)
            {
                float blinkRate = Mathf.Lerp(3f, 10f, t / typingTime);
                warningIcon.SetActive(Mathf.Repeat(t * blinkRate, 1f) < 0.6f);
            }
            yield return null;
        }
        if (warningIcon != null) warningIcon.SetActive(false);

        // 3. nicht unterbrochen -> zufaellig W/S oder A/D vertauschen
        if (!interrupted && playerHack != null)
            playerHack.ApplyHack(Random.value < 0.5f, hackDuration, this);

        // 4. aufstehen
        SetHackingAnim(false);
        yield return new WaitForSeconds(standUpTime);

        if (movement != null) movement.enabled = true;
        cooldownTimer = Random.Range(hackCooldown.x, hackCooldown.y);
        isHacking = false;
    }

    void OnHit(int damage)
    {
        if (isHacking && hitInterrupts)
            interrupted = true;
    }

    void SetHackingAnim(bool value)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool(hackingBool, value);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, hackRange);
    }
}