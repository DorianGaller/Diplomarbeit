using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    void Update()
    {
        if (player == null) return;

        // Richtung berechnen
        Vector3 direction = (player.position - transform.position).normalized;

        // Bewegung
        transform.position += direction * speed * Time.deltaTime;
    }
}
