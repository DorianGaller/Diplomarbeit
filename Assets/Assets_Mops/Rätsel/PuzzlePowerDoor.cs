using UnityEngine;

/// <summary>
/// Tuer im Raetselraum mit Stromanzeige (3 Lampen).
/// Jede geloeste Station schaltet eine Lampe an. Sind alle geloest, geht die Tuer auf.
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

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lampClip;
    [SerializeField] private AudioClip openClip;

    private bool isOpen;

    private void Start()
    {
        foreach (PuzzleStation s in stations)
            if (s != null) s.OnSolved += _ => Refresh(true);

        Refresh(false);
    }

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