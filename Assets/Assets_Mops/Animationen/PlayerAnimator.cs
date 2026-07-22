using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        bool isMoving = playerMovement.Direction != Vector2.zero;
        animator.SetBool("IsMoving", isMoving);

        if (playerMovement.Direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (playerMovement.Direction.x < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }
}