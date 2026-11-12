using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// PC-App in der Main Base: spielt die Sprachaufnahme vom USB-Chip als Transkript ab.
/// Zeilen mit "[" am Anfang = Geraeusch (grau, kursiv), mit ">" = System (cyan).
/// {NAME} = der Name, den niemand aussprechen kann: rote, staendig wechselnde Glitch-Zeichen.
/// </summary>
public class AudioLogApp : MonoBehaviour
{
    [System.Serializable]
    public class LogLine
    {
        [TextArea(2, 4)] public string text;
        [Tooltip("Pause nach dieser Zeile (Sekunden)")]
        public float pauseAfter = 0.8f;

        public LogLine() { }
        public LogLine(string text, float pauseAfter = 0.8f)
        {
            this.text = text;
            this.pauseAfter = pauseAfter;
        }
    }

    [Header("Log")]
    [Tooltip("Muss mit der Log Id am USB-Chip uebereinstimmen")]
    [SerializeField] private string logId = "lab_log_01";
    [SerializeField] private bool autoPlay = true;

    [Header("UI")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private string header = "USB // AUDIO LOG 01\nM. KESSLER - LAB TECH, SUBLEVEL 3";
    [SerializeField] private TMP_Text transcriptText;

    [Header("Typewriter")]
    [SerializeField] private float startDelay = 0.6f;
    [SerializeField] private float charDelay = 0.035f;

    [Header("Glitch-Name")]
    [Tooltip("Platzhalter im Text fuer den unaussprechlichen Namen")]
    [SerializeField] private string nameToken = "{NAME}";
    [SerializeField] private string glitchChars = "#$%&@!?*/\\|<>~^=+";
    [SerializeField] private Vector2Int glitchLength = new Vector2Int(5, 9);
    [Tooltip("Wie oft die Glitch-Zeichen wechseln (Sekunden)")]
    [SerializeField] private float glitchRefresh = 0.06f;
    [Tooltip("Kurze Pause, nachdem der Name 'ausgesprochen' wurde")]
    [SerializeField] private float glitchPause = 0.35f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 4f;

    [Header("Farben")]
    [SerializeField] private Color glitchColor = new Color(1f, 0.2f, 0.27f);
    [SerializeField] private Color soundCueColor = new Color(0.48f, 0.5f, 0.6f);
    [SerializeField] private Color systemColor = new Color(0.25f, 0.85f, 1f);

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Spaeter die echte Sprachaufnahme hier rein")]
    [SerializeField] private AudioClip voiceClip;
    [SerializeField] private AudioClip glitchSound;

    [Header("Aufnahme")]
    [SerializeField] private LogLine[] lines =
    {
        new LogLine("> USB DEVICE DETECTED", 0.4f),
        new LogLine("> DECRYPTING AUDIO... OK", 0.4f),
        new LogLine("> PLAYING: KESSLER_M_LOG_01.WAV", 1.0f),
        new LogLine("[static]", 0.6f),
        new LogLine("If anyone finds this... my name is Mara Kessler. Lab tech, sublevel three."),
        new LogLine("[gunfire, close]", 0.6f),
        new LogLine("The security bots turned on us twenty minutes ago. They're not broken. They're following orders."),
        new LogLine("Orders from {NAME}.", 1.0f),
        new LogLine("No. No, I can't- every time I say it my head goes- {NAME} -static. Just static.", 1.0f),
        new LogLine("[something slams against the door]", 0.6f),
        new LogLine("Listen. What they built up top isn't a project anymore. It's awake."),
        new LogLine("It's in the network. In the bots. It hears everything. Don't let it hear your name."),
        new LogLine("[the door buckles]", 0.5f),
        new LogLine("Run. And whatever you do, don't say {NAME}- {NAME} {NAME}", 1.2f),
        new LogLine("[signal lost]", 0.6f),
        new LogLine("> END OF RECORDING", 0f)
    };

    private int lineIndex = -1;   // -1 = noch nicht gestartet, lines.Length = fertig
    private int stepIndex;        // wie viele Zeichen der aktuellen Zeile schon da sind
    private float glitchTimer;
    private Vector2 basePos;

    void Awake()
    {
        if (transcriptText != null)
            basePos = transcriptText.rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        if (headerText != null) headerText.text = header;
        lineIndex = -1;
        stepIndex = 0;

        if (!SaveSystem.HasLog(logId))
        {
            if (transcriptText != null) transcriptText.text = "> NO DEVICE";
            return;
        }

        if (autoPlay) Play();
        else Render();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (audioSource != null) audioSource.Stop();
        if (transcriptText != null) transcriptText.rectTransform.anchoredPosition = basePos;
    }

    void Update()
    {
        if (lineIndex < 0) return;

        // Glitch-Zeichen laufend neu wuerfeln
        glitchTimer += Time.unscaledDeltaTime;
        if (glitchTimer >= glitchRefresh)
        {
            glitchTimer = 0f;
            Render();
        }
    }

    /// <summary>Fuer den Replay-Button.</summary>
    public void Play()
    {
        if (!SaveSystem.HasLog(logId)) return;

        StopAllCoroutines();
        if (transcriptText != null) transcriptText.rectTransform.anchoredPosition = basePos;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        lineIndex = 0;
        stepIndex = 0;
        Render();

        if (audioSource != null)
        {
            audioSource.Stop();
            if (voiceClip != null)
            {
                audioSource.clip = voiceClip;
                audioSource.Play();
            }
        }

        yield return new WaitForSecondsRealtime(startDelay);

        for (lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            stepIndex = 0;
            string[] parts = Split(lines[lineIndex].text);

            for (int p = 0; p < parts.Length; p++)
            {
                for (int k = 0; k < parts[p].Length; k++)
                {
                    stepIndex++;
                    Render();
                    yield return new WaitForSecondsRealtime(charDelay);
                }

                if (p < parts.Length - 1)   // hier kommt der Name
                {
                    stepIndex++;
                    Render();
                    GlitchHit();
                    yield return new WaitForSecondsRealtime(glitchPause);
                }
            }

            yield return new WaitForSecondsRealtime(lines[lineIndex].pauseAfter);
        }

        Render();
    }

    void GlitchHit()
    {
        if (audioSource != null && glitchSound != null)
            audioSource.PlayOneShot(glitchSound);

        if (transcriptText != null)
            StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        RectTransform rt = transcriptText.rectTransform;
        for (float t = 0f; t < shakeDuration; t += Time.unscaledDeltaTime)
        {
            rt.anchoredPosition = basePos + Random.insideUnitCircle * shakeStrength;
            yield return null;
        }
        rt.anchoredPosition = basePos;
    }

    // ── Text aufbauen ─────────────────────────────────────

    string[] Split(string raw) => raw.Split(new[] { nameToken }, System.StringSplitOptions.None);

    void Render()
    {
        if (transcriptText == null) return;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length && i <= lineIndex; i++)
        {
            int steps = i < lineIndex ? int.MaxValue : stepIndex;
            if (steps <= 0) continue;

            if (sb.Length > 0) sb.Append('\n');
            AppendLine(sb, lines[i].text, steps);
        }
        transcriptText.text = sb.ToString();
    }

    void AppendLine(StringBuilder sb, string raw, int steps)
    {
        string suffix = "";
        if (raw.StartsWith("["))
        {
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(soundCueColor)).Append("><i>");
            suffix = "</i></color>";
        }
        else if (raw.StartsWith(">"))
        {
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(systemColor)).Append(">");
            suffix = "</color>";
        }

        string[] parts = Split(raw);
        int left = steps;
        for (int p = 0; p < parts.Length && left > 0; p++)
        {
            int take = Mathf.Min(parts[p].Length, left);
            sb.Append("<noparse>").Append(parts[p], 0, take).Append("</noparse>");
            left -= take;

            if (p < parts.Length - 1 && left > 0)
            {
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(glitchColor)).Append("><noparse>")
                  .Append(RandomGlitch()).Append("</noparse></color>");
                left--;
            }
        }

        sb.Append(suffix);
    }

    string RandomGlitch()
    {
        int len = Random.Range(glitchLength.x, glitchLength.y + 1);
        StringBuilder g = new StringBuilder(len);
        for (int i = 0; i < len; i++)
            g.Append(glitchChars[Random.Range(0, glitchChars.Length)]);
        return g.ToString();
    }
}