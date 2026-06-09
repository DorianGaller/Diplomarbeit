using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 👈 für Image

public class PlayerDash : MonoBehaviour
{
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    // 🔹 Ghost Settings
    public GameObject dashGhostPrefab;
    public float ghostSpawnRate = 0.05f;

    // 🔹 UI
    public Image dashCooldownImage; // Fill-Image im UI (z.B. Kreis/Balken)

    private bool isDashing = false;
    private bool canDash = true;

    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();

        // Startzustand: Dash bereit = voll
        if (dashCooldownImage != null)
            dashCooldownImage.fillAmount = 1f;
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

        // UI sofort leeren
        if (dashCooldownImage != null)
            dashCooldownImage.fillAmount = 0f;

        movement.SetDashState(true, dashSpeed);

        StartCoroutine(SpawnGhosts());

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        movement.SetDashState(false, 0f);

        // Cooldown visuell auffüllen
        float timer = 0f;
        while (timer < dashCooldown)
        {
            timer += Time.deltaTime;

            if (dashCooldownImage != null)
                dashCooldownImage.fillAmount = timer / dashCooldown;

            yield return null;
        }

        // Sicherheitshalber auf voll setzen
        if (dashCooldownImage != null)
            dashCooldownImage.fillAmount = 1f;

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
        if (dashGhostPrefab == null) return;

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
