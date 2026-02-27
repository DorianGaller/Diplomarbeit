using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Tiles/Item Vent Tile")]
public class ItemVentTile : InteractableTile
{
    [Header("Tile Swap")]
    public TileBase defaultTile;
    public TileBase openedTile;

    [Header("Required Item")]
    public string requiredItemName; // Name des Items

    [Header("Canvas Settings")]
    public GameObject canvasToEnable;
    public GameObject canvasToDisable;

    private static Dictionary<Vector3Int, int> ventStates = new();
    private static Dictionary<Vector3Int, int> interactCount = new();

    public override void OnInteract(Vector3Int cellPos, Tilemap tilemap, GameObject player)
    {
        //PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        //if (inventory == null || !inventory.HasItem(requiredItemName))
        //{
            //Debug.Log("Du brauchst das Item: " + requiredItemName);
            //return;
        //}

        int state = ventStates.ContainsKey(cellPos) ? ventStates[cellPos] : 0;
        int count = interactCount.ContainsKey(cellPos) ? interactCount[cellPos] : 0;

        if (state == 0)
        {
            count++;
            interactCount[cellPos] = count;

            Debug.Log("Interaktion " + count + "/4");

            if (count >= 4)
            {
                Debug.Log("Vent wird geöffnet");

                if (openedTile != null)
                    tilemap.SetTile(cellPos, openedTile);

                ventStates[cellPos] = 1;
                interactCount[cellPos] = 0;
            }
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