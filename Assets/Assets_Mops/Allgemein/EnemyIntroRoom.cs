using UnityEngine;
using System.Collections;

/// <summary>
/// Gegner-Tutorial im Fight Room. Haengt am SpawnPoint eines EnemyIntroduction-Raums (statt FightRoomTrigger).
/// Ablauf: Spieler kommt an -> Ryan erklaert den Gegner (Spieler gesperrt, Vorfuehr-Gegner unverwundbar)
/// -> Vorfuehr-Gegner steht als Zielscheibe da -> abgeschossen -> Tutorial-Welle startet.
/// Alle drei Raeume sind EIN Gespraech: Melee klingelt, Dash geht weiter, nach Robot wird aufgelegt.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyIntroRoom : MonoBehaviour
{
    public enum IntroKind { Melee, Dash, Robot, Hacker }

    [Header("Raum")]
    [SerializeField] private IntroKind kind = IntroKind.Melee;
    [SerializeField] private EnemySpawn enemySpawn;
    [Tooltip("Gegner im Raum, den Ryan vorstellt (Prefab-Instanz reinziehen). Steht still, bis man ihn abschiesst.")]
    [SerializeField] private GameObject showcaseEnemy;

    [Header("Gespraech")]
    [SerializeField] private TutorialManager phoneCall;
    [Tooltip("Leer lassen = Standard-Dialog fuer diesen Gegner")]
    [SerializeField] private TutorialManager.CallLine[] customLines;

    [Tooltip("Ryans Aufnahmen in der Reihenfolge SEINER Zeilen (Spieler-Zeilen ueberspringen). Leeres Feld = diese Zeile ohne Ton")]
    [SerializeField] private AudioClip[] ryanVoice;

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
        // 1. Ryan erklaert (Spieler bleibt die ganze Zeit gesperrt)
        TutorialManager.CallLine[] part = GetLines();
        AttachVoices(part);
        if (phoneCall != null && part.Length > 0)
        {
            // Hacker = eigener Anruf (klingelt + legt auf), unabhaengig vom Gespraech im 1. Stock
            bool standalone = kind == IntroKind.Hacker;
            bool ring = standalone || kind == IntroKind.Melee || !callStarted;
            bool hangUp = standalone || kind == IntroKind.Robot;
            if (!standalone)
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
                    Ryan("Choom, you're on the testing floor. Corpo security trains here."),
                    You("Trains? Trains with what?"),
                    Ryan("See that tin can? Melee Robot. Dumb, but it hits like a truck."),
                    Ryan("It walks right at you and swings once it's close."),
                    You("So I don't let it get close."),
                    Ryan("Preem. It's slower than you. Back off and keep shooting."),
                    Ryan("Go on, scrap it. Then the real drill starts.")
                };

            case IntroKind.Dash:
                return new[]
                {
                    Ryan("Nice work. Next one's nastier. Dash Robot."),
                    You("Let me guess. It dashes."),
                    Ryan("Straight at you, faster than you can blink. Then it waits a bit."),
                    Ryan("Don't stand still. Move and hit SPACE to dash out of the way."),
                    You("Dodge first, shoot after. Got it."),
                    Ryan("Hit it while it's recharging. Show it who's boss.")
                };

            case IntroKind.Hacker:
                return new[]
                {
                    Ryan("Choom, new floor, new problem. See the one hunched over that deck?"),
                    You("Another robot?"),
                    Ryan("Hacker unit. It doesn't fight you. It fights your controls."),
                    Ryan("When it sits down and starts typing, you've got two seconds."),
                    Ryan("Hit it while it types and the hack fails."),
                    Ryan("Too slow and W/S or A/D get swapped for a few seconds."),
                    You("So my own legs stop listening to me."),
                    Ryan("Just for a bit. It also runs when you get close. Corner it."),
                    Ryan("Scrap this one, then deal with the rest. Ryan out.")
                };



            default:
                return new[]
                {
                    Ryan("Last one. Standard security bot. This one shoots back."),
                    You("Of course it does."),
                    Ryan("It fires every couple seconds, but its shots stop at walls."),
                    Ryan("Use cover, peek out, return fire. Never stand in the open."),
                    You("And after that?"),
                    Ryan("After that you're ready for the real fight. Don't flatline, rookie."),
                    Ryan("Ryan out.")
                };
        }
    }

        // Haengt die Aufnahmen der Reihe nach an Ryans Zeilen
    private void AttachVoices(TutorialManager.CallLine[] part)
    {
        if (ryanVoice == null || ryanVoice.Length == 0) return;

        int v = 0;
        foreach (TutorialManager.CallLine line in part)
        {
            if (line == null || line.fromPlayer) continue;
            if (v >= ryanVoice.Length) break;

            if (line.voice == null)
                line.voice = ryanVoice[v];
            v++;
        }
    }

    private static TutorialManager.CallLine Ryan(string text) =>
        new TutorialManager.CallLine { fromPlayer = false, text = text };

    private static TutorialManager.CallLine You(string text) =>
        new TutorialManager.CallLine { fromPlayer = true, text = text };
}