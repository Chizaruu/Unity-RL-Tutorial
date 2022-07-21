using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A* pathfinding algorithm </summary>
public class AStar : MonoBehaviour {
  [SerializeField] private Vector3Int startPos, goalPos; // Start and goal positions

  private Node current; // Current node
  private HashSet<Node> openList, closedList; // Open and closed list
  private Stack<Vector3Int> path; // Path to goal
  private List<Vector3Int> obstacleTiles = new List<Vector3Int>(); // Obstacle tiles

  // Start is called before the first frame update
  private void Start() => obstacleTiles = GetObstacleTiles(); // Get obstacle tiles

  /// <summary> Finds the path to the goal </summary>
  public Stack<Vector3Int> Algorithm(Vector3 startPosition, Vector3 goalPosition) {
    Reset(); // Reset
    startPos = MapManager.instance.FloorMap.WorldToCell(startPosition); // Set start position
    goalPos = MapManager.instance.FloorMap.WorldToCell(goalPosition); // Set goal position

    // Add start node to open list
    if (current == null) {
      Initialize(); // Initialize
    }

    // While there is no path
    while (openList.Count > 0 && path == null) {
      List<Node> neighbours = FindNeighbours(current.position); // Get neighbours

      ExamineNeighbours(neighbours, current); // Examine neighbours
      UpdateCurrentTile(ref current); // Update current tile

      path = GeneratePath(current); // Generate path
    }
    return path; // Return path
  }

  /// <summary> Initializes the algorithm </summary>
  private void Initialize() {
    current = GetNode(startPos); // Set current node

    openList = new HashSet<Node>(); // Initialize open list

    closedList = new HashSet<Node>(); // Initialize closed list

    openList.Add(current); // Add current node to open list
  }

  /// <summary> Finds the neighbours of the current node </summary>
  private List<Node> FindNeighbours(Vector3Int parentPosition) {
    List<Node> neighbours = new List<Node>(); // Initialize neighbours

    // For each neighbour in the x position
    for (int x = -1; x <= 1; x++) {
      // For each neighbour in the y position
      for (int y = -1; y <= 1; y++) {
        Vector3Int neighbourPos = new Vector3Int(parentPosition.x - x, parentPosition.y - y, parentPosition.z); // Set neighbour position

        // If the neighbour is not the parent node
        if (y != 0 || x != 0) {
          // If the neighbour is not an obstacle tile
          if (neighbourPos != startPos && MapManager.instance.FloorMap.GetTile(neighbourPos)) {
            Node neighbour = GetNode(neighbourPos); // Get neighbour
            neighbours.Add(neighbour); // Add neighbour to neighbours
          }
        }
      }
    }
    return neighbours; // Return neighbours
  }

  /// <summary> Examine neighbours </summary>
  private void ExamineNeighbours(List<Node> neighbours, Node current) {
    // For each neighbour
    for (int i = 0; i < neighbours.Count; i++) {
      Node neighbour = neighbours[i]; // Set neighbour

      // If neighbour is not in closed list
      if (!ConnectedDiagonally(current, neighbour)) {
        continue; // Skip
      }

      int gScore = DetermineGScore(neighbours[i].position, current.position); // Determine g score

      // If neighbour is not in open list
      if (openList.Contains(neighbour)) {
        // If g score is lower than neighbour's g score
        if (current.g + gScore < neighbour.g) {
          // Set neighbour's g score
          CalcValues(current, neighbour, gScore);
        }
      }
      // If neighbour is not in open list
      else if (!closedList.Contains(neighbour)) {
        // Set neighbour's g score
        CalcValues(current, neighbour, gScore);

        // Add neighbour to open list
        openList.Add(neighbour);
      }
    }
  }

  /// <summary> Calculates the values of the neighbour </summary>
  private void CalcValues(Node parent, Node neighbour, int cost) {
    neighbour.parent = parent; // Set parent
    neighbour.g = parent.g + cost; // Set g score
    neighbour.h = ((Mathf.Abs((neighbour.position.x - goalPos.x)) + Mathf.Abs((neighbour.position.y - goalPos.y))) * 10); // Set h score
    neighbour.f = neighbour.g + neighbour.h; // Set f score
  }

  /// <summary> Determines the g score </summary>
  private int DetermineGScore(Vector3Int neighbour, Vector3Int current) {
    int gScore = 0; // Initialize g score

    int x = current.x - neighbour.x; // Set x
    int y = current.y - neighbour.y; // Set y

    // If x is positive
    if (Mathf.Abs(x - y) % 2 == 1) {
      gScore = 10; // Set g score
    }
    // else x is negative
    else {
      gScore = 14; // Set g score
    }

    return gScore; // Return g score
  }

  /// <summary> Updates the current tile </summary>
  private void UpdateCurrentTile(ref Node current) {
    openList.Remove(current); // Remove current tile from open list

    closedList.Add(current); // Add current tile to closed list

    // If open list is empty
    if (openList.Count > 0) {
      current = openList.OrderBy(x => x.f).First(); // Set current tile to open list's first element
    }
  }

  /// <summary> Gets the node </summary>
  private Node GetNode(Vector3Int position) {
    // If node is not in open list
    if (GameManager.instance.AllNodes.ContainsKey(position)) {
      return GameManager.instance.AllNodes[position]; // Return node
    }
    // else node is not in open list
    else {
      Node node = new Node(position); // Create node
      GameManager.instance.AllNodes.Add(position, node); // Add node to all nodes
      return node; // Return node
    }
  }

  /// <summary> Checks if the node is connected diagonally </summary>
  private bool ConnectedDiagonally(Node currentNode, Node neighbour) {
    Vector3Int direct = currentNode.position - neighbour.position; // Set direct

    Vector3Int first = new Vector3Int(current.position.x + (direct.x * -1), current.position.y, current.position.z); // Set first
    Vector3Int second = new Vector3Int(current.position.x, current.position.y + (direct.y * -1), current.position.z); // Set second

    // If first is in open list or second is in open list
    if (obstacleTiles.Contains(first) || obstacleTiles.Contains(second)) {
      return false; // Return false
    }
    return true; // Return true
  }

  /// <summary> Generates the path </summary>
  private Stack<Vector3Int> GeneratePath(Node current) {
    // If current is the goal
    if (current.position == goalPos) {
      Stack<Vector3Int> finalPath = new Stack<Vector3Int>(); // Initialize final path

      // While current is not the start
      while (current.position != startPos) {
        finalPath.Push(current.position); // Add current to final path

        current = current.parent; // Set current to current's parent
      }
      return finalPath; // Return final path
    }
    return null; // Return null
  }

  /// <summary> Resets the path </summary>
  public void Reset() {
    obstacleTiles.Clear();
    path = null;
    current = null;
  }

  /// <summary> Gets all the obstacle tiles in the path </summary>
  private List<Vector3Int> GetObstacleTiles() {
    //for each tile position in obstacle Map
    foreach (Vector3Int pos in MapManager.instance.ObstacleMap.cellBounds.allPositionsWithin) {
      obstacleTiles.Add(pos); // Add pos to obstacle tiles
    }
    return obstacleTiles; // Return obstacle tiles
  }
}

