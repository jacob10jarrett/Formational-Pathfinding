using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost; // Cost from the start node
    public int hCost; // Heuristic cost to the end node
    public int fCost { get { return gCost + hCost; } }

    public Node parent; // For retracing the path

    public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
