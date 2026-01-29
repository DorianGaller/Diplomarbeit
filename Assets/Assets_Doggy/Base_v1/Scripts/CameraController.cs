using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public bool followPlayer = true;
    public float smoothSpeed = 5f;
    
    private Vector3 targetPosition;
    private float fixedZ; // Z-Position beibehalten

    void Start()
    {
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (followPlayer)
        {
            targetPosition = new Vector3(player.position.x, player.position.y, fixedZ);
        }
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void MoveToCameraPosition(Vector3 newPosition)
    {
        followPlayer = false;
        targetPosition = new Vector3(newPosition.x, newPosition.y, fixedZ);
    }

    public void FollowPlayer()
    {
        followPlayer = true;
    }
}