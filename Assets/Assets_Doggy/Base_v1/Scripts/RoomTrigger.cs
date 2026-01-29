using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Vector3 cameraPosition; // Wo soll die Kamera hin?
    public CameraController cameraController;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.MoveToCameraPosition(cameraPosition);
        }
    }
}