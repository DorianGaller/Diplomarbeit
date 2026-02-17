using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Tiles/Interactable Vent Tile")]
public class InteractableVentTile : InteractableTile
{
    [Header("Tile Swap")]
    public TileBase defaultTile;
    public TileBase openedTile;

    [Header("Canvas Settings")]
    public GameObject canvasToEnable;
    public GameObject canvasToDisable;

    // Zustand pro Tile Position speichern
    private static Dictionary<Vector3Int, int> ventStates = new();

    public override void OnInteract(Vector3Int cellPos, Tilemap tilemap, GameObject player)
    {
        int state = ventStates.ContainsKey(cellPos) ? ventStates[cellPos] : 0;

        if (state == 0)
        {
            Debug.Log("Vent wird geöffnet");

            if (openedTile != null)
                tilemap.SetTile(cellPos, openedTile);

            ventStates[cellPos] = 1;
        }
        else
{
    Debug.Log("Spieler geht in Vent");

    EnterVent();

    if (defaultTile != null)
        tilemap.SetTile(cellPos, defaultTile);

    ventStates[cellPos] = 0;
}
    }

    private void EnterVent()
{
    VentManager manager = Object.FindFirstObjectByType<VentManager>();

    if (manager != null)
    {
        manager.ToggleVent();
    }
    else
    {
        Debug.LogWarning("VentManager nicht gefunden!");
    }
}


}
