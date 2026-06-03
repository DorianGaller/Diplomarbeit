using UnityEngine;

public class TableProximityTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Das InfoText GameObject (child des Table)")]
    public GameObject infoText;

    [Header("Settings")]
    [Tooltip("Radius, ab dem der InfoText eingeblendet wird")]
    public float activationRadius = 2f;

    private Transform playerTransform;

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

        if (distance <= activationRadius)
        {
            infoText.SetActive(true);
        }
        else
        {
            infoText.SetActive(false);
        }
    }

    // Radius im Scene-View visualisieren
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}