using UnityEngine;

/// <summary> Node class for the A* pathfinding algorithm </summary>
public class Node {
  public int g { get; set; } // g = cost from start to current node
  public int h { get; set; }  // h = cost from current node to end
  public int f { get; set; }  // f = g + h
  public Node parent { get; set; } // parent node
  public Vector3Int position { get; set; } // position of the node

  /// <summary> Node constructor </summary>
  public Node(Vector3Int position) => this.position = position;
}
