using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement Detection")]
    [Tooltip("Mindestgeschwindigkeit (Einheiten/Sekunde), ab der IsMoving = true gesetzt wird")]
    [SerializeField] private float movementThreshold = 0.05f;

    [Header("Anti-Flicker")]
    [SerializeField] private float stateChangeDelay = 0.1f;

    [Header("Shoot Animation")]
    [SerializeField] private string shootClipName = "Robot_Shooting";
    private float shootAnimationDuration = 0.5f;

    private Animator animator;
    private Transform player;

    private Vector3 lastPosition;
    private bool currentIsMoving = false;
    private float stateTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        lastPosition = transform.position;
        CacheShootAnimationDuration();
    }

    void CacheShootAnimationDuration()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == shootClipName)
            {
                shootAnimationDuration = clip.length;
                break;
            }
        }
    }

    void Update()
    {
        float speed = Vector3.Distance(transform.position, lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = transform.position;

        bool rawIsMoving = speed > movementThreshold;

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

        // Flip-Logik entfernt — übernehmen jetzt EnemyPatrol/EnemyChase selbst
    }

    public float GetShootAnimationDuration() => shootAnimationDuration;

    public void TriggerShoot()
    {
        if (animator == null) return;

        animator.SetTrigger("Shoot");
        animator.SetBool("IsShooting", true);

        StopCoroutine(nameof(ClearShootingFlagAfterDelay));
        StartCoroutine(ClearShootingFlagAfterDelay());
    }

    IEnumerator ClearShootingFlagAfterDelay()
    {
        yield return new WaitForSeconds(shootAnimationDuration);
        animator.SetBool("IsShooting", false);
    }
}