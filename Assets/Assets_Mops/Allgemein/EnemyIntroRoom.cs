using UnityEngine;
using System.Collections;

/// <summary>
/// Gegner-Tutorial im Fight Room. Haengt am SpawnPoint eines EnemyIntroduction-Raums (statt FightRoomTrigger).
/// Ablauf: Spieler kommt an -> Rayn erklaert den Gegner (Spieler gesperrt, Vorfuehr-Gegner unverwundbar)
/// -> Vorfuehr-Gegner steht als Zielscheibe da -> abgeschossen -> Tutorial-Welle startet.
/// Alle drei Raeume sind EIN Gespraech: Melee klingelt, Dash geht weiter, nach Robot wird aufgelegt.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyIntroRoom : MonoBehaviour
{
    public enum IntroKind { Melee, Dash, Robot }

    [Header("Raum")]
    [SerializeField] private IntroKind kind = IntroKind.Melee;
    [SerializeField] private EnemySpawn enemySpawn;
    [Tooltip("Gegner im Raum, den Rayn vorstellt (Prefab-Instanz reinziehen). Steht still, bis man ihn abschiesst.")]
    [SerializeField] private GameObject showcaseEnemy;

    [Header("Gespraech")]
    [SerializeField] private TutorialManager phoneCall;
    [Tooltip("Leer lassen = Standard-Dialog fuer diesen Gegner")]
    [SerializeField] private TutorialManager.CallLine[] customLines;

    // Gespraech laeuft schon (ueberlebt das Neuladen der Szene zwischen den Raeumen)
    private static bool callStarted;

    private bool triggered;

    private void Start()
    {
        // Laut Spielstand schon geschafft -> kein Dialog, kein Vorfuehr-Gegner
        if (enemySpawn != null && SaveSystem.IsCleared(SaveSystem.GetObjectId(enemySpawn.gameObject)))
        {
            triggered = true;
            if (showcaseEnemy != null) showcaseEnemy.SetActive(false);
            return;
        }

        PrepareShowcase();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // 1. Rayn erklaert (Spieler bleibt die ganze Zeit gesperrt)
        TutorialManager.CallLine[] part = GetLines();
        if (phoneCall != null && part.Length > 0)
        {
            bool ring = kind == IntroKind.Melee || !callStarted;
            bool hangUp = kind == IntroKind.Robot;
            callStarted = !hangUp;

            yield return phoneCall.PlayPart(part, ring, hangUp, true);
        }

        // 2. Zielscheibe freigeben und warten, bis sie abgeschossen ist
        if (showcaseEnemy != null)
        {
            SetShowcaseHittable(true);

            while (showcaseEnemy != null)   // wird beim Tod zerstoert
                yield return null;
        }

        // 3. Tutorial-Welle starten
        if (enemySpawn != null)
            enemySpawn.StartRoom();
    }

    // ── Vorfuehr-Gegner ───────────────────────────────────

    private void PrepareShowcase()
    {
        if (showcaseEnemy == null) return;

        // KI aus (laufen, dashen, schiessen, hacken ...), nur Leben + Animation bleiben
        foreach (MonoBehaviour mb in showcaseEnemy.GetComponents<MonoBehaviour>())
            if (!(mb is EnemyLife) && !(mb is EnemyAnimator))
                mb.enabled = false;

        // Waehrend der Erklaerung unverwundbar
        SetShowcaseHittable(false);
    }

    private void SetShowcaseHittable(bool hittable)
    {
        foreach (Collider2D col in showcaseEnemy.GetComponentsInChildren<Collider2D>(true))
            col.enabled = hittable;
    }

    // ── Dialog ────────────────────────────────────────────

    private TutorialManager.CallLine[] GetLines()
    {
        if (customLines != null && customLines.Length > 0)
            return customLines;

                switch (kind)
        {
            case IntroKind.Melee:
                return new[]
                {
                    Rayn("Choom, you're on the testing floor. Corpo security trains here."),
                    You("Trains? Trains with what?"),
                    Rayn("See that tin can? Melee Robot. Dumb, but it hits like a truck."),
                    Rayn("It walks right at you and swings once it's close."),
                    You("So I don't let it get close."),
                    Rayn("Preem. It's slower than you. Back off and keep shooting."),
                    Rayn("Go on, scrap it. Then the real drill starts.")
                };

            case IntroKind.Dash:
                return new[]
                {
                    Rayn("Nice work. Next one's nastier. Dash Robot."),
                    You("Let me guess. It dashes."),
                    Rayn("Straight at you, faster than you can blink. Then it waits a bit."),
                    Rayn("Don't stand still. Move and hit SPACE to dash out of the way."),
                    You("Dodge first, shoot after. Got it."),
                    Rayn("Hit it while it's recharging. Show it who's boss.")
                };

            default:
                return new[]
                {
                    Rayn("Last one. Standard security bot. This one shoots back."),
                    You("Of course it does."),
                    Rayn("It fires every couple seconds, but its shots stop at walls."),
                    Rayn("Use cover, peek out, return fire. Never stand in the open."),
                    You("And after that?"),
                    Rayn("After that you're ready for the real fight. Don't flatline, rookie."),
                    Rayn("Rayn out.")
                };
        }
    }

    private static TutorialManager.CallLine Rayn(string text) =>
        new TutorialManager.CallLine { fromPlayer = false, text = text };

    private static TutorialManager.CallLine You(string text) =>
        new TutorialManager.CallLine { fromPlayer = true, text = text };
}