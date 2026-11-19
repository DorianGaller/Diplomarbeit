using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Basis fuer die Stationen im Raetselraum (Stock 2).
/// Spieler kommt in die Naehe -> F oeffnet ein Minigame-Fenster, das per Code aufgebaut wird.
/// ESC (oder F) schliesst es wieder. Geloeste Stationen werden im Spielstand gemerkt.
/// Die einzelnen Minigames erben davon (PowerGridStation, PatchPanelStation, WireCutStation).
/// </summary>
public abstract class PuzzleStation : InteractableBase
{
    [Header("Station")]
    [SerializeField] protected string stationTitle = "STATION";
    [Tooltip("Station, die vorher geloest sein muss. Leer = hat sofort Strom")]
    [SerializeField] private PuzzleStation requiredStation;
    [SerializeField] private string noPowerText = "NO POWER";
    [SerializeField] private string solvedText = "ONLINE";

    [Header("Aussehen in der Welt")]
    [SerializeField] private SpriteRenderer stationRenderer;
    [SerializeField] private Sprite noPowerSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite solvedSprite;
    [Tooltip("Wird beim Loesen eingeschaltet, z.B. die leuchtende Bodenkabel-Tilemap")]
    [SerializeField] private GameObject[] poweredObjects;

    [Header("Sound (optional)")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip clickClip;
    [SerializeField] protected AudioClip errorClip;
    [SerializeField] private AudioClip solvedClip;

    public bool IsSolved { get; private set; }
    public bool HasPower => requiredStation == null || requiredStation.IsSolved;
    public bool IsOpen => windowRoot != null;

    /// <summary>Wird beim Loesen gefeuert (z.B. fuer die Tuer).</summary>
    public event System.Action<PuzzleStation> OnSolved;

    // ── Farben (Serverraum-Stil) ──────────────────────────
    protected static readonly Color ColBackdrop = new Color32(6, 8, 14, 220);
    protected static readonly Color ColPanel = new Color32(24, 26, 42, 255);
    protected static readonly Color ColPanelDark = new Color32(14, 16, 28, 255);
    protected static readonly Color ColLine = new Color32(70, 70, 110, 255);
    protected static readonly Color ColPurple = new Color32(150, 60, 220, 255);
    protected static readonly Color ColCyan = new Color32(63, 224, 255, 255);
    protected static readonly Color ColPink = new Color32(255, 70, 170, 255);
    protected static readonly Color ColYellow = new Color32(242, 194, 48, 255);
    protected static readonly Color ColRed = new Color32(255, 59, 85, 255);
    protected static readonly Color ColGreen = new Color32(93, 255, 160, 255);
    protected static readonly Color ColText = new Color32(230, 233, 245, 255);
    protected static readonly Color ColMuted = new Color32(139, 146, 173, 255);
    protected static readonly Color ColOff = new Color32(80, 86, 120, 255);

    protected RectTransform content;   // Spielfeld, Ursprung = Mitte
    private GameObject windowRoot;
    private Image backdrop;
    private TMP_Text statusText;
    private string basePrompt;
    private bool wasInRange;
    private bool controlsLocked;
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;

    // ── Unity ─────────────────────────────────────────────

    protected virtual void Awake()
    {
        // Spielstand ist beim Laden der Szene schon da -> hier lesen,
        // damit andere Stationen und die Tuer es in Start() sicher sehen
        IsSolved = SaveSystem.IsCleared(SaveSystem.GetObjectId(gameObject));
    }

    protected override void Start()
    {
        base.Start();
        basePrompt = promptText;

        if (requiredStation != null)
            requiredStation.OnSolved += _ => UpdateVisuals();

        UpdateVisuals();
    }

    protected override void Update()
    {
        if (IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactionKey))
                Close();
            else if (!IsSolved)
                UpdatePuzzle();
            return;
        }

        base.Update();

