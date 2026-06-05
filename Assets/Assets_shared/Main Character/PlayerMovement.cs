using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;

    private Vector2 direction;
    private float currentSpeed;
    private Rigidbody2D rb;

    public Vector2 Direction => direction;

    void Start()
    {
        currentSpeed = movementSpeed;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
{
    float inputX = Input.GetAxisRaw("Horizontal");
    float inputY = Input.GetAxisRaw("Vertical");

    direction = new Vector2(inputX, inputY).normalized;

    // Sicherheit: explizit auf zero wenn keine Eingabe
    if (inputX == 0 && inputY == 0)
        direction = Vector2.zero;
}

    void FixedUpdate()
    {
        if (direction != Vector2.zero)
        {
            Vector2 newPos = rb.position + direction * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    // 🔹 Wird vom Dash-Skript gesteuert
    public void SetDashState(bool dashing, float dashSpeed)
    {
        currentSpeed = dashing ? dashSpeed : movementSpeed;
    }
}