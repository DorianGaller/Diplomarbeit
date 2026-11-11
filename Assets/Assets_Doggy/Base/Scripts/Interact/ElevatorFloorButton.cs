using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kommt auf einen Stockwerk-Button im Elevator-Panel.
/// Solange das Stockwerk nicht freigeschaltet ist (Boss-Exit vom Stock davor betreten),
/// ist der Button grau, nicht klickbar und zeigt den Locked-Text.
/// </summary>
[RequireComponent(typeof(Button))]
public class ElevatorFloorButton : MonoBehaviour
{
    [Tooltip("Muss genau mit 'Unlock Floor On Enter' am Boss-Exit übereinstimmen")]
    [SerializeField] private string floorId = "2. Stock";

    [Header("Beschriftung")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private string unlockedText = "Level 2";
    [SerializeField] private string lockedText = "LOCKED";
    [SerializeField] private Color lockedTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private Button button;
    private Color unlockedTextColor;

    void Awake()
    {
        button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (label != null) unlockedTextColor = label.color;
    }

    // Läuft jedes Mal, wenn das Elevator-Panel geöffnet wird
    void OnEnable() => Refresh();

    public void Refresh()
    {
        bool unlocked = SaveSystem.IsFloorUnlocked(floorId);

        button.interactable = unlocked;

        if (label != null)
        {
            label.text = unlocked ? unlockedText : lockedText;
            label.color = unlocked ? unlockedTextColor : lockedTextColor;
        }
    }
}