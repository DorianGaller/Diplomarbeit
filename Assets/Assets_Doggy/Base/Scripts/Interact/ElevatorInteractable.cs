using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Elevator-spezifisches Interactable: oeffnet/schliesst ein Auswahl-Panel statt
/// direkt ein Event zu feuern, laedt beim Bestaetigen eine neue Szene.
/// Erbt Radius-Check, Prompt-Handling und Input von InteractableBase.
/// </summary>
public class ElevatorInteractable : InteractableBase
{
    [Header("UI Referenzen")]
    [SerializeField] private GameObject elevatorPanel;

    [Header("Ziel")]
    [SerializeField] private string levelSceneName = "Level_01";

    private bool isPanelOpen;

    protected override void Start()
    {
        base.Start();
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
    }

    protected override bool ShouldShowPrompt() => !isPanelOpen;

    protected override void OnInteractPressed() => TogglePanel();

    protected override void Update()
    {
        base.Update();

        if (isPanelOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();

        if (isPanelOpen && !playerInRange)
            ClosePanel();
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
        RefreshPrompt();
    }

    public void ClosePanel()
    {
        if (elevatorPanel != null) elevatorPanel.SetActive(false);
        isPanelOpen = false;
        RefreshPrompt();
    }

    public void LoadLevel() => LoadScene(levelSceneName);

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[Elevator] Kein Szenenname gesetzt!");
            return;
        }

        DoorTransition.nextSpawnID = sceneName;
        FightRoomProgress.RegisterElevatorEntry(sceneName);
        SceneManager.LoadScene(sceneName);
    }
}