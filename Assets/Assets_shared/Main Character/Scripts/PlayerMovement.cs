using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;

    private float weightSpeedMultiplier = 1f;
    private float agilitySpeedMultiplier = 1f;   // NEU
    private Vector2 direction;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isDashing = false;

    public Vector2 Direction => direction;

    private float TotalSpeedMultiplier => weightSpeedMultiplier * agilitySpeedMultiplier;   // NEU

    void Start()
    {
        currentSpeed = movementSpeed;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        direction = new Vector2(inputX, inputY).normalized;
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

    public void SetDashState(bool dashing, float dashSpeed)
    {
        isDashing = dashing;
        currentSpeed = dashing ? dashSpeed * TotalSpeedMultiplier : movementSpeed * TotalSpeedMultiplier;
    }

    public void SetWeightPenalty(float multiplier)
    {
        weightSpeedMultiplier = multiplier;

        if (!isDashing)
            currentSpeed = movementSpeed * TotalSpeedMultiplier;
    }

    public void SetAgilityBonus(float multiplier)   // NEU
    {
        agilitySpeedMultiplier = multiplier;

        if (!isDashing)
            currentSpeed = movementSpeed * TotalSpeedMultiplier;
    }
}