using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement; // Wichtig für Scene wechseln

[CreateAssetMenu(menuName = "Tiles/Interactable Vent Tile")]
public class InteractableVentTile : InteractableTile
{
    [Header("Vent States")]
    public TileBase closedVentTile;
    public TileBase openVentTile;

    [Header("Scene Settings")]
    public string sceneToLoad; // Name der Szene, in die der Spieler geht

    public override void OnInteract(Vector3Int cellPos, Tilemap tilemap, GameObject player)
    {
        TileBase currentTile = tilemap.GetTile(cellPos);

        // Vent ist geschlossen → öffnen
        if (currentTile == closedVentTile)
        {
            Debug.Log("Vent wird geöffnet");
            tilemap.SetTile(cellPos, openVentTile);
        }
        // Vent ist offen → Spieler geht rein
        else if (currentTile == openVentTile)
        {
            Debug.Log("Spieler geht in den Vent");
            EnterVent();
        }
    }

    private void EnterVent()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("Keine Szene zum Laden angegeben!");
            return;
        }

        // Szene wechseln
        SceneManager.LoadScene(sceneToLoad);
    }
}
