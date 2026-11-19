using System.Collections;
using UnityEngine;

/// <summary>
/// Tuer im Raetselraum mit Stromanzeige (3 Lampen).
/// Jede geloeste Station schaltet eine Lampe an. Sind alle geloest, geht die Tuer auf.
/// Nach jeder geloesten Station faehrt die Kamera kurz zur Stromanzeige, damit man sieht, was passiert.
/// Der Zustand kommt aus dem Spielstand der Stationen, die Tuer selbst muss nichts speichern.
/// </summary>
public class PuzzlePowerDoor : MonoBehaviour
{
    [SerializeField] private PuzzleStation[] stations;

    [Header("Stromanzeige")]
    [SerializeField] private SpriteRenderer lampRenderer;
    [Tooltip("Index = Anzahl geloester Stationen: 0 = alle aus ... 3 = alle an")]
    [SerializeField] private Sprite[] lampSprites;

    [Header("Tuer")]
    [Tooltip("Wird beim Oeffnen ausgeschaltet, z.B. geschlossene Tuer mit Collider")]
    [SerializeField] private GameObject[] disableWhenOpen;
    [Tooltip("Wird beim Oeffnen eingeschaltet, z.B. offene Tuer")]
    [SerializeField] private GameObject[] enableWhenOpen;

    [Header("Kamerafahrt nach jeder geloesten Station")]
    [SerializeField] private bool cameraPan = true;
    [Tooltip("Wohin die Kamera faehrt. Leer = zur Stromanzeige")]
    [SerializeField] private Transform panTarget;
    [Tooltip("Dauer der Fahrt hin bzw. zurueck (Sekunden)")]
    [SerializeField] private float panDuration = 0.8f;
    [Tooltip("So lange bleibt die Kamera bei der Anzeige. Wenn die Tuer aufgeht, etwas laenger")]
    [SerializeField] private float holdTime = 1.2f;
    [Tooltip("Zoom waehrend der Fahrt (Orthographic Size, Spiel-Kamera hat 8). 0 = nicht zoomen")]
    [SerializeField] private float panZoom = 6f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lampClip;
    [SerializeField] private AudioClip openClip;

    private bool isOpen;
    private bool panning;

    private void Start()
    {
        foreach (PuzzleStation s in stations)
            if (s != null) s.OnSolved += OnStationSolved;

        Refresh(false);
    }

    private void OnDestroy()
    {
        foreach (PuzzleStation s in stations)
            if (s != null) s.OnSolved -= OnStationSolved;
    }

    private void OnStationSolved(PuzzleStation station)
    {
        if (!cameraPan || Camera.main == null)
        {
            Refresh(true);
            return;
        }

        StartCoroutine(ShowSolved(station));
    }

    // ── Kamerafahrt ───────────────────────────────────────

    private IEnumerator ShowSolved(PuzzleStation station)
    {
        // Falls zwei Stationen kurz hintereinander geloest werden: nacheinander zeigen
        while (panning) yield return null;
        panning = true;

        // Erst warten, bis das Minigame-Fenster zu ist
        while (station != null && station.IsOpen) yield return null;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Refresh(true);
            panning = false;
            yield break;
        }

        Transform camT = cam.transform;
        Vector3 localStart = camT.localPosition;   // Kamera haengt am Spieler
        float startSize = cam.orthographicSize;
        Transform target = panTarget != null ? panTarget : lampRenderer != null ? lampRenderer.transform : transform;

        SetPlayerLocked(true);

        // Hin
        Vector3 from = camT.position;
        Vector3 to = new Vector3(target.position.x, target.position.y, from.z);
        float zoom = panZoom > 0f ? panZoom : startSize;
        yield return Move(cam, from, to, startSize, zoom);

        // Lampe an (bei der letzten geht die Tuer auf)
        Refresh(true);
        yield return new WaitForSeconds(isOpen ? holdTime + 0.8f : holdTime);

        // Zurueck zum Spieler
        Vector3 back = camT.parent != null ? camT.parent.TransformPoint(localStart) : from;
        yield return Move(cam, camT.position, back, cam.orthographicSize, startSize);
        camT.localPosition = localStart;
        cam.orthographicSize = startSize;

        SetPlayerLocked(false);
        panning = false;
    }

    private IEnumerator Move(Camera cam, Vector3 from, Vector3 to, float sizeFrom, float sizeTo)
    {
        float duration = Mathf.Max(0.01f, panDuration);
        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            float e = Mathf.SmoothStep(0f, 1f, t);
            cam.transform.position = Vector3.Lerp(from, to, e);
            if (cam.orthographic) cam.orthographicSize = Mathf.Lerp(sizeFrom, sizeTo, e);
            yield return null;
        }

        cam.transform.position = to;
        if (cam.orthographic) cam.orthographicSize = sizeTo;
    }

    private void SetPlayerLocked(bool locked)
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;

        PlayerMovement movement = p.GetComponentInChildren<PlayerMovement>();
        if (movement != null) movement.SetMovementInputLocked(locked);

        InteractKeys keys = p.GetComponentInChildren<InteractKeys>();
        if (keys != null) keys.EnableShooting(!locked);
    }

    // ── Anzeige und Tuer ──────────────────────────────────

    private void Refresh(bool withSound)
    {
        int solved = 0;
        foreach (PuzzleStation s in stations)
            if (s != null && s.IsSolved) solved++;

        if (lampRenderer != null && lampSprites != null && lampSprites.Length > 0)
            lampRenderer.sprite = lampSprites[Mathf.Clamp(solved, 0, lampSprites.Length - 1)];

        bool open = stations.Length > 0 && solved >= stations.Length;

        if (withSound && audioSource != null)
        {
            AudioClip clip = open && !isOpen ? openClip : lampClip;
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        if (open == isOpen && withSound) return;
        isOpen = open;

        foreach (GameObject go in disableWhenOpen)
            if (go != null) go.SetActive(!open);
        foreach (GameObject go in enableWhenOpen)
            if (go != null) go.SetActive(open);
    }
}