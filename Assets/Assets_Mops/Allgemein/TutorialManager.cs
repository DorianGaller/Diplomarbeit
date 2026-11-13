using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Tutorial als Phone Call:
/// 1. "Incoming Call" erscheint, mit der Annehmen-Taste abheben
/// 2. Nachrichten abwechselnd links (Anrufer, rot) und rechts (Spieler, gruen),
///    jeweils mit Schreibmaschinen-Effekt, danach automatisch weiter
/// 3. Auflegen -> Spieler wird wieder freigegeben
/// Kommt nur einmal pro Spielstart/Profil und nur bis das Level geschafft ist.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class CallLine
    {
        [Tooltip("Haken = Spieler (gruen, rechts). Ohne Haken = Anrufer (rot, links)")]
        public bool fromPlayer;
        [TextArea(2, 4)] public string text;
    }

    [Header("Incoming Call")]
    [SerializeField] private GameObject incomingPanel;
    [SerializeField] private RectTransform incomingBadge;   // pulsiert
    [SerializeField] private RectTransform incomingRing;    // dreht sich
    [SerializeField] private TMP_Text incomingText;
    [SerializeField] private KeyCode answerKey = KeyCode.Space;


    [Header("Ablauf")]
    [Tooltip("Aus = startet nicht von selbst (z.B. im Fight Room, dort steuern die EnemyIntroRoom-Trigger den Call)")]
    [SerializeField] private bool playOnStart = true;

    [Header("Nachrichten")]
    [Tooltip("Bereich fuer die Nachrichten, Pivot oben")]
    [SerializeField] private RectTransform messageArea;
    [Tooltip("Vorlage (inaktiv), aufgebaut fuer die LINKE Seite")]
    [SerializeField] private CallMessageView messageTemplate;
    [SerializeField] private float rowSpacing = 150f;
    [SerializeField] private int maxVisible = 4;

    [Header("Layout (wird per Code gesetzt)")]
    [SerializeField] private float badgeSize = 96f;
    [SerializeField] private float nameHeight = 30f;
    [SerializeField] private float textGap = 20f;
    [SerializeField] private float textWidth = 700f;
    [SerializeField] private float nameFontSize = 24f;
    [SerializeField] private float bodyFontSize = 26f;
    [SerializeField] private Color backdropColor = new Color(0.02f, 0.04f, 0.07f, 0.65f);
    [SerializeField] private float backdropPadding = 12f;

    [Header("Namen & Farben")]
    [SerializeField] private string callerName = "Ryan Stekken";
    [SerializeField] private string playerName = "You";
    [SerializeField] private Color callerColor = new Color(1f, 0.35f, 0.38f);
    [SerializeField] private Color playerColor = new Color(0.4f, 1f, 0.55f);
    [SerializeField] private Sprite callerBadge;
    [SerializeField] private Sprite callerRing;
    [SerializeField] private Sprite playerBadge;
    [SerializeField] private Sprite playerRing;

    [Header("Timing")]
    [SerializeField] private float charsPerSecond = 40f;
    [Tooltip("Pause nach jeder Nachricht: Grundzeit + pro Zeichen")]
    [SerializeField] private float pauseBase = 1.0f;
    [SerializeField] private float pausePerChar = 0.035f;
    [SerializeField] private float endDelay = 1.5f;
    [SerializeField] private float ringSpeed = 220f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ringClip;
    [SerializeField] private AudioClip typeClip;
    [SerializeField] private AudioClip hangUpClip;

    [Header("Dialog")]
    [SerializeField]
    private CallLine[] lines =
    {
        new CallLine { fromPlayer = false, text = "Choom, you awake? Good. Listen up, this lab is locked down tight." },
        new CallLine { fromPlayer = true,  text = "Ryan? Where the hell am I?" },
        new CallLine { fromPlayer = false, text = "Corpo lab. No time for a tour. See those tables? Press G to grab one, G again to drop it." },
        new CallLine { fromPlayer = true,  text = "Moving furniture. Real glamorous." },
        new CallLine { fromPlayer = false, text = "Climb a table to reach the vents. T to get up, T again to get down." },
        new CallLine { fromPlayer = false, text = "Up there, hit F to pop the vent open. F again and you're crawling." },
        new CallLine { fromPlayer = true,  text = "And the locked door?" },
        new CallLine { fromPlayer = false, text = "Keycard lock. Find the card, hold it in your main hand and click the door. Then F to walk through." },
        new CallLine { fromPlayer = true,  text = "Got it." },
        new CallLine { fromPlayer = false, text = "Don't flatline on me, rookie. Bad for business." }
    };

    /// <summary>True solange der Call laeuft (andere Skripte koennen damit pausieren).</summary>
    public static bool IsRunning { get; private set; }
    private bool callActive;

    // Bleibt gesetzt bis das Spiel neu gestartet wird
    private static bool hasShown = false;

    /// <summary>Beim Starten/Laden eines Profils darf das Tutorial wieder kommen.</summary>
    public static void ResetSession() => hasShown = false;

    private PlayerMovement playerMovement;
    private InteractKeys interactKeys;
    private readonly List<CallMessageView> rows = new List<CallMessageView>();

    private void Start()
    {
        if (incomingPanel != null) incomingPanel.SetActive(false);
                if (messageTemplate != null) messageTemplate.gameObject.SetActive(false);

        if (!playOnStart) return;

        // Nur bis das Level einmal geschafft ist (Boss besiegt)
        if (hasShown || SaveSystem.IsLevelCompleted(SaveSystem.GetLevelId(gameObject.scene.path))) return;

        StartCoroutine(RunCall());
    }

    private IEnumerator RunCall()
    {
        // Wartet einen Frame, damit DontDestroy-Duplikate bereinigt sind
        yield return null;

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("TutorialManager: Kein Dialog eingetragen!");
            yield break;
        }

        hasShown = true;
        yield return PlayPart(lines, true, true, false);
    }

    /// <summary>
    /// Spielt einen Teil eines Gespraechs ab (wird auch von EnemyIntroRoom benutzt).
    /// ring: erst "Incoming Call" + Annehmen. hangUp: am Ende Auflege-Sound.
    /// lockWholeTime: Spieler bleibt gesperrt bis der Teil fertig ist (sonst nur bis zum Annehmen).
    /// </summary>
    public IEnumerator PlayPart(CallLine[] partLines, bool ring, bool hangUp, bool lockWholeTime)
    {
        if (partLines == null || partLines.Length == 0) yield break;

        if (messageArea == null || messageTemplate == null || messageTemplate.bodyText == null
            || (ring && incomingPanel == null))
        {
            Debug.LogWarning("TutorialManager: Incoming Panel, Message Area oder Vorlage (mit Body Text) fehlen!");
            yield break;
        }

        ResolvePlayer();
        callActive = true;
        IsRunning = true;
        SetPlayerLocked(true);

        // Falls das Canvas im Editor ausgeschaltet wurde, mit einschalten
        Canvas canvas = messageArea.GetComponentInParent<Canvas>(true);
        if (canvas != null)
            canvas.gameObject.SetActive(true);

        // ── 1. Anruf kommt rein ──
        if (ring)
        {
            CenterRing(incomingRing, incomingBadge);

            if (incomingText != null)
                incomingText.text = "INCOMING CALL\n<b>" + callerName + "</b>\n<size=60%>[" + KeyName(answerKey) + "] answer</size>";

            incomingPanel.SetActive(true);
            PlayLoop(ringClip);

            while (!Input.GetKeyDown(answerKey))
            {
                AnimateIncoming();
                yield return null;
            }

            StopLoop();
            incomingPanel.SetActive(false);
        }

        if (!lockWholeTime)
        {
            IsRunning = false;
            SetPlayerLocked(false);
        }

        // ── 2. Gespraech ──
        foreach (CallLine line in partLines)
        {
            if (line == null || string.IsNullOrEmpty(line.text)) continue;

            CallMessageView view = SpawnRow(line);
            yield return TypeText(view.bodyText);
            yield return new WaitForSeconds(pauseBase + pausePerChar * line.text.Length);
        }

        // ── 3. Teil zu Ende, Nachrichten verschwinden (bei hangUp: auflegen) ──
        yield return new WaitForSeconds(endDelay);

        if (hangUp && audioSource != null && hangUpClip != null)
            audioSource.PlayOneShot(hangUpClip);

        EndTutorial();
    }

    private void ResolvePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        playerMovement = playerObj.GetComponent<PlayerMovement>();
        interactKeys = playerObj.GetComponent<InteractKeys>();

        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (!callActive) return;

        // Ringe der sichtbaren Nachrichten drehen sich weiter
        foreach (CallMessageView row in rows)
            if (row != null && row.ring != null)
                row.ring.rectTransform.Rotate(0f, 0f, -ringSpeed * Time.deltaTime);
    }

    private void AnimateIncoming()
    {
        if (incomingBadge != null)
            incomingBadge.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(Time.time * 8f));

        if (incomingRing != null)
            incomingRing.Rotate(0f, 0f, -ringSpeed * Time.deltaTime);
    }

    // ── Nachrichten ───────────────────────────────────────

    private CallMessageView SpawnRow(CallLine line)
    {
        CallMessageView view = Instantiate(messageTemplate, messageArea);
        view.gameObject.SetActive(true);

        bool player = line.fromPlayer;
        Color col = player ? playerColor : callerColor;

        if (view.nameText != null)
        {
            view.nameText.text = player ? playerName : callerName;
            view.nameText.color = col;
        }

        Sprite badgeSprite = player ? playerBadge : callerBadge;
        if (view.badge != null && badgeSprite != null)
            view.badge.sprite = badgeSprite;

        Sprite ringSprite = player ? playerRing : callerRing;
        if (view.ring != null && ringSprite != null)
            view.ring.sprite = ringSprite;

        view.bodyText.text = line.text;
        view.bodyText.color = col;
        view.bodyText.maxVisibleCharacters = 0;

        ApplyLayout(view, player);

        rows.Add(view);
        if (rows.Count > maxVisible)
        {
            Destroy(rows[0].gameObject);
            rows.RemoveAt(0);
        }

        LayoutRows();
        return view;
    }

    // Untereinander stapeln, neueste unten
    private void LayoutRows()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            RectTransform rt = (RectTransform)rows[i].transform;
            rt.anchoredPosition = new Vector2(0f, -i * rowSpacing);
        }
    }

    // Vorlage ist fuer links gebaut -> fuer den Spieler horizontal spiegeln
    private void ApplyLayout(CallMessageView view, bool right)
    {
        float side = right ? 1f : 0f;   // 0 = linker Rand, 1 = rechter Rand
        float dir = right ? -1f : 1f;   // Richtung vom Rand nach innen

        // Zeile ueber die volle Breite der MessageArea
        RectTransform row = (RectTransform)view.transform;
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.sizeDelta = new Vector2(0f, nameHeight + badgeSize);

        // Name ueber dem Badge
        if (view.nameText != null)
        {
            Place(view.nameText.rectTransform, side, Vector2.zero, new Vector2(badgeSize * 4f, nameHeight));
            view.nameText.fontSize = nameFontSize;
            view.nameText.alignment = right ? TextAlignmentOptions.BottomRight : TextAlignmentOptions.BottomLeft;
        }

        // Badge direkt am Rand, Ring genau darauf
        if (view.badge != null)
        {
            Place(view.badge.rectTransform, side, new Vector2(0f, -nameHeight), new Vector2(badgeSize, badgeSize));
            view.badge.preserveAspect = true;

            if (view.ring != null)
                CenterRing(view.ring.rectTransform, view.badge.rectTransform);
        }

        // Text neben dem Badge, vertikal mittig
        Place(view.bodyText.rectTransform, side, new Vector2(dir * (badgeSize + textGap), -nameHeight),
              new Vector2(textWidth, badgeSize));
        view.bodyText.fontSize = bodyFontSize;
        view.bodyText.alignment = right ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;

        // Dunkles Panel hinter Name, Badge und Text, so breit wie der Text wirklich ist
        Vector2 pref = view.bodyText.GetPreferredValues(view.bodyText.text, textWidth, badgeSize);
        float width = badgeSize + textGap + Mathf.Min(textWidth, pref.x) + backdropPadding * 2f;
        float height = nameHeight + badgeSize + backdropPadding * 2f;

        Image backdrop = GetOrCreateBackdrop(view);
        Place(backdrop.rectTransform, side, new Vector2(-dir * backdropPadding, backdropPadding), new Vector2(width, height));
        backdrop.color = backdropColor;

        // Schmaler Leuchtstreifen in der Seitenfarbe an der Aussenkante
        RectTransform accent = backdrop.transform.Find("Accent") as RectTransform;
        if (accent != null)
        {
            accent.anchorMin = new Vector2(side, 0f);
            accent.anchorMax = new Vector2(side, 1f);
            accent.pivot = new Vector2(side, 0.5f);
            accent.sizeDelta = new Vector2(4f, 0f);
            accent.anchoredPosition = Vector2.zero;

            Color c = right ? playerColor : callerColor;
            c.a = 0.9f;
            accent.GetComponent<Image>().color = c;
        }
    }

    // Legt das Panel (+ Leuchtstreifen) einmal an und schiebt es hinter alles andere
    private static Image GetOrCreateBackdrop(CallMessageView view)
    {
        Transform existing = view.transform.Find("Backdrop");
        Image img;

        if (existing != null)
        {
            img = existing.GetComponent<Image>();
        }
        else
        {
            GameObject go = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(view.transform, false);
            img = go.GetComponent<Image>();
            img.raycastTarget = false;

            GameObject accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(go.transform, false);
            accent.GetComponent<Image>().raycastTarget = false;
        }

        img.transform.SetAsFirstSibling();   // hinter Name, Badge und Text
        return img;
    }

    // Setzt ein Element an den linken (side 0) oder rechten (side 1) Rand der Zeile
    private static void Place(RectTransform rt, float side, Vector2 offset, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(side, 1f);
        rt.pivot = new Vector2(side, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = offset;
    }

    // Ring genau mittig aufs Badge legen, damit er sich um die Mitte dreht
    private static void CenterRing(RectTransform ring, RectTransform badge)
    {
        if (ring == null || badge == null) return;

        if (ring.parent != badge)
            ring.SetParent(badge, false);

        ring.anchorMin = ring.anchorMax = ring.pivot = new Vector2(0.5f, 0.5f);
        ring.anchoredPosition = Vector2.zero;
        ring.sizeDelta = badge.rect.size;
        ring.localRotation = Quaternion.identity;
        ring.localScale = Vector3.one;
    }

    // Schreibmaschinen-Effekt
    private IEnumerator TypeText(TMP_Text text)
    {
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        float shown = 0f;
        int lastSound = 0;

        while (text.maxVisibleCharacters < total)
        {
            shown += charsPerSecond * Time.deltaTime;
            text.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));

            if (audioSource != null && typeClip != null && text.maxVisibleCharacters - lastSound >= 2)
            {
                lastSound = text.maxVisibleCharacters;
                audioSource.PlayOneShot(typeClip, 0.4f);
            }
            yield return null;
        }
    }

    // ── Ende ──────────────────────────────────────────────

    private void EndTutorial()
    {
        IsRunning = false;
        callActive = false;

        foreach (CallMessageView row in rows)
            if (row != null) Destroy(row.gameObject);
        rows.Clear();

        StopLoop();
        if (incomingPanel != null) incomingPanel.SetActive(false);

        

        SetPlayerLocked(false);
    }

    private void SetPlayerLocked(bool locked)
    {
        // Laufen + Dash sperren (Dash braucht eine Richtung)
        if (playerMovement != null)
            playerMovement.SetMovementInputLocked(locked);

        // Schiessen + F-Interaktion sperren
        if (interactKeys != null)
            interactKeys.enabled = !locked;
    }

    // ── Sound ─────────────────────────────────────────────

    private void PlayLoop(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopLoop()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    private static string KeyName(KeyCode key) =>
        key == KeyCode.Space ? "SPACE" : key.ToString().ToUpper();

    private void OnDestroy()
    {
        // Falls die Szene mitten im Call gewechselt wird
        if (IsRunning)
        {
            IsRunning = false;
            SetPlayerLocked(false);
        }
    }
}