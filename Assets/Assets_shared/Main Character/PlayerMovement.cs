using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    public Vector2 direction;

    public float bounds = 5f;

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        if (inputX != 0 || inputY != 0)
        {
            direction = new Vector2(inputX, inputY).normalized;
        }

        Vector3 newPos = transform.position + (Vector3)(direction * movementSpeed * Time.deltaTime);

        // 🔹 X-Bounds
        if (newPos.x > bounds)
        {
            newPos.x = bounds;
            direction.x *= -1;
        }
        else if (newPos.x < -bounds)
        {
            newPos.x = -bounds;
            direction.x *= -1;
        }

        // 🔹 Y-Bounds
        if (newPos.y > bounds)
        {
            newPos.y = bounds;
            direction.y *= -1;
        }
        else if (newPos.y < -bounds)
        {
            newPos.y = -bounds;
            direction.y *= -1;
        }

        transform.position = newPos;
    }
}
