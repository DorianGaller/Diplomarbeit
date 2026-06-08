using UnityEngine;

public class TableProximityTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Das InfoText GameObject (child des Table)")]
    public GameObject infoText;

    [Header("Canvas References")]
    [Tooltip("Canvas der geschlossen wird, wenn der Spieler weit genug weg ist")]
    public GameObject canvasToClose;

    [Tooltip("Canvas der geöffnet wird, wenn der Spieler weit genug weg ist (optional)")]
    public GameObject canvasToOpen;

    [Header("Settings")]
    [Tooltip("Radius, ab dem der InfoText eingeblendet wird")]
    public float activationRadius = 2f;

    private Transform playerTransform;
    private bool isInRange = false;

    void Start()
    {
        // Player über Tag finden
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("TableProximityTrigger: Kein GameObject mit Tag 'Player' gefunden!");
        }

        // InfoText zu Beginn deaktivieren
        if (infoText != null)
            infoText.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null || infoText == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool playerNearby = distance <= activationRadius;

        // Nur bei Zustandsänderung reagieren (Performance-Optimierung)
        if (playerNearby && !isInRange)
        {
            isInRange = true;
            OnPlayerEnter();
        }
        else if (!playerNearby && isInRange)
        {
            isInRange = false;
            OnPlayerExit();
        }
    }

    void OnPlayerEnter()
    {
        if (infoText != null)
            infoText.SetActive(true);
    }

    void OnPlayerExit()
    {
        if (infoText != null)
            infoText.SetActive(false);

        // Canvas schließen
        if (canvasToClose != null)
            canvasToClose.SetActive(false);

        // Canvas öffnen (optional)
        if (canvasToOpen != null)
            canvasToOpen.SetActive(true);
    }

    // Radius im Scene-View visualisieren
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}