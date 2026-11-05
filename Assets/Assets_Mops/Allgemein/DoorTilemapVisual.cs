using UnityEngine;

/// <summary>
/// Schaltet zwischen rotem und gruenem Tilemap-Tor um.
/// - Mit EnemySpawn: gruen, sobald alle Wellen geschafft sind (oder der Raum laut Spielstand schon erledigt ist)
/// - Mit DoorInteraction: gruen, sobald die Tuer per Keycard entsperrt wurde
/// Nur EINES der beiden Felder ausfuellen.
/// </summary>
public class DoorTilemapVisual : MonoBehaviour
{
    [Header("Ausloeser (nur eines ausfuellen)")]
    [Tooltip("Kampfraum: Tor wird gruen, wenn alle Wellen geschafft sind")]
    [SerializeField] private EnemySpawn waveSpawner;
    [Tooltip("Keycard-Tuer: Tor wird gruen, wenn die Tuer entsperrt wurde")]
    [SerializeField] private DoorInteraction door;

    [Header("Tilemaps")]
    [Tooltip("Tilemap mit dem roten Tor (wand_tuer_5x3) – sichtbar, solange zu")]
    [SerializeField] private GameObject lockedVisual;
    [Tooltip("Tilemap mit dem gruenen Tor (wand_tuer_5x3_offen) – sichtbar, sobald offen")]
    [SerializeField] private GameObject unlockedVisual;

    private string doorSaveId;

    private void Start()
    {
        if (waveSpawner == null && door == null)
        {
            waveSpawner = GetComponent<EnemySpawn>();
            door = GetComponent<DoorInteraction>();
        }

        if (waveSpawner == null && door == null)
        {
            Debug.LogError("DoorTilemapVisual: Weder EnemySpawn noch DoorInteraction zugewiesen!");
            enabled = false;
            return;
        }

        if (door != null)
            doorSaveId = SaveSystem.GetObjectId(door.gameObject);   // gleiche ID wie in DoorInteraction

        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private bool IsOpen()
    {
        if (waveSpawner != null) return waveSpawner.IsCleared;
        return SaveSystem.IsCleared(doorSaveId);
    }

    private void Apply()
    {
        bool open = IsOpen();

        if (lockedVisual != null && lockedVisual.activeSelf == open)
            lockedVisual.SetActive(!open);

        if (unlockedVisual != null && unlockedVisual.activeSelf != open)
            unlockedVisual.SetActive(open);
    }
}