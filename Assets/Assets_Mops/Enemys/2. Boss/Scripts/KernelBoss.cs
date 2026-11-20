using System.Collections;
using UnityEngine;

/// <summary>
/// Boss Stockwerk 2: KERNEL, ein stationaerer Server-Kern.
/// Schild an, solange einer der Strom-Knoten lebt. Alle Knoten zerstoert -> Kern liegt kurz frei.
/// Angriffe: Laser-Sweep, Daten-Salve (Kugelringe), Hack-Puls. Ab 50 % Phase 2 mit Gegnern und zwei Lasern.
/// Braucht BossLife auf demselben Objekt (Leben, Healthbar, Loot, Exit, Spielstand).
/// </summary>
[RequireComponent(typeof(BossLife))]
public class KernelBoss : MonoBehaviour
{
    [Header("Aussehen")]
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite chargeSprite;
    [SerializeField] private Sprite exposedSprite;
    [Tooltip("Kind-Objekt mit kernel_shield (nur Optik)")]
    [SerializeField] private GameObject shieldVisual;

    [Header("Strom-Knoten")]
    [SerializeField] private KernelPowerNode[] nodes;
    [Tooltip("So lange liegt der Kern frei, wenn alle Knoten zerstoert sind")]
    [SerializeField] private float exposedTime = 5f;

    [Header("Laser (Phase 1 nutzt den ersten, Phase 2 zwei)")]
    [SerializeField] private KernelLaser[] lasers;
    [SerializeField] private float laserWarnTime = 1f;
    [SerializeField] private float laserSweepAngle = 220f;
    [SerializeField] private float laserSpeed = 55f;
    [SerializeField] private float laserSpeedPhase2 = 80f;

    [Header("Daten-Salve")]
    [Tooltip("Kopie vom BossBullet mit kernel_bullet: langsamer, lebt laenger")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("Mitte des Auges: hier starten Kugeln und Laser")]
    [SerializeField] private Transform eye;
    [SerializeField] private int bulletsPerRing = 16;
    [SerializeField] private int ringsPhase1 = 2;
    [SerializeField] private int ringsPhase2 = 3;
    [SerializeField] private float timeBetweenRings = 0.45f;

    [Header("Hack-Puls")]
    [SerializeField] private float hackChargeTime = 1f;
    [SerializeField] private float hackDuration = 3f;

    [Header("Phase 2: Gegner aus den Boden-Luken")]
    [SerializeField] private GameObject[] phase2Enemies;
    [SerializeField] private Transform[] phase2SpawnPoints;
    [Tooltip("Spaetestens nach so vielen Sekunden geht es weiter, auch wenn noch Gegner leben")]
    [SerializeField] private float maxAddWait = 25f;

    [Header("Pausen zwischen den Angriffen")]
    [SerializeField] private float pausePhase1 = 1.2f;
    [SerializeField] private float pausePhase2 = 0.6f;

    [Header("Ryan-Anruf vor dem Kampf (optional)")]
    [SerializeField] private TutorialManager phoneCall;
    [SerializeField]
    private TutorialManager.CallLine[] introLines =
    {
        new TutorialManager.CallLine { fromPlayer = false, text = "Choom, that thing in the middle is KERNEL. This whole floor runs through it." },
        new TutorialManager.CallLine { fromPlayer = false, text = "Its shield feeds on those four power nodes. Shoot them out and the core is naked for a few seconds." },
        new TutorialManager.CallLine { fromPlayer = true,  text = "And then?" },
        new TutorialManager.CallLine { fromPlayer = false, text = "Then you unload everything you've got. And watch the red lines, that's where the laser goes." }
    };

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip laserChargeSound;
    [SerializeField] private AudioClip laserSound;
    [SerializeField] private AudioClip salvoSound;
    [SerializeField] private AudioClip hackSound;
    [SerializeField] private AudioClip shieldDownSound;
    [SerializeField] private AudioClip shieldUpSound;
    [SerializeField] private AudioClip phase2Sound;

    private BossLife life;
    private Transform player;
    private Coroutine attackLoop;
    private bool active;
    private bool exposed;
    private bool inTransition;
    private int cycle;

    private void Awake()
    {
        life = GetComponent<BossLife>();
        life.OnPhaseTransitionStart += StartPhaseTransition;
        life.OnDeath += HandleDeath;

        foreach (KernelPowerNode node in nodes)
            if (node != null) node.OnDestroyed += _ => CheckNodes();

        SetShield(true);
        SetCore(idleSprite);
        HideLasers();
    }

    /// <summary>Wird vom KernelRoomTrigger aufgerufen.</summary>
    public void ActivateBoss()
    {
        if (active) return;
        active = true;
        StartCoroutine(StartFight());
    }

    private IEnumerator StartFight()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (phoneCall != null && introLines != null && introLines.Length > 0)
            yield return phoneCall.PlayPart(introLines, true, true, true);

