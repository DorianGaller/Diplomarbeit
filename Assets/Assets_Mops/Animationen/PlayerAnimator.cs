using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    private Animator animator;

    [Header("Anti-Stutter")]
    [Tooltip("Wie lange keine Eingabe da sein muss, bevor wirklich auf Idle gewechselt wird")]
    [SerializeField] private float stopDelay = 0.08f;

    private float stopTimer = 0f;
    private bool isMovingState = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        bool hasInput = playerMovement.Direction != Vector2.zero;

        if (hasInput)
        {
            stopTimer = 0f;

            if (!isMovingState)
            {
                isMovingState = true;
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            stopTimer += Time.deltaTime;

            if (isMovingState && stopTimer >= stopDelay)
            {
                isMovingState = false;
                animator.SetBool("IsMoving", false);
            }
        }

        if (playerMovement.Direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (playerMovement.Direction.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }
}