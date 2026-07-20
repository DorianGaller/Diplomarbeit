using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Interactable Tile")]
public class InteractableTile : Tile
{
    public string tileId;

    public virtual void OnInteract(Vector3Int cellPos, Tilemap tilemap, GameObject player)
    {
        Debug.Log("Interagiert mit Tile: " + tileId);

        // Default-Verhalten (z. B. abbauen)
        tilemap.SetTile(cellPos, null);
    }
}
