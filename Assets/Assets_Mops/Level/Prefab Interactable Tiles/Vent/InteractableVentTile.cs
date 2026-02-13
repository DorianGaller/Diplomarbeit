using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Interactable Vent Tile")]
public class InteractableVentTile : InteractableTile
{
    [Header("Vent States")]
    public TileBase closedVentTile;
    public TileBase openVentTile;

    [Header("Canvas Settings")]
    public GameObject canvasToDisable;
    public GameObject canvasToEnable;

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
    VentCanvasManager manager =
        GameObject.FindWithTag("VentCanvasManager")
        .GetComponent<VentCanvasManager>();

    if (manager != null)
    {
        manager.SwitchCanvas();
    }
    else
    {
        Debug.LogWarning("VentCanvasManager nicht gefunden!");
    }
}

}
