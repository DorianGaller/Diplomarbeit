using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Station 2 – Patch-Panel.
/// Links Ports, rechts Ziele. Kabel per Drag &amp; Drop vom Port zum Ziel ziehen.
/// Welcher Port wohin gehoert, steht NICHT im Fenster, sondern auf Hinweis-Zetteln im Raum (PuzzleClue).
/// Sind alle Ports gesteckt, blinken falsche Kabel rot und springen wieder raus.
/// </summary>
public class PatchPanelStation : PuzzleStation
{
    [Header("Patch-Panel")]
    [SerializeField] private string[] targetNames = { "CORE", "FIREWALL", "COOLING", "BACKUP", "DOOR CTRL" };
    [Tooltip("0 = bei jedem Raum-Besuch neu gemischt, sonst immer dieselbe Loesung")]
    [SerializeField] private int seed = 0;

    private static readonly Color32[] CableColors =
    {
        new Color32(63, 224, 255, 255),   // cyan
        new Color32(255, 70, 170, 255),   // pink
        new Color32(242, 194, 48, 255),   // gelb
        new Color32(93, 255, 160, 255),   // gruen
        new Color32(255, 140, 50, 255)    // orange
    };

    private int count;
    private int[] solution;      // solution[port] = Ziel
    private int[] targetRow;     // Anzeige-Zeile pro Ziel (gemischt)
    private int[] conn;          // conn[port] = Ziel oder -1

    private RectTransform[] portRects, targetRects;
    private Vector2[] portPins, targetPins;
    private Image[] cables;
    private Image dragCable;
    private int dragging = -1;
    private bool checking;

    private void Reset()
    {
        stationTitle = "NETWORK PATCH PANEL // RACK 07";
        promptText = "Press F to patch the network";
    }

    protected override void Awake()
    {
        base.Awake();

        // Loesung schon hier festlegen, damit die Hinweis-Zettel in Start() denselben Stand zeigen
        count = Mathf.Min(targetNames.Length, CableColors.Length);
        System.Random rng = seed != 0 ? new System.Random(seed) : new System.Random();

        solution = Shuffled(count, rng);
        targetRow = Shuffled(count, rng);
    }

    public override string GetClue(int index)
    {
        if (solution == null || index < 0 || index >= count) return "";
        return "PORT " + (index + 1) + " -> " + targetNames[solution[index]];
    }

    // ── Oeffnen ───────────────────────────────────────────

    protected override void OnOpen()
    {
        conn = new int[count];
        for (int i = 0; i < count; i++) conn[i] = -1;
        dragging = -1;
        checking = false;

        BuildPanel();
        SetStatus("Drag a cable from each PORT to its target. The labels are somewhere in this room.", ColMuted);
    }

    protected override void OnClose()
    {
        dragging = -1;
    }

    protected override void UpdatePuzzle()
    {
        if (checking) return;

        // Ziehen beginnen
        if (Input.GetMouseButtonDown(0))
        {
            for (int p = 0; p < count; p++)
            {
                if (!MouseOver(portRects[p])) continue;

                conn[p] = -1;   // war schon gesteckt -> wieder rausziehen
                dragging = p;
                dragCable.color = CableColors[p];
                dragCable.enabled = true;
                Play(clickClip);
                RefreshCables();
                break;
            }
        }

        if (dragging < 0) return;

        SetLine(dragCable.rectTransform, portPins[dragging], MouseInContent(), 12f);

        // Loslassen
        if (Input.GetMouseButtonUp(0))
        {
            for (int t = 0; t < count; t++)
            {
                if (!MouseOver(targetRects[t])) continue;

                for (int p = 0; p < count; p++)
                    if (conn[p] == t) conn[p] = -1;   // Ziel war belegt -> altes Kabel raus

                conn[dragging] = t;
                Play(clickClip);
                break;
            }

            dragging = -1;
            dragCable.enabled = false;
            RefreshCables();

            if (AllConnected())
                StartCoroutine(Check());
        }
    }

    private bool AllConnected()
    {
        foreach (int c in conn)
            if (c < 0) return false;
        return true;
    }

