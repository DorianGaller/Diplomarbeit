using UnityEngine;

[DefaultExecutionOrder(-100)]   // läuft VOR allen anderen Start()-Methoden
public class PlayerSpawner : MonoBehaviour
{
    private void Awake()
    {
        if (string.IsNullOrEmpty(DoorTransition.nextSpawnID))
            return; // keine Ziel-ID gesetzt -> Spieler bleibt wo er ist (z.B. beim ersten Szenenstart)

        SpawnPoint[] allSpawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        SpawnPoint target = null;

        foreach (SpawnPoint sp in allSpawnPoints)
        {
            if (sp.spawnID == DoorTransition.nextSpawnID)
            {
                target = sp;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("PlayerSpawner: Kein SpawnPoint mit ID '" + DoorTransition.nextSpawnID + "' gefunden!");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = target.transform.position;

        DoorTransition.nextSpawnID = ""; // zurücksetzen, damit es nicht beim nächsten Szenenwechsel stört
    }
}