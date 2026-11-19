using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  

/// <summary>
/// Station 3 – Kabel kappen.
/// 5 bunte Kabel, genau eins ist richtig. Welches, sagen die Regeln im Handbuch (PuzzleClue im Raum).
/// Zum Schneiden muss das Werkzeug (Schere) in der Main Hand sein.
/// Falscher Schnitt = Alarm: Gegner spawnen, die Kabel werden neu gemischt.
/// </summary>
public class WireCutStation : PuzzleStation
{
    [Header("Kabelschacht")]
    [Tooltip("Werkzeug in der Main Hand. Leer = kein Werkzeug noetig")]
    [SerializeField] private string requiredItemName = "Scissors";

    [Header("Alarm bei falschem Schnitt")]
    [Tooltip("Gegner-Prefabs, die bei einem Fehler spawnen (z.B. 2x Robot)")]
    [SerializeField] private GameObject[] alarmEnemies;
    [Tooltip("Spawnpunkte im Raum. Die Gegner werden der Reihe nach verteilt")]
    [SerializeField] private Transform[] alarmSpawnPoints;
    [Tooltip("Nach so vielen Alarmen spawnen keine Gegner mehr")]
    [SerializeField] private int maxAlarms = 3;

    private enum WireColor { Red, Blue, Yellow, White, Black }

    private static readonly Color32[] WireColors =
    {
        new Color32(230, 50, 60, 255),    // Rot
        new Color32(50, 110, 255, 255),   // Blau
        new Color32(245, 205, 40, 255),   // Gelb
        new Color32(235, 238, 245, 255),  // Weiss
        new Color32(30, 30, 38, 255)      // Schwarz
    };

    private const int WireCount = 5;
    private const string Manual =
        "WIRE PANEL - SAFETY MANUAL\n" +
        "Count wires from top to bottom.\n" +
        "Check the rules in this order:\n\n" +
        "1. No yellow wire?\n    Cut the 2nd wire.\n" +
        "2. More than one red wire?\n    Cut the last red wire.\n" +
        "3. Is the last wire white?\n    Cut the 1st wire.\n" +
        "4. Exactly one blue wire?\n    Cut the blue wire.\n" +
        "5. Otherwise cut the 4th wire.";

    private WireColor[] wires;
    private int correctWire;
    private int alarms;
    private bool busy;

    private RectTransform[] wireRects;
    private Image[] leftHalf, rightHalf;

    private void Reset()
    {
        stationTitle = "MAINTENANCE SHAFT // DOOR POWER";
        promptText = "Press F to open the panel";
    }

    protected override void Awake()
    {
        base.Awake();
        RollWires();
    }

    /// <summary>Das Handbuch fuer den Zettel im Raum (Index egal).</summary>
    public override string GetClue(int index) => Manual;

    // ── Oeffnen ───────────────────────────────────────────

    protected override void OnOpen()
    {
        busy = false;
        BuildPanel();

        if (HasTool())
                        SetStatus("Follow the manual. Cut ONE wire - wrong cut = ALARM!", ColMuted);
        else
            SetStatus("You need " + requiredItemName.ToUpper() + " in your main hand.", ColYellow);
    }

    protected override void UpdatePuzzle()
    {
        if (busy || !Input.GetMouseButtonDown(0)) return;

        for (int i = 0; i < WireCount; i++)
        {
            if (!MouseOver(wireRects[i])) continue;

            if (!HasTool())
            {
                Play(errorClip);
                SetStatus("You need " + requiredItemName.ToUpper() + " in your main hand.", ColRed);
                return;
            }

            StartCoroutine(Cut(i));
            return;
        }
    }

    private IEnumerator Cut(int i)
    {
        busy = true;
        Play(clickClip);

        // Kabel auseinander
        leftHalf[i].rectTransform.anchoredPosition += new Vector2(-18, -6);
        rightHalf[i].rectTransform.anchoredPosition += new Vector2(18, 6);

        if (i == correctWire)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            Solve();
            yield break;
        }

        // Falsch -> Alarm
        Play(errorClip);
        FlashBackdrop(ColRed);
        SetStatus("WRONG WIRE - ALARM TRIGGERED!", ColRed);
        TriggerAlarm();
        RollWires();   // naechster Versuch bekommt neue Kabel

