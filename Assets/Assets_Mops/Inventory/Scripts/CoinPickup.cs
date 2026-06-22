using UnityEngine;
public class CoinPickup : MonoBehaviour
{
public int value = 1;
private void OnTriggerEnter2D(Collider2D other)
    {
if (other.CompareTag("Player"))
        {
PlayerStats stats = GameObject.Find("StatManager").GetComponent<PlayerStats>();
stats.AddCoins(value);
Destroy(gameObject);
        }
    }
}