        life.ShowHealthBar();
        ReviveNodes();
        attackLoop = StartCoroutine(AttackLoop());
    }

    // ── Angriffs-Schleife ─────────────────────────────────

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return LaserSweep();
            yield return Pause();

            yield return DataSalvo();
            yield return Pause();

            cycle++;
            if (cycle % 2 == 0)
            {
                yield return HackPulse();
                yield return Pause();
            }
        }
    }

    private IEnumerator Pause()
    {
        yield return new WaitForSeconds(life.CurrentPhase == 2 ? pausePhase2 : pausePhase1);
    }

    private IEnumerator LaserSweep()
    {
        int count = Mathf.Min(lasers.Length, life.CurrentPhase == 2 ? 2 : 1);
        if (count == 0 || player == null) yield break;

        // Startet seitlich vom Spieler und dreht ueber ihn hinweg
        float sign = Random.value < 0.5f ? 1f : -1f;
        Vector2 toPlayer = player.position - EyePos();
        float start = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - sign * 70f;

        for (int i = 0; i < count; i++)
            if (lasers[i] != null) lasers[i].ShowWarning(start + i * 180f);

        SetCore(chargeSprite);
        Play(laserChargeSound);
        yield return new WaitForSeconds(laserWarnTime);

        for (int i = 0; i < count; i++)
            if (lasers[i] != null) lasers[i].Fire();
        Play(laserSound);

        float speed = life.CurrentPhase == 2 ? laserSpeedPhase2 : laserSpeed;
        float swept = 0f;
        while (swept < laserSweepAngle)
        {
            swept += speed * Time.deltaTime;
            for (int i = 0; i < count; i++)
                if (lasers[i] != null) lasers[i].SetAngle(start + i * 180f + sign * swept);
            yield return null;
        }

        HideLasers();
        SetCore(idleSprite);
    }

    private IEnumerator DataSalvo()
    {
        if (bulletPrefab == null) yield break;

        int rings = life.CurrentPhase == 2 ? ringsPhase2 : ringsPhase1;
        for (int r = 0; r < rings; r++)
        {
            float offset = (r % 2) * (180f / bulletsPerRing);   // jeder 2. Ring versetzt -> Luecken wechseln
            for (int i = 0; i < bulletsPerRing; i++)
            {
                float a = (offset + i * 360f / bulletsPerRing) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                GameObject shot = Instantiate(bulletPrefab, EyePos(), Quaternion.identity);
                shot.GetComponent<GunshotEffect>()?.Init(dir);
            }
            Play(salvoSound);
            yield return new WaitForSeconds(timeBetweenRings);
        }
    }

    private IEnumerator HackPulse()
    {
        SetCore(chargeSprite);
        yield return new WaitForSeconds(hackChargeTime);

        if (player != null)
        {
            PlayerControlHack hack = player.GetComponentInChildren<PlayerControlHack>();
            if (hack != null) hack.ApplyHack(Random.value < 0.5f, hackDuration, this);
        }
        Play(hackSound);
        SetCore(idleSprite);
    }

    // ── Schild / Knoten ───────────────────────────────────

    private void CheckNodes()
    {
        if (!active || exposed || inTransition) return;
        foreach (KernelPowerNode node in nodes)
            if (node != null && node.IsAlive) return;

        StartCoroutine(ExposeCore());
    }

    private IEnumerator ExposeCore()
    {
        exposed = true;
        StopAttacks();

        SetShield(false);
        SetCore(exposedSprite);
        Play(shieldDownSound);

        yield return new WaitForSeconds(exposedTime);

        exposed = false;
        SetShield(true);
        SetCore(idleSprite);
        Play(shieldUpSound);
        ReviveNodes();
        attackLoop = StartCoroutine(AttackLoop());
    }

    private void ReviveNodes()
    {
        foreach (KernelPowerNode node in nodes)
            if (node != null) node.Revive();
    }

    private void StopAttacks()
    {
        if (attackLoop != null) StopCoroutine(attackLoop);
        attackLoop = null;
        HideLasers();
        EnemySound.GetSource(this).Stop();
    }

    // ── Phase 2 ───────────────────────────────────────────

    private void StartPhaseTransition()
    {
        StopAllCoroutines();   // auch das Freiliegen beenden
        attackLoop = null;
        StartCoroutine(PhaseTransition());
    }

    private IEnumerator PhaseTransition()
    {
        inTransition = true;
        exposed = false;
        HideLasers();
        SetShield(true);
        SetCore(chargeSprite);
        Play(phase2Sound != null ? phase2Sound : shieldUpSound);

        int alive = 0;
        if (phase2Enemies != null && phase2SpawnPoints != null && phase2SpawnPoints.Length > 0)
        {
            for (int i = 0; i < phase2Enemies.Length; i++)
            {
                if (phase2Enemies[i] == null) continue;
                Transform point = phase2SpawnPoints[i % phase2SpawnPoints.Length];
                GameObject enemy = Instantiate(phase2Enemies[i], point.position, Quaternion.identity);
                EnemyLife el = enemy.GetComponent<EnemyLife>();
                if (el != null) { alive++; el.OnDeath += () => alive--; }
            }
        }

        float t = 0f;
        while (alive > 0 && t < maxAddWait)
        {
            t += Time.deltaTime;
            yield return null;
        }

        inTransition = false;
        life.EnterPhase2();
        SetCore(idleSprite);
        ReviveNodes();
        attackLoop = StartCoroutine(AttackLoop());
    }

    // ── Tod / Hilfen ──────────────────────────────────────

    private void HandleDeath()
    {
        StopAllCoroutines();
        HideLasers();
        foreach (KernelPowerNode node in nodes)
            if (node != null) node.Shutdown();
    }

    private void SetShield(bool on)
    {
        if (life != null) life.externalShield = on;
        if (shieldVisual != null) shieldVisual.SetActive(on);
    }

    private void SetCore(Sprite sprite)
    {
        if (coreRenderer != null && sprite != null) coreRenderer.sprite = sprite;
    }

    private void HideLasers()
    {
        if (lasers == null) return;
        foreach (KernelLaser laser in lasers)
            if (laser != null) laser.Hide();
    }

    private Vector3 EyePos() => eye != null ? eye.position : transform.position;

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        AudioSource src = EnemySound.GetSource(this);
        src.pitch = 1f;
        src.PlayOneShot(clip, 0.8f);
    }
}