        yield return new WaitForSecondsRealtime(1.2f);
        Close();
    }

    private void TriggerAlarm()
    {
        if (alarms >= maxAlarms || alarmEnemies == null || alarmSpawnPoints == null || alarmSpawnPoints.Length == 0)
            return;

        alarms++;
        for (int k = 0; k < alarmEnemies.Length; k++)
        {
            if (alarmEnemies[k] == null) continue;
            Transform point = alarmSpawnPoints[k % alarmSpawnPoints.Length];
            if (point != null)
                Instantiate(alarmEnemies[k], point.position, Quaternion.identity);
        }
    }

    // ── Regeln ────────────────────────────────────────────

    private void RollWires()
    {
        wires = new WireColor[WireCount];
        for (int i = 0; i < WireCount; i++)
            wires[i] = (WireColor)Random.Range(0, 5);

        correctWire = SolveRules(wires);
    }

    private static int SolveRules(WireColor[] w)
    {
        int last = w.Length - 1;

        if (Count(w, WireColor.Yellow) == 0) return 1;                          // 1. kein Gelb -> 2. Kabel
        if (Count(w, WireColor.Red) > 1) return LastIndex(w, WireColor.Red);    // 2. mehr als ein Rot -> letztes Rot
        if (w[last] == WireColor.White) return 0;                               // 3. letztes Weiss -> 1. Kabel
        if (Count(w, WireColor.Blue) == 1) return LastIndex(w, WireColor.Blue); // 4. genau ein Blau -> das Blaue
        return 3;                                                               // 5. sonst -> 4. Kabel
    }

    private static int Count(WireColor[] w, WireColor c)
    {
        int n = 0;
        foreach (WireColor x in w) if (x == c) n++;
        return n;
    }

    private static int LastIndex(WireColor[] w, WireColor c)
    {
        for (int i = w.Length - 1; i >= 0; i--) if (w[i] == c) return i;
        return -1;
    }

    // ── Werkzeug ──────────────────────────────────────────

    private bool HasTool()
    {
        if (string.IsNullOrEmpty(requiredItemName)) return true;

        GameObject canvas = GameObject.Find("InventoryCanvas");
        InventoryManager inv = canvas != null ? canvas.GetComponent<InventoryManager>() : null;
        if (inv == null || inv.equippedSlot == null) return false;

        foreach (EquippedSlot slot in inv.equippedSlot)
            if (slot != null && slot.GetItemType() == ItemType.mainHand && slot.GetItemName() == requiredItemName)
                return true;

        return false;
    }

    // ── Anzeige ───────────────────────────────────────────

        private void BuildPanel()
    {
        wireRects = new RectTransform[WireCount];
        leftHalf = new Image[WireCount];
        rightHalf = new Image[WireCount];

        // ── Links: die Kabel ──
        const float cx = -215f;    // Mitte der Kabel
        const float half = 240f;   // halbe Kabellaenge

        NewImage("Backplate", content, ColPanelDark, new Vector2(620, 520), new Vector2(cx, 0));
        NewImage("TerminalLeft", content, ColLine, new Vector2(36, 480), new Vector2(cx - half - 20, 0));
        NewImage("TerminalRight", content, ColLine, new Vector2(36, 480), new Vector2(cx + half + 20, 0));

        float step = 90f;
        float top = (WireCount - 1) * step / 2f;

        for (int i = 0; i < WireCount; i++)
        {
            float y = top - i * step;
            Color32 col = WireColors[(int)wires[i]];

            // Klickflaeche ueber die ganze Kabellaenge
            RectTransform hit = NewRect("Wire " + (i + 1), content, new Vector2(half * 2f, 60), new Vector2(cx, y));
            wireRects[i] = hit;

            // Schwarzes Kabel bekommt einen hellen Rand, damit man es sieht
            if (wires[i] == WireColor.Black)
                NewImage("Outline", content, ColOff, new Vector2(half * 2f + 4, 30), new Vector2(cx, y));

            leftHalf[i] = NewImage("Left", content, col, new Vector2(half, 22), new Vector2(cx - half / 2f, y));
            rightHalf[i] = NewImage("Right", content, col, new Vector2(half, 22), new Vector2(cx + half / 2f, y));

            NewImage("ScrewL", content, ColMuted, new Vector2(26, 26), new Vector2(cx - half - 10, y));
            NewImage("ScrewR", content, ColMuted, new Vector2(26, 26), new Vector2(cx + half + 10, y));
            NewText("Nr", content, (i + 1).ToString(), 26, ColMuted, new Vector2(40, 40), new Vector2(cx - half - 70, y));
        }

        // ── Rechts: das Handbuch ──
        Vector2 m = new Vector2(315, 0);
        NewImage("ManualFrame", content, ColYellow, new Vector2(424, 524), m);
        NewImage("ManualPaper", content, ColPanelDark, new Vector2(416, 516), m);
        NewImage("ManualHeader", content, ColYellow, new Vector2(416, 52), m + new Vector2(0, 232));
        NewText("ManualTitle", content, "SAFETY MANUAL", 30, ColPanelDark, new Vector2(400, 50), m + new Vector2(0, 232));
        NewText("ManualText", content, ColorizeManual(), 22, ColText, new Vector2(380, 440), m + new Vector2(0, -26), TextAlignmentOptions.TopLeft);
    }

    /// <summary>Handbuch ohne erste Zeile (steht schon im Kopf), Farbwoerter in ihrer Kabelfarbe.</summary>
    private static string ColorizeManual()
    {
        string text = Manual.Substring(Manual.IndexOf('\n') + 1);
        string[] names = { "red", "blue", "yellow", "white", "black" };   // gleiche Reihenfolge wie WireColor

        for (int c = 0; c < names.Length; c++)
        {
            string hex = ColorUtility.ToHtmlStringRGB(WireColors[c]);
            text = System.Text.RegularExpressions.Regex.Replace(
                text, @"\b" + names[c] + @"\b", "<color=#" + hex + ">" + names[c] + "</color>");
        }
        return text;
    }
}