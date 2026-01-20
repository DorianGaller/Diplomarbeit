using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    // 🔹 Ghost Settings
    public GameObject dashGhostPrefab;
    public float ghostSpawnRate = 0.05f;

    private bool isDashing = false;
    private bool canDash = true;

    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDash && movement.Direction != Vector2.zero)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        movement.SetDashState(true, dashSpeed);

        StartCoroutine(SpawnGhosts());

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        movement.SetDashState(false, 0f);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator SpawnGhosts()
    {
        while (isDashing)
        {
            SpawnGhost();
            yield return new WaitForSeconds(ghostSpawnRate);
        }
    }

    void SpawnGhost()
{
    if (dashGhostPrefab == null) return; // 🔥 verhindert Crash

    GameObject ghost = Instantiate(
        dashGhostPrefab,
        transform.position,
        transform.rotation
    );

    SpriteRenderer ghostSR = ghost.GetComponent<SpriteRenderer>();
    SpriteRenderer playerSR = GetComponent<SpriteRenderer>();

    if (ghostSR && playerSR)
    {
        ghostSR.sprite = playerSR.sprite;
        ghostSR.flipX = playerSR.flipX;
        ghostSR.flipY = playerSR.flipY;

        Color c = ghostSR.color;
        c.a = 0.5f;
        ghostSR.color = c;
    }
}

}