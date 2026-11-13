using UnityEngine;
using TMPro;

/// <summary>
/// Vom Hacker ausgeloest: vertauscht fuer kurze Zeit eine Bewegungsachse (W/S oder A/D).
/// Sitzt auf dem Main Character (neben PlayerMovement).
/// </summary>
public class PlayerControlHack : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("Hinweis im Player UI (z.B. Panel mit Text), nur waehrend eines Hacks sichtbar")]
    [SerializeField] private GameObject hackHud;
    [SerializeField] private TMP_Text hackText;
    [SerializeField] private string verticalText = "SYSTEM HACKED!\nW / S swapped";
    [SerializeField] private string horizontalText = "SYSTEM HACKED!\nA / D swapped";

    [Header("Schutz (z.B. Firewall-Modul)")]
    [Tooltip("1 = volle Dauer, 0.5 = halbe Dauer, 0 = immun")]
    [Range(0f, 1f)] public float hackDurationMultiplier = 1f;

    public bool InvertX { get; private set; }
    public bool InvertY { get; private set; }
    public bool IsHacked => timer > 0f;

    private float timer;
    private Object source;

    void Start()
    {
        if (hackHud != null) hackHud.SetActive(false);
    }

    /// <summary>vertical = true: W/S vertauscht, false: A/D vertauscht.</summary>
    public bool ApplyHack(bool vertical, float duration, Object from)
    {
        float d = duration * hackDurationMultiplier;
        if (d <= 0.05f) return false;   // geschuetzt

        InvertY = vertical;
        InvertX = !vertical;
        timer = d;
        source = from;

        if (hackText != null) hackText.text = vertical ? verticalText : horizontalText;
        if (hackHud != null) hackHud.SetActive(true);
        return true;
    }

    /// <summary>Nur aufheben, wenn genau dieser Hacker den Hack ausgeloest hat (z.B. weil er gestorben ist).</summary>
    public void ClearHackFrom(Object from)
    {
        if (IsHacked && source == from)
            ClearHack();
    }

    public void ClearHack()
    {
        timer = 0f;
        InvertX = false;
        InvertY = false;
        source = null;
        if (hackHud != null) hackHud.SetActive(false);
    }

    void Update()
    {
        if (timer <= 0f) return;

        timer -= Time.deltaTime;

        // in der letzten Sekunde blinken
        if (hackHud != null)
            hackHud.SetActive(timer > 1f || Mathf.Repeat(timer * 6f, 1f) < 0.6f);

        if (timer <= 0f)
            ClearHack();
    }
}