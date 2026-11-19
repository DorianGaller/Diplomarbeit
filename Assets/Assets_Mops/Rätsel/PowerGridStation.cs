using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Station 1 – Strom umleiten.
/// Raster aus Kabelstuecken: Linksklick dreht im Uhrzeigersinn, Rechtsklick dagegen.
/// Ziel: durchgehende Leitung vom Generator (links) zum Ausgang (rechts).
/// Verschmorte Felder lassen sich nicht drehen, man muss aussen herum bauen.
/// Das Raster wird zufaellig erzeugt und ist immer loesbar.
/// </summary>
public class PowerGridStation : PuzzleStation
{
    [Header("Raster")]
    [SerializeField, Range(4, 7)] private int columns = 5;
    [SerializeField, Range(3, 6)] private int rows = 5;
    [Tooltip("Verschmorte Felder, die sich nicht drehen lassen")]
    [SerializeField, Range(0, 4)] private int burntTiles = 2;
    [Tooltip("0 = bei jedem Oeffnen ein neues Raster, sonst immer dasselbe")]
    [SerializeField] private int seed = 0;

    private const int N = 1, E = 2, S = 4, W = 8;
    private static readonly int[] Dirs = { N, E, S, W };

    private int[] mask;
    private bool[] burnt;
    private bool[] lit;
    private int startCell, endCell;

    private RectTransform[] cellRects;
    private Image[,] arms;     // [Zelle, Richtung 0..3]
    private Image[] hubs;
    private Image genBox, outBox, genLine, outLine;

    private void Reset()
    {
        stationTitle = "POWER ROUTING // GENERATOR B2";
        promptText = "Press F to reroute power";
    }

    // ── Oeffnen ───────────────────────────────────────────

    protected override void OnOpen()
    {
        Generate();
        BuildBoard();
        Refresh();
        SetStatus("Click to rotate (right click = back). Route power from GEN to OUT.", ColMuted);
    }

    protected override void UpdatePuzzle()
    {
        bool left = Input.GetMouseButtonDown(0);
        bool right = Input.GetMouseButtonDown(1);
        if (!left && !right) return;

        for (int i = 0; i < cellRects.Length; i++)
        {
            if (!MouseOver(cellRects[i])) continue;

            if (burnt[i])
            {
                Play(errorClip);
                SetStatus("This tile is fried. Build around it.", ColRed);
                return;
            }

            mask[i] = left ? Rot(mask[i]) : Rot(Rot(Rot(mask[i])));
            Play(clickClip);
            ComputeLit();
            Refresh();

            if (IsComplete())
                Solve();
            return;
        }
    }

    // ── Raster erzeugen ───────────────────────────────────

    private void Generate()
    {
        System.Random rng = seed != 0 ? new System.Random(seed) : new System.Random();
        int n = columns * rows;

        for (int attempt = 0; attempt < 200; attempt++)
        {
            mask = new int[n];
            burnt = new bool[n];

            startCell = Idx(0, rng.Next(rows));
            endCell = Idx(columns - 1, rng.Next(rows));

            HashSet<int> blocked = new HashSet<int>();
            while (blocked.Count < burntTiles)
                blocked.Add(Idx(1 + rng.Next(columns - 2), rng.Next(rows)));

            List<int> path = FindPath(startCell, endCell, blocked, rng);
            if (path == null || path.Count < columns + 2 || path.Count > n * 0.65f)
                continue;   // kein Weg, zu kurz (zu leicht) oder zu lang (Chaos)

            // Weg-Stuecke passend zum Weg
            for (int i = 0; i < path.Count; i++)
            {
                int m = i == 0 ? W : DirTo(path[i], path[i - 1]);
                m |= i == path.Count - 1 ? E : DirTo(path[i], path[i + 1]);
                mask[path[i]] = m;
            }

            // Rest: zufaellige Stuecke (gerade, Kurve, T)
            HashSet<int> onPath = new HashSet<int>(path);
            for (int i = 0; i < n; i++)
            {
                if (onPath.Contains(i)) continue;
                if (blocked.Contains(i)) { burnt[i] = true; mask[i] = 0; continue; }

                int t = rng.Next(3);
                mask[i] = t == 0 ? (N | S) : t == 1 ? (N | E) : (N | E | S);
            }

            // Alles verdrehen
            for (int i = 0; i < n; i++)
            {
                if (burnt[i]) continue;
                int turns = rng.Next(4);
                for (int k = 0; k < turns; k++) mask[i] = Rot(mask[i]);
            }

            ComputeLit();
            if (!IsComplete()) return;   // fertig, aber nicht schon geloest
        }

        Debug.LogWarning("[PowerGridStation] Konnte kein gutes Raster erzeugen, nimm weniger verschmorte Felder.");
    }

    private List<int> FindPath(int start, int goal, HashSet<int> blocked, System.Random rng)
    {
        List<int> path = new List<int>();
        bool[] visited = new bool[columns * rows];
        return Dfs(start, goal, blocked, rng, path, visited) ? path : null;
    }

