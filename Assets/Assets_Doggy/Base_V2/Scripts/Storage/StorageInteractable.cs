using UnityEngine;

/// <summary>
/// Macht das Lager interaktiv - Spieler kann mit E-Taste öffnen
/// Hänge dieses Script zusammen mit StorageContainer an dein Lager-GameObject
/// </summary>
[RequireComponent(typeof(StorageContainer))]
public class StorageInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Wie nah muss der Spieler sein?")]
    public float interactionRadius = 2f;
    
    [Tooltip("Welche Taste öffnet das Lager?")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Reference")]
    [Tooltip("Ziehe hier dein Storage UI Panel rein")]
    public GameObject storageUIPanel;
    
    [Header("Player Settings")]
    [Tooltip("Tag des Spielers (Standard: 'Player')")]
    public string playerTag = "Player";
    
    [Header("Visual Feedback (Optional)")]
    [Tooltip("Zeige 'Press E' Text wenn Spieler nah ist")]
    public GameObject interactionPrompt;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private StorageContainer storageContainer;
    private StorageUI storageUI;
    private Transform player;
    private bool playerInRange = false;
    private bool isStorageOpen = false;

    void Start()
    {
        // Hole Komponenten
        storageContainer = GetComponent<StorageContainer>();
        
        if (storageUIPanel != null)
        {
            storageUI = storageUIPanel.GetComponent<StorageUI>();
            storageUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[StorageInteractable] Kein Storage UI Panel zugewiesen!");
        }

        // Finde Spieler
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[StorageInteractable] Kein GameObject mit Tag '{playerTag}' gefunden!");
        }

        // Interaktions-Prompt verstecken
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();
        HandleInput();
    }

    /// <summary>
    /// Prüft ob Spieler in Reichweite ist
    /// </summary>
    void CheckPlayerDistance()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRadius;

        // Zeige/Verstecke Interaktions-Prompt
        if (interactionPrompt != null && !isStorageOpen)
        {
            interactionPrompt.SetActive(playerInRange);
        }

        // Debug Info
        if (showDebugInfo && playerInRange != wasInRange)
        {
            Debug.Log($"[StorageInteractable] Player in range: {playerInRange}");
        }
    }

    /// <summary>
    /// Verarbeitet Spieler Input
    /// </summary>
    void HandleInput()
    {
        // E-Taste zum Öffnen/Schließen
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleStorage();
        }

        // ESC zum Schließen
        if (isStorageOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseStorage();
        }
    }

    /// <summary>
    /// Öffnet/Schließt das Lager
    /// </summary>
    void ToggleStorage()
    {
        if (isStorageOpen)
        {
            CloseStorage();
        }
        else
        {
            OpenStorage();
        }
    }

    /// <summary>
    /// Öffnet das Lager
    /// </summary>
    void OpenStorage()
    {
        if (storageUIPanel == null)
        {
            Debug.LogWarning("[StorageInteractable] Kann UI nicht öffnen - kein Panel zugewiesen!");
            return;
        }

        isStorageOpen = true;
        storageUIPanel.SetActive(true);

        // Initialisiere UI mit diesem Storage Container
        if (storageUI != null)
        {
            storageUI.Initialize(storageContainer);
        }

        // Verstecke Interaktions-Prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        // Pause Spiel (optional - kannst du später anpassen)
        Time.timeScale = 0f;

        if (showDebugInfo)
            Debug.Log("[StorageInteractable] Storage opened");
    }

    /// <summary>
    /// Schließt das Lager
    /// </summary>
    void CloseStorage()
    {
        isStorageOpen = false;
        
        if (storageUIPanel != null)
        {
            storageUIPanel.SetActive(false);
        }

        // Resume Spiel
        Time.timeScale = 1f;

        // Zeige Prompt wieder wenn Spieler noch in Range ist
        if (interactionPrompt != null && playerInRange)
        {
            interactionPrompt.SetActive(true);
        }

        if (showDebugInfo)
            Debug.Log("[StorageInteractable] Storage closed");
    }

    /// <summary>
    /// Externe Systeme können das Lager damit öffnen
    /// </summary>
    public void OpenStorageExternal()
    {
        if (!isStorageOpen)
        {
            OpenStorage();
        }
    }

    public void CloseStorageExternal()
    {
        if (isStorageOpen)
        {
            CloseStorage();
        }
    }

    public StorageContainer GetStorageContainer()
    {
        return storageContainer;
    }

    public bool IsStorageOpen()
    {
        return isStorageOpen;
    }

    /// <summary>
    /// Visualisierung im Editor (Interaction Radius)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan mit Transparenz
        Gizmos.DrawSphere(transform.position, interactionRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
