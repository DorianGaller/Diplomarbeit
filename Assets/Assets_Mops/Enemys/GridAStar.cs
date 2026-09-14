using UnityEngine;
using System.Collections.Generic;

public static class GridAStar
{
    private class Node
    {
        public Vector2Int cell;
        public Node parent;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
    }

    public static List<Vector2> FindPath(Vector2 startWorld, Vector2 targetWorld, Vector2 cellSize, LayerMask obstacleLayer, int maxSearchCells = 400)
    {
        Vector2Int start = WorldToCell(startWorld, cellSize);
        Vector2Int target = WorldToCell(targetWorld, cellSize);

        if (IsBlocked(target, cellSize, obstacleLayer))
            target = FindNearestFreeCell(target, cellSize, obstacleLayer);

        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        var allNodes = new Dictionary<Vector2Int, Node>();

        Node startNode = new Node { cell = start, gCost = 0, hCost = Heuristic(start, target) };
        openSet.Add(startNode);
        allNodes[start] = startNode;

        Vector2Int[] neighbors = {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxSearchCells)
        {
            iterations++;

            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current.cell);

            if (current.cell == target)
                return RetracePath(startNode, current, cellSize);

            foreach (var offset in neighbors)
            {
                Vector2Int neighborCell = current.cell + offset;

                if (closedSet.Contains(neighborCell)) continue;
                if (IsBlocked(neighborCell, cellSize, obstacleLayer)) continue;

                float newCost = current.gCost + 1f;

                if (!allNodes.TryGetValue(neighborCell, out Node neighborNode))
                {
                    neighborNode = new Node { cell = neighborCell };
                    allNodes[neighborCell] = neighborNode;
                }

                if (newCost < neighborNode.gCost || !openSet.Contains(neighborNode))
                {
                    neighborNode.gCost = newCost;
                    neighborNode.hCost = Heuristic(neighborCell, target);
                    neighborNode.parent = current;

                    if (!openSet.Contains(neighborNode))
                        openSet.Add(neighborNode);
                }
            }
        }

        return new List<Vector2>();
    }

    private static List<Vector2> RetracePath(Node start, Node end, Vector2 cellSize)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = end;

        while (current != null && current.cell != start.cell)
        {
            path.Add(CellToWorld(current.cell, cellSize));
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private static bool IsBlocked(Vector2Int cell, Vector2 cellSize, LayerMask obstacleLayer)
    {
        Vector2 worldPos = CellToWorld(cell, cellSize);
        return Physics2D.OverlapBox(worldPos, cellSize * 0.8f, 0f, obstacleLayer) != null;
    }

    private static Vector2Int FindNearestFreeCell(Vector2Int cell, Vector2 cellSize, LayerMask obstacleLayer)
    {
        for (int radius = 1; radius <= 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int check = cell + new Vector2Int(x, y);
                    if (!IsBlocked(check, cellSize, obstacleLayer))
                        return check;
                }
            }
        }
        return cell;
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static Vector2Int WorldToCell(Vector2 world, Vector2 cellSize)
    {
        return new Vector2Int(Mathf.RoundToInt(world.x / cellSize.x), Mathf.RoundToInt(world.y / cellSize.y));
    }

    private static Vector2 CellToWorld(Vector2Int cell, Vector2 cellSize)
    {
        return new Vector2(cell.x * cellSize.x, cell.y * cellSize.y);
    }
}