using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Zeigt am Anfang vom Level Tutorial-Texte unten am Bildschirm.
/// Der Spieler ist gesperrt, bis alle Texte weggeklickt sind.
/// Wird nur einmal pro Spielstart angezeigt.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Texte")]
    [TextArea(2, 5)]
    [SerializeField] private string[] messages =
    {
        "You can move tables! Stand next to a table and press G to pick it up. Press G again to put it down.",
        "Press T next to a table to climb on it. Press T again to climb down. Use tables to reach vents!",
        "Stand next to a vent and press F to open it. Press F again to crawl inside.",
        "Some doors are locked. Find the keycard, equip it in your main hand and press F at the door."
    };

    [Header("Steuerung")]
    [SerializeField] private KeyCode nextKey = KeyCode.Return;

    /// <summary>True solange das Tutorial laeuft (andere Skripte koennen damit pausieren).</summary>
    public static bool IsRunning { get; private set; }

    // Bleibt gesetzt bis das Spiel neu gestartet wird
    private static bool hasShown = false;

    private PlayerMovement playerMovement;
    private InteractKeys interactKeys;
    private int index;

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (hasShown) return;

        StartCoroutine(StartNextFrame());
    }

    // Wartet einen Frame, damit DontDestroy-Duplikate bereinigt sind
    private IEnumerator StartNextFrame()
    {
        yield return null;

        if (messages == null || messages.Length == 0 || tutorialPanel == null || tutorialText == null)
        {
            Debug.LogWarning("TutorialManager: Panel, Text oder Messages fehlen!");
            yield break;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            interactKeys = playerObj.GetComponent<InteractKeys>();

            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        hasShown = true;
        IsRunning = true;
        SetPlayerLocked(true);

        index = 0;
        tutorialText.text = messages[index];
        tutorialPanel.SetActive(true);
    }

    private void Update()
    {
        if (!IsRunning || !Input.GetKeyDown(nextKey)) return;

        index++;

        if (index >= messages.Length)
        {
            EndTutorial();
            return;
        }

        tutorialText.text = messages[index];
    }

    private void EndTutorial()
    {
        IsRunning = false;
        tutorialPanel.SetActive(false);
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

    private void OnDestroy()
    {
        // Falls die Szene mitten im Tutorial gewechselt wird
        if (IsRunning)
        {
            IsRunning = false;
            SetPlayerLocked(false);
        }
    }
}