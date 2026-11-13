using UnityEngine;

/// <summary>
/// Blendet ein HUD-Element (ESCredits, XP-Leiste) nur kurz ein, wenn sich etwas aendert.
/// Solange eines der "Keep Visible"-Objekte aktiv ist (z.B. Inventar), bleibt es sichtbar.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HudAutoHide : MonoBehaviour
{
    [Header("Timing (Sekunden)")]
    public float fadeInTime = 0.15f;
    public float holdTime = 2.5f;
    public float fadeOutTime = 0.6f;

    [Header("Immer sichtbar, solange eines davon offen ist")]
    public GameObject[] keepVisibleWhileActive;

    private CanvasGroup group;
    private float hideAt = -1f;   // ab diesem Zeitpunkt (unscaled) wird ausgeblendet

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    /// <summary>Kurz einblenden. Erneuter Aufruf verlaengert die Anzeige.</summary>
    public void Show()
    {
        hideAt = Time.unscaledTime + fadeInTime + holdTime;
    }

    void Update()
    {
        bool visible = IsForcedVisible() || Time.unscaledTime < hideAt;
        float duration = visible ? fadeInTime : fadeOutTime;
        group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f,
                                        Time.unscaledDeltaTime / Mathf.Max(0.01f, duration));
    }

    bool IsForcedVisible()
    {
        if (keepVisibleWhileActive == null) return false;
        foreach (GameObject go in keepVisibleWhileActive)
            if (go != null && go.activeInHierarchy) return true;
        return false;
    }
}