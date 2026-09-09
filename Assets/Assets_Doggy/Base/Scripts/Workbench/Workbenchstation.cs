using UnityEngine;

/// <summary>
/// Steuert das Workbench-Panel. Die Interaktion selbst (Reichweite, F-Taste,
/// Prompt-Text) uebernimmt die vorhandene Interactable-Komponente auf
/// demselben GameObject — Toggle() gehoert dort ins onInteract-Event.
///
/// Setup in der Hierarchy:
///   Workbench (GameObject)
///     - Interactable (bestehend, Prompt Text z.B. "Press F to Interact")
///         On Interact() -> Objekt: Workbench, Funktion: WorkbenchStation.Toggle
///     - WorkbenchStation (dieses Skript)
///
/// Close-Button im Canvas:
///     On Click() -> Objekt: Workbench, Funktion: WorkbenchStation.CloseFromButton
/// </summary>
public class WorkbenchStation : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Wurzel-GameObject des Workbench-Canvas.")]
    [SerializeField] private GameObject workbenchRoot;

    [Tooltip("Das normale Player-UI (Health/Coins/Level/...), das waehrend " +
             "der Workbench ausgeblendet werden soll. Optional.")]
    [SerializeField] private GameObject playerUIRoot;

    [Header("Optional")]
    [Tooltip("Bewegungsskript des Spielers, wird waehrend der Benutzung deaktiviert.")]
    [SerializeField] private MonoBehaviour playerMovementToDisable;

    [Tooltip("Eigene InteractKeys-Referenz, damit Schiessen waehrend des Panels gesperrt wird. " +
             "Leer lassen, wird automatisch am Spieler gesucht.")]
    [SerializeField] private InteractKeys playerInteractKeys;

    public bool IsOpen { get; private set; }

    private void Start()
    {
        // Zustand und GameObject muessen von Anfang an uebereinstimmen,
        // sonst geht der erste Tastendruck ins Leere.
        IsOpen = false;

        if (workbenchRoot != null)
            workbenchRoot.SetActive(false);

        if (playerInteractKeys == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerInteractKeys = playerObj.GetComponent<InteractKeys>();
        }
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    /// <summary>Ins onInteract-Event der Interactable-Komponente eintragen.</summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (workbenchRoot == null)
        {
            Debug.LogError("[Workbench] Kein workbenchRoot zugewiesen.");
            return;
        }

        IsOpen = true;
        workbenchRoot.SetActive(true);

        if (playerUIRoot != null)
            playerUIRoot.SetActive(false);

        if (playerMovementToDisable != null)
            playerMovementToDisable.enabled = false;

        if (playerInteractKeys != null)
            playerInteractKeys.EnableShooting(false);
    }

    public void Close()
    {
        IsOpen = false;

        if (workbenchRoot != null)
            workbenchRoot.SetActive(false);

        if (playerUIRoot != null)
            playerUIRoot.SetActive(true);

        if (playerMovementToDisable != null)
            playerMovementToDisable.enabled = true;

        if (playerInteractKeys != null)
            playerInteractKeys.EnableShooting(true);
    }

    /// <summary>Fuer einen Schliessen-Button im Canvas.</summary>
    public void CloseFromButton() => Close();
}