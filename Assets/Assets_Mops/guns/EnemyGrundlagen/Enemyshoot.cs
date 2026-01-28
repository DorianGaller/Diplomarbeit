using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public Transform player;
    public GameObject gunshotPrefab;
    public Transform shootPoint;
    
    public float shootRange = 10f; //ab wann der gegner schießt
    public float shootInterval = 2f; //timer wann schießen
    private float shootTimer;


void Start()
{
    if (player == null)
        player = GameObject.FindGameObjectWithTag("Player").transform;
}

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            if (Vector2.Distance(transform.position, player.position) <= shootRange)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
    }

    private void Shoot()
    {
        if (!player || !gunshotPrefab || !shootPoint) return;

        // Richtung zum Player
        Vector2 shootDir = (player.position - shootPoint.position).normalized;

        GameObject shot = Instantiate(
            gunshotPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        // Projektil initialisieren
        shot.GetComponent<GunshotEffect>()?.Init(shootDir);
    }
}
