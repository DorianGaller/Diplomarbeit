using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Assign the main character's Transform (used as reference for parallax).")]
    public Transform player;

    [Header("Parallax Objects")]
    [Tooltip("Near object (e.g. window). Will move only horizontally by default.")]
    public Transform nearObject;

    [Tooltip("Far object (background/backdrop).")]
    public Transform farObject;

    // Parallax factors are script-controlled (not editable in Inspector)
    private readonly Vector2 nearParallax = new Vector2(0.1f, 0.015f);
    private readonly Vector2 farParallax = new Vector2(0.025f, 0.07f);

    // Smoothing (script-controlled)
    private readonly float smoothSpeed = 3f;

    private Vector3 playerStartPos;
    private Vector3 nearStartPos;
    private Vector3 farStartPos;

    void Start()
    {
        // auto-find player if not assigned
        if (player == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }

        if (player == null && Camera.main != null)
            player = Camera.main.transform;

        CaptureInitialPositions();
    }

    void LateUpdate()
    {
        if (player == null) return;

        float dt = Time.deltaTime;
        Vector3 playerOffset = player.position - playerStartPos;

        // Near: X and Y movement (opposite direction like parallax)
        if (nearObject != null)
        {
            Vector3 target = nearStartPos + new Vector3(-playerOffset.x * nearParallax.x, -playerOffset.y * nearParallax.y, 0f);
            float t = 1f - Mathf.Exp(-smoothSpeed * dt);
            Vector3 newPos = Vector3.Lerp(nearObject.position, target, t);
            nearObject.position = newPos;
        }

        // Far: tiny X movement and opposite-direction Y movement for parallax
        if (farObject != null)
        {
            Vector3 target = farStartPos + new Vector3(-playerOffset.x * farParallax.x, -playerOffset.y * farParallax.y, 0f);
            float t = 1f - Mathf.Exp(-smoothSpeed * dt);
            Vector3 newPos = Vector3.Lerp(farObject.position, target, t);
            farObject.position = newPos;
        }
    }

    // Re-capture initial positions at runtime
    public void CaptureInitialPositions()
    {
        playerStartPos = player != null ? player.position : Vector3.zero;
        nearStartPos = nearObject != null ? nearObject.position : Vector3.zero;
        farStartPos = farObject != null ? farObject.position : Vector3.zero;
    }
}
