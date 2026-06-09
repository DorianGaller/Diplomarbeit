using UnityEngine;

public class TableProximityTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject infoText;

    [Header("Canvas References")]
    public GameObject canvasToClose;
    public GameObject canvasToOpen;

    [Header("Settings")]
    public float activationRadius = 2f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool isInRange = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody holen

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("TableProximityTrigger: Kein Player gefunden!");

        if (infoText != null)
            infoText.SetActive(false);
    }

    void FixedUpdate() // ← FixedUpdate statt Update
    {
        if (playerTransform == null || infoText == null) return;

        // Rigidbody-Position statt transform.position
        Vector2 tablePos = rb != null ? rb.position : (Vector2)transform.position;
        float distance = Vector2.Distance(tablePos, (Vector2)playerTransform.position);

        bool playerNearby = distance <= activationRadius;

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

        if (canvasToClose != null)
            canvasToClose.SetActive(false);

        if (canvasToOpen != null)
            canvasToOpen.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}