        // InteractableBase zeigt den Hinweis beim Reinlaufen nicht zuverlaessig an -> hier nachziehen
        if (playerInRange != wasInRange)
        {
            wasInRange = playerInRange;
            RefreshPrompt();
        }
    }

    protected virtual void OnDisable()
    {
        if (IsOpen) Close();
    }

    protected override bool ShouldShowPrompt() => !IsOpen;

    protected override void OnInteractPressed()
    {
        if (IsSolved) return;

        if (!HasPower)
        {
            Play(errorClip);
            return;
        }

        Open();
    }

    // ── Fuer die Minigames ────────────────────────────────

    /// <summary>Baut das Spielfeld in 'content' auf (Ursprung Mitte, ca. 1000 x 560).</summary>
    protected abstract void OnOpen();

    /// <summary>Laeuft jeden Frame, solange das Fenster offen und die Station ungeloest ist.</summary>
    protected abstract void UpdatePuzzle();

    protected virtual void OnClose() { }

    /// <summary>Text fuer Hinweis-Zettel in der Welt (PuzzleClue).</summary>
    public virtual string GetClue(int index) => "";

    protected void Solve()
    {
        if (IsSolved) return;

        IsSolved = true;
        SaveSystem.MarkCleared(SaveSystem.GetObjectId(gameObject));
        Play(solvedClip);
        SetStatus("SYSTEM ONLINE", ColGreen);
        UpdateVisuals();
        OnSolved?.Invoke(this);

        if (IsOpen)
            StartCoroutine(CloseAfter(1.3f));
    }

    protected void SetStatus(string text, Color color)
    {
        if (statusText == null) return;
        statusText.text = text;
        statusText.color = color;
    }

    protected void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }

    /// <summary>Kurzes rotes Aufblitzen des Hintergrunds (Fehler/Alarm).</summary>
    protected void FlashBackdrop(Color color)
    {
        if (backdrop != null) StartCoroutine(FlashRoutine(color));
    }

    protected void Close()
    {
        if (!IsOpen) return;

        StopAllCoroutines();
        OnClose();
        Destroy(windowRoot);
        windowRoot = null;
        content = null;
        statusText = null;
        backdrop = null;
        LockPlayer(false);
        UpdateVisuals();
        RefreshPrompt();
    }

    // ── Fenster ───────────────────────────────────────────

    private void Open()
    {
        if (IsOpen) return;

        windowRoot = new GameObject(stationTitle + " Window", typeof(RectTransform));
        Canvas canvas = windowRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = windowRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = (RectTransform)windowRoot.transform;

        backdrop = NewImage("Backdrop", root, ColBackdrop, Vector2.zero, Vector2.zero);
        Stretch(backdrop.rectTransform);

        Image frame = NewImage("Frame", root, ColPurple, new Vector2(1164, 824), Vector2.zero);
        NewImage("Panel", frame.transform, ColPanel, new Vector2(1152, 812), Vector2.zero);
        NewImage("TitleBar", frame.transform, ColPanelDark, new Vector2(1152, 80), new Vector2(0, 366));
        NewText("Title", frame.transform, stationTitle, 44, ColCyan, new Vector2(1100, 70), new Vector2(0, 366));
        NewText("Hint", frame.transform, "[ESC] close", 26, ColMuted, new Vector2(300, 40), new Vector2(400, -376),
                TextAlignmentOptions.Right);
        statusText = NewText("Status", frame.transform, "", 30, ColMuted, new Vector2(900, 50), new Vector2(-90, -376),
                             TextAlignmentOptions.Left);

        content = NewRect("Content", frame.transform, new Vector2(1060, 600), new Vector2(0, -6));

        if (PlayerInteractPrompt.Instance != null) PlayerInteractPrompt.Instance.Hide();
        LockPlayer(true);
        OnOpen();
    }

    private IEnumerator CloseAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Close();
    }

    private IEnumerator FlashRoutine(Color color)
    {
        for (float t = 0f; t < 0.5f && backdrop != null; t += Time.unscaledDeltaTime)
        {
            backdrop.color = Color.Lerp(color, ColBackdrop, t / 0.5f);
            yield return null;
        }
        if (backdrop != null) backdrop.color = ColBackdrop;
    }

    private void LockPlayer(bool locked)
    {
        if (locked == controlsLocked) return;
        controlsLocked = locked;

        GameObject playerObj = player != null ? player.gameObject : GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            PlayerMovement movement = playerObj.GetComponentInChildren<PlayerMovement>();
            if (movement != null) movement.SetMovementInputLocked(locked);

            InteractKeys keys = playerObj.GetComponentInChildren<InteractKeys>();
            if (keys != null) keys.EnableShooting(!locked);
        }

        if (locked)
        {
            prevCursorVisible = Cursor.visible;
            prevCursorLock = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = prevCursorVisible;
            Cursor.lockState = prevCursorLock;
        }
    }

    private void UpdateVisuals()
    {
        if (stationRenderer != null)
        {
            Sprite s = IsSolved ? solvedSprite : HasPower ? activeSprite : noPowerSprite;
            if (s != null) stationRenderer.sprite = s;
        }

        if (poweredObjects != null)
            foreach (GameObject go in poweredObjects)
                if (go != null) go.SetActive(IsSolved);

        if (basePrompt == null) basePrompt = promptText;
        SetPromptText(IsSolved ? solvedText : HasPower ? basePrompt : noPowerText);
        RefreshPrompt();
    }

    // ── UI-Helfer (alles mittig verankert) ────────────────

    protected static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return rt;
    }

    protected static Image NewImage(string name, Transform parent, Color color, Vector2 size, Vector2 pos)
    {
        RectTransform rt = NewRect(name, parent, size, pos);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    protected static TMP_Text NewText(string name, Transform parent, string text, float fontSize, Color color,
                                      Vector2 size, Vector2 pos,
                                      TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        RectTransform rt = NewRect(name, parent, size, pos);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    protected static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

        /// <summary>Erklaer-Kasten rechts im Fenster (gleiche Groesse wie das Handbuch im Kabelschacht).</summary>
    protected void AddInfoPanel(string title, string body, Color accent)
    {
        Vector2 m = new Vector2(315, 0);
        NewImage("InfoFrame", content, accent, new Vector2(424, 524), m);
        NewImage("InfoPaper", content, ColPanelDark, new Vector2(416, 516), m);
        NewImage("InfoHeader", content, accent, new Vector2(416, 52), m + new Vector2(0, 232));
        NewText("InfoTitle", content, title, 30, ColPanelDark, new Vector2(400, 50), m + new Vector2(0, 232));
        NewText("InfoText", content, body, 22, ColText, new Vector2(380, 440), m + new Vector2(0, -26),
                TextAlignmentOptions.TopLeft);
    }

    /// <summary>Linie von a nach b (beide im Koordinatensystem von 'content').</summary>
    protected static void SetLine(RectTransform line, Vector2 a, Vector2 b, float thickness)
    {
        Vector2 d = b - a;
        line.pivot = new Vector2(0f, 0.5f);
        line.anchoredPosition = a;
        line.sizeDelta = new Vector2(d.magnitude, thickness);
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
    }

    protected static bool MouseOver(RectTransform rt) =>
        rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, null);

    /// <summary>Mausposition im Koordinatensystem von 'content'.</summary>
    protected Vector2 MouseInContent()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, Input.mousePosition, null, out Vector2 p);
        return p;
    }
}