    private IEnumerator Check()
    {
        checking = true;

        List<int> wrong = new List<int>();
        for (int p = 0; p < count; p++)
            if (conn[p] != solution[p]) wrong.Add(p);

        if (wrong.Count == 0)
        {
            checking = false;
            Solve();
            yield break;
        }

        Play(errorClip);
        SetStatus("LINK ERROR on " + wrong.Count + (wrong.Count == 1 ? " port" : " ports"), ColRed);

        // Falsche Kabel blinken rot, dann raus
        for (int blink = 0; blink < 3; blink++)
        {
            foreach (int p in wrong) cables[p].color = ColRed;
            yield return new WaitForSecondsRealtime(0.18f);
            foreach (int p in wrong) cables[p].color = CableColors[p];
            yield return new WaitForSecondsRealtime(0.18f);
        }

        foreach (int p in wrong) conn[p] = -1;
        RefreshCables();
        checking = false;
    }

    // ── Anzeige ───────────────────────────────────────────

    private void BuildPanel()
    {
        portRects = new RectTransform[count];
        targetRects = new RectTransform[count];
        portPins = new Vector2[count];
        targetPins = new Vector2[count];
        cables = new Image[count];

        float step = Mathf.Min(100f, 520f / count);
        float top = (count - 1) * step / 2f;

        // Kabel zuerst anlegen, damit sie unter den Steckern liegen
        for (int p = 0; p < count; p++)
        {
            cables[p] = NewImage("Cable " + (p + 1), content, CableColors[p], Vector2.zero, Vector2.zero);
            cables[p].enabled = false;
        }
        dragCable = NewImage("DragCable", content, Color.white, Vector2.zero, Vector2.zero);
        dragCable.enabled = false;

        for (int p = 0; p < count; p++)
        {
            Vector2 pos = new Vector2(-440f, top - p * step);
            Image box = NewImage("Port " + (p + 1), content, ColPanelDark, new Vector2(180, step - 20), pos);
            portRects[p] = box.rectTransform;
            NewText("Label", box.transform, "PORT " + (p + 1), 30, ColText, new Vector2(130, step - 20), new Vector2(-20, 0));
            NewImage("Jack", box.transform, CableColors[p], new Vector2(28, 28), new Vector2(68, 0));
            portPins[p] = pos + new Vector2(68, 0);
        }

        for (int t = 0; t < count; t++)
        {
            Vector2 pos = new Vector2(-10f, top - targetRow[t] * step);
            Image box = NewImage("Target " + targetNames[t], content, ColPanelDark, new Vector2(220, step - 20), pos);
            targetRects[t] = box.rectTransform;
            NewText("Label", box.transform, targetNames[t], 28, ColText, new Vector2(160, step - 20), new Vector2(22, 0));
            NewImage("Socket", box.transform, ColLine, new Vector2(28, 28), new Vector2(-88, 0));
            targetPins[t] = pos + new Vector2(-88, 0);
        }

        string yellow = ColorUtility.ToHtmlStringRGB(ColYellow);
        string red = ColorUtility.ToHtmlStringRGB(ColRed);
        AddInfoPanel("HOW IT WORKS",
            "Connect every PORT to the\nright system.\n\n" +
            "- Drag a cable from a PORT\n  plug to a system\n" +
            "- Click a PORT again to unplug it\n" +
            "- Which port goes where is\n  NOT shown here. Find the\n" +
            "  <color=#" + yellow + ">labels</color> in this room,\n  e.g. PORT 3 -> CORE\n" +
            "- When all ports are plugged,\n  wrong cables flash <color=#" + red + ">red</color>\n  and pop out",
            ColPink);
    }

    private void RefreshCables()
    {
        for (int p = 0; p < count; p++)
        {
            bool on = conn[p] >= 0;
            cables[p].enabled = on;
            if (!on) continue;

            cables[p].color = CableColors[p];
            SetLine(cables[p].rectTransform, portPins[p], targetPins[conn[p]], 12f);
        }
    }

    private static int[] Shuffled(int n, System.Random rng)
    {
        int[] a = new int[n];
        for (int i = 0; i < n; i++) a[i] = i;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }
}