    private bool Dfs(int cur, int goal, HashSet<int> blocked, System.Random rng, List<int> path, bool[] visited)
    {
        path.Add(cur);
        visited[cur] = true;
        if (cur == goal) return true;

        int[] order = (int[])Dirs.Clone();
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        foreach (int d in order)
        {
            int nb = Neighbor(cur, d);
            if (nb < 0 || visited[nb] || blocked.Contains(nb)) continue;
            if (Dfs(nb, goal, blocked, rng, path, visited)) return true;
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    // ── Logik ─────────────────────────────────────────────

    private void ComputeLit()
    {
        lit = new bool[mask.Length];
        if ((mask[startCell] & W) == 0) return;

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startCell);
        lit[startCell] = true;

        while (queue.Count > 0)
        {
            int cur = queue.Dequeue();
            foreach (int d in Dirs)
            {
                if ((mask[cur] & d) == 0) continue;
                int nb = Neighbor(cur, d);
                if (nb < 0 || lit[nb] || (mask[nb] & Opp(d)) == 0) continue;
                lit[nb] = true;
                queue.Enqueue(nb);
            }
        }
    }

    private bool IsComplete() => lit[endCell] && (mask[endCell] & E) != 0;

    private int Idx(int col, int row) => row * columns + col;

    private int Neighbor(int i, int d)
    {
        int r = i / columns, c = i % columns;
        if (d == N) r--;
        else if (d == S) r++;
        else if (d == E) c++;
        else c--;
        return r < 0 || r >= rows || c < 0 || c >= columns ? -1 : Idx(c, r);
    }

    private int DirTo(int from, int to)
    {
        if (to == from + 1) return E;
        if (to == from - 1) return W;
        return to > from ? S : N;
    }

    private static int Rot(int m) => ((m << 1) | (m >> 3)) & 15;   // im Uhrzeigersinn
    private static int Opp(int d) => ((d << 2) | (d >> 2)) & 15;

    // ── Anzeige ───────────────────────────────────────────

    private void BuildBoard()
    {
                // Links das Raster (mit GEN und OUT), rechts der Erklaer-Kasten
        Vector2 center = new Vector2(-212f, 0f);
        float cell = Mathf.Min(520f / rows, 620f / (columns + 3.3f));
        float thick = cell * 0.2f;
        Vector2 origin = center + new Vector2(-(columns - 1) * cell / 2f, (rows - 1) * cell / 2f);

        int n = columns * rows;
        cellRects = new RectTransform[n];
        arms = new Image[n, 4];
        hubs = new Image[n];

                NewImage("Board", content, ColLine, new Vector2(columns * cell + 12, rows * cell + 12), center);

        for (int i = 0; i < n; i++)
        {
            Vector2 pos = origin + new Vector2((i % columns) * cell, -(i / columns) * cell);
            Image bg = NewImage("Cell " + i, content, burnt[i] ? new Color32(48, 18, 26, 255) : ColPanelDark,
                                new Vector2(cell - 6, cell - 6), pos);
            cellRects[i] = bg.rectTransform;

            if (burnt[i])
            {
                NewText("Burnt", bg.transform, "X", cell * 0.5f, ColRed, new Vector2(cell, cell), Vector2.zero);
                continue;
            }

            for (int k = 0; k < 4; k++)
            {
                Vector2 dir = k == 0 ? Vector2.up : k == 1 ? Vector2.right : k == 2 ? Vector2.down : Vector2.left;
                Vector2 size = k % 2 == 0 ? new Vector2(thick, cell / 2f) : new Vector2(cell / 2f, thick);
                arms[i, k] = NewImage("Arm" + k, bg.transform, ColOff, size, dir * cell / 4f);
            }
            hubs[i] = NewImage("Hub", bg.transform, ColOff, new Vector2(thick * 1.4f, thick * 1.4f), Vector2.zero);
        }

        // Generator links, Ausgang rechts
        Vector2 startPos = origin + new Vector2(0, -(startCell / columns) * cell);
        Vector2 endPos = origin + new Vector2((columns - 1) * cell, -(endCell / columns) * cell);

        genLine = NewImage("GenLine", content, ColCyan, new Vector2(cell * 0.9f, thick), startPos + new Vector2(-cell * 0.95f, 0));
        genBox = NewImage("Gen", content, ColCyan, new Vector2(cell * 1.1f, cell * 0.8f), startPos + new Vector2(-cell * 1.6f, 0));
        NewText("GenText", genBox.transform, "GEN", cell * 0.3f, ColPanelDark, new Vector2(cell, cell), Vector2.zero);

        outLine = NewImage("OutLine", content, ColOff, new Vector2(cell * 0.9f, thick), endPos + new Vector2(cell * 0.95f, 0));
        outBox = NewImage("Out", content, ColOff, new Vector2(cell * 1.1f, cell * 0.8f), endPos + new Vector2(cell * 1.6f, 0));
        NewText("OutText", outBox.transform, "OUT", cell * 0.3f, ColPanelDark, new Vector2(cell, cell), Vector2.zero);

                string cyan = ColorUtility.ToHtmlStringRGB(ColCyan);
        string red = ColorUtility.ToHtmlStringRGB(ColRed);
        AddInfoPanel("HOW IT WORKS",
            "Bring power from <color=#" + cyan + ">GEN</color> to <color=#" + cyan + ">OUT</color>.\n\n" +
            "- Left click: turn a tile\n" +
            "- Right click: turn it back\n" +
            "- Powered lines glow <color=#" + cyan + ">cyan</color>\n" +
            "- <color=#" + red + ">X</color> tiles are fried and\n" +
            "  can't be turned.\n" +
            "  Build around them!\n\n" +
            "Every grid can be solved.",
            ColCyan);
    }

    private void Refresh()
    {
        for (int i = 0; i < mask.Length; i++)
        {
            if (burnt[i]) continue;

            Color c = lit[i] ? ColCyan : ColOff;
            for (int k = 0; k < 4; k++)
            {
                arms[i, k].enabled = (mask[i] & Dirs[k]) != 0;
                arms[i, k].color = c;
            }
            hubs[i].enabled = mask[i] != 0;
            hubs[i].color = c;
        }

        Color outColor = IsComplete() ? ColCyan : ColOff;
        outLine.color = outColor;
        outBox.color = outColor;
    }
}