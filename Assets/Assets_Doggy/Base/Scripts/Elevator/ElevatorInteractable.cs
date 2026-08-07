using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Elevator: Radius-Check auf den Spieler, Prompt einblenden, mit F das
/// Auswahl-Panel oeffnen, per Button eine neue Szene laden.
/// Gleiches Muster wie StorageInteractable / PC-Terminal.
/// </summary>
public class ElevatorInteractable : MonoBehaviour
{
    [Header("Interaktion")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";

    [Header("UI Referenzen")]
    [SerializeField] private GameObject interactionPrompt;   // "F zum Interagieren"
    [SerializeField] private GameObject elevatorPanel;       // Auswahl-UI

    [Header("Ziel")]
    [SerializeField] private string levelSceneName = "Level_01";

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Transform player;
    private bool playerInRange;
    private bool isPanelOpen;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[Elevator] Kein GameObject mit Tag '{playerTag}' gefunden!");
        }

        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();
        HandleInput();
    }

    /// <summary>Prueft ob der Spieler in Reichweite ist.</summary>
    void CheckPlayerDistance()
    {
        bool wasInRange = playerInRange;
        playerInRange = Vector2.Distance(transform.position, player.position) <= interactionRadius;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(playerInRange && !isPanelOpen);
        }

        // Weglaufen schliesst das Panel
        if (isPanelOpen && !playerInRange)
        {
            ClosePanel();
        }

        if (showDebugInfo && playerInRange != wasInRange)
        {
            Debug.Log($"[Elevator] Player in range: {playerInRange}");
        }
    }

    void HandleInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            TogglePanel();
        }

        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    void TogglePanel()
    {
        if (isPanelOpen) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        if (elevatorPanel == null)
        {
            Debug.LogWarning("[Elevator] Kein Elevator Panel zugewiesen!");
            return;
        }

        elevatorPanel.SetActive(true);
        isPanelOpen = true;

        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    /// <summary>Wird auch vom "Verlassen"-Button aufgerufen.</summary>
    public void ClosePanel()
    {
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
        isPanelOpen = false;

        if (interactionPrompt != null) interactionPrompt.SetActive(playerInRange);
    }

    /// <summary>Button "Level starten" haengt hier drauf.</summary>
    public void LoadLevel()
    {
        LoadScene(levelSceneName);
    }

    /// <summary>
    /// Fuer weitere Ziel-Buttons: Szenennamen direkt im Button-OnClick eintragen,
    /// dann braucht es kein neues Script pro Ziel.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[Elevator] Kein Szenenname gesetzt!");
            return;
        }

        FightRoomProgress.RegisterElevatorEntry(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}