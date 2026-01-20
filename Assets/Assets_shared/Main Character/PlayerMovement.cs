using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float bounds = 5f;

    private Vector2 direction;
    private float currentSpeed;

    public Vector2 Direction => direction;

    void Start()
    {
        currentSpeed = movementSpeed;
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        direction = new Vector2(inputX, inputY).normalized;

        if (direction != Vector2.zero)
        {
            Vector3 newPos = transform.position + (Vector3)(direction * currentSpeed * Time.deltaTime);

            newPos.x = Mathf.Clamp(newPos.x, -bounds, bounds);
            newPos.y = Mathf.Clamp(newPos.y, -bounds, bounds);

            transform.position = newPos;
        }
    }

    // 🔹 Wird vom Dash-Skript gesteuert
    public void SetDashState(bool dashing, float dashSpeed)
    {
        currentSpeed = dashing ? dashSpeed : movementSpeed;
    }
}
