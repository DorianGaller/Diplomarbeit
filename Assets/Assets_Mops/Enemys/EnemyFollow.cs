using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

void Start()
{
    if (player == null)
        player = GameObject.FindGameObjectWithTag("Player").transform;
}

    void Update()
    {
        if (player == null) return;

        // Richtung berechnen
        Vector3 direction = (player.position - transform.position).normalized;

        // Bewegung
        transform.position += direction * speed * Time.deltaTime;
    }
}
