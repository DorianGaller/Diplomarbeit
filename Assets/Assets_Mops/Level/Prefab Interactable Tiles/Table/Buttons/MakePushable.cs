using UnityEngine;

public class MakePushable : MonoBehaviour
{
    [Header("Objekt zum Aufheben")]
    public GameObject targetObject;

    private Rigidbody2D rb;
    private bool isPickedUp = false;

    public void OnButtonClick()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Kein Zielobjekt gesetzt!");
            return;
        }

        rb = targetObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = targetObject.AddComponent<Rigidbody2D>();
        }

        // Rigidbody auf Kinematic, damit es direkt vom Player bewegt werden kann
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        // Collider sicherstellen
        BoxCollider2D collider = targetObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = targetObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = false;

        // Objekt an Player parenten
        targetObject.transform.SetParent(transform);
        targetObject.transform.localPosition = Vector3.up; // optional: über dem Player positionieren
        isPickedUp = true;

        Debug.Log(targetObject.name + " wurde aufgehoben!");
    }

    // Optional: Loslassen
    public void DropObject()
    {
        if (isPickedUp && targetObject != null)
        {
            targetObject.transform.SetParent(null);
            rb.bodyType = RigidbodyType2D.Dynamic; // wieder physisch korrekt
            isPickedUp = false;
            Debug.Log(targetObject.name + " wurde losgelassen!");
        }
    }
}
