using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hintergrundmusik fuer das ganze Spiel. Ein einziges Objekt, das Szenenwechsel ueberlebt
/// und beim ersten Aufruf von Get() automatisch entsteht (nichts in eine Szene legen).
/// - Blendet weich zwischen zwei Stuecken ueber.
/// - Laeuft in der naechsten Szene dasselbe Stueck, spielt es einfach weiter.
/// - Szene ohne SceneMusic -> Musik blendet aus.
/// - Waehrend eines Anrufs leiser (SetDucked).
/// - Optional: Pause nach dem Ende des Stuecks, dann wieder von vorne.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Tooltip("Gesamtlautstaerke der Musik")]
    [Range(0f, 1f)] public float masterVolume = 0.5f;

    [Header("Waehrend eines Anrufs")]
    [Tooltip("So laut ist die Musik, waehrend Ryan anruft (0.3 = 30 %)")]
    [Range(0f, 1f)] public float duckedVolume = 0.3f;
    [Tooltip("Sekunden fuer das Leiser- und wieder Lauterwerden")]
    public float duckFadeTime = 0.6f;

    private AudioSource sourceA, sourceB, current;
    private readonly float[] baseVolume = new float[2];   // Lautstaerke ohne Anruf-Daempfung
    private float duck = 1f, duckTarget = 1f;
    private float pauseAfterTrack;                         // 0 = nahtlos loopen
    private float restartAt = -1f;
    private Coroutine fadeRoutine, resumeRoutine;

    public static MusicManager Get()
    {
        if (Instance == null)
            new GameObject("MusicManager").AddComponent<MusicManager>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sourceA = CreateSource();
        sourceB = CreateSource();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private AudioSource CreateSource()
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.loop = true;
        s.playOnAwake = false;
        s.spatialBlend = 0f;
        s.volume = 0f;
        return s;
    }

    private int Idx(AudioSource s) => s == sourceA ? 0 : 1;

    private void Update()
    {
        if (sourceA == null) return;   // Duplikat, wird gerade geloescht

        // Anruf: weich leiser bzw. lauter
        duck = Mathf.MoveTowards(duck, duckTarget, Time.unscaledDeltaTime / Mathf.Max(0.01f, duckFadeTime));
        sourceA.volume = baseVolume[0] * duck;
        sourceB.volume = baseVolume[1] * duck;

        // Stueck zu Ende -> kurz Pause -> wieder von vorne
        if (current == null || current.loop) { restartAt = -1f; return; }
        if (current.isPlaying) return;

        if (restartAt < 0f)
            restartAt = Time.unscaledTime + pauseAfterTrack;
        else if (Time.unscaledTime >= restartAt)
        {
            restartAt = -1f;
            current.Play();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (resumeRoutine != null) { StopCoroutine(resumeRoutine); resumeRoutine = null; }
        if (FindFirstObjectByType<SceneMusic>() == null) Stop(1.5f);
    }

    /// <summary>
    /// Stueck abspielen (weich ueberblenden). Gleiches Stueck laeuft einfach weiter.
    /// pauseAfter: Sekunden Stille nach dem Ende, dann wieder von vorne (0 = nahtlos loopen).
    /// </summary>
    public void Play(AudioClip clip, float fadeTime = 1.5f, float volume = 1f, float pauseAfter = 0f)
    {
        if (clip == null) { Stop(fadeTime); return; }
        if (resumeRoutine != null) { StopCoroutine(resumeRoutine); resumeRoutine = null; }

        float target = masterVolume * volume;
        pauseAfterTrack = Mathf.Max(0f, pauseAfter);

        // Laeuft schon (oder macht gerade Pause) -> nur Lautstaerke anpassen
        if (current != null && current.clip == clip && (current.isPlaying || restartAt >= 0f))
        {
            current.loop = pauseAfterTrack <= 0f;
            StartFade(current, target, fadeTime);
            return;
        }

        AudioSource next = current == sourceA ? sourceB : sourceA;
        next.clip = clip;
        next.loop = pauseAfterTrack <= 0f;
        baseVolume[Idx(next)] = 0f;
        next.Play();
        current = next;
        restartAt = -1f;
        StartFade(next, target, fadeTime);
    }

    /// <summary>Musik ausblenden.</summary>
    public void Stop(float fadeTime = 1.5f)
    {
        current = null;
        restartAt = -1f;
        StartFade(null, 0f, fadeTime);
    }

    /// <summary>Waehrend eines Anrufs leiser (true) bzw. wieder normal (false).</summary>
    public void SetDucked(bool ducked) => duckTarget = ducked ? duckedVolume : 1f;

    /// <summary>Nach einer Pause (z.B. Sieg-Fanfare) wieder die Musik der Szene spielen.</summary>
    public void ResumeSceneMusic(float delay)
    {
        if (resumeRoutine != null) StopCoroutine(resumeRoutine);
        resumeRoutine = StartCoroutine(ResumeAfter(delay));
    }

    private IEnumerator ResumeAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        resumeRoutine = null;
        SceneMusic sceneMusic = FindFirstObjectByType<SceneMusic>();
        if (sceneMusic != null) sceneMusic.PlayNow(2f);
    }

    private void StartFade(AudioSource to, float target, float time)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(to, target, time));
    }

    // 'to' auf target, die andere Quelle auf 0 (laeuft auch bei Zeitstopp, z.B. beim Tod)
    private IEnumerator Fade(AudioSource to, float target, float time)
    {
        float[] start = { baseVolume[0], baseVolume[1] };
        int toIdx = to == null ? -1 : Idx(to);

        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            float k = time > 0f ? t / time : 1f;
            for (int i = 0; i < 2; i++)
                baseVolume[i] = Mathf.Lerp(start[i], i == toIdx ? target : 0f, k);
            yield return null;
        }

        for (int i = 0; i < 2; i++)
        {
            baseVolume[i] = i == toIdx ? target : 0f;
            if (i != toIdx) (i == 0 ? sourceA : sourceB).Stop();
        }
        fadeRoutine = null;
    }
}