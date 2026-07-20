using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Muss mit dem targetSpawnID der Tür übereinstimmen, die hierher führt")]
    public string spawnID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}