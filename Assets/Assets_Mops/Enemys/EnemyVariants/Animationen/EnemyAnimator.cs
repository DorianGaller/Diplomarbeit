using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private EnemyFollow follow;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Anti-Flicker")]
    [Tooltip("Wie lange der gegenteilige Zustand anhalten muss, bevor wirklich umgeschaltet wird")]
    [SerializeField] private float stateChangeDelay = 0.1f;

    private Animator animator;
    private Transform player;

    private bool currentIsMoving = false;
    private float stateTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (follow == null) follow = GetComponent<EnemyFollow>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        bool rawIsMoving = false;
        if (player != null && follow != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            float stopDistance = follow.shootRange - follow.stopOffset;
            rawIsMoving = distance > stopDistance;
        }

        // NEU: Puffer, damit ein einzelner Grenzwert-Frame nicht sofort den State wechselt
        if (rawIsMoving != currentIsMoving)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= stateChangeDelay)
            {
                currentIsMoving = rawIsMoving;
                stateTimer = 0f;
                animator.SetBool("IsMoving", currentIsMoving);
            }
        }
        else
        {
            stateTimer = 0f;
        }

        if (player != null && spriteRenderer != null)
        {
            Vector3 scale = transform.localScale;
            float dirX = player.position.x - transform.position.x;
            if (Mathf.Abs(dirX) > 0.05f)
                scale.x = Mathf.Abs(scale.x) * (dirX < 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }
}