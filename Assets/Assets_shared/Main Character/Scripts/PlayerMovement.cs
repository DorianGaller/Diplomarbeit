using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;

    private float weightSpeedMultiplier = 1f;   // NEU
    private Vector2 direction;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isDashing = false;   // NEU

    public Vector2 Direction => direction;

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
        isDashing = dashing;   // NEU
        currentSpeed = dashing ? dashSpeed * weightSpeedMultiplier : movementSpeed * weightSpeedMultiplier;   // NEU: Gewichts-Multiplikator greift bei beidem
    }

    // NEU: wird von PlayerStats aufgerufen, sobald sich das Ausrüstungsgewicht ändert
    public void SetWeightPenalty(float multiplier)
    {
        weightSpeedMultiplier = multiplier;

        if (!isDashing)
            currentSpeed = movementSpeed * weightSpeedMultiplier;
    }
}