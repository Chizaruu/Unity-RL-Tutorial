using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A* pathfinding algorithm </summary>
public class AStar : MonoBehaviour {
  [SerializeField] private Vector2Int startPos, goalPos; // Start and goal positions
  [SerializeField] private Node currentNode; // Current node
  [SerializeField] private HashSet<Node> openList, closedList; // Open and closed list
  [SerializeField] private Stack<Vector2Int> path; // Path to goal

  /// <summary> Finds the shortest path between two points </summary>q
  /// <param name="current">Start position</param>
  /// <param name="goal">End position</param>
  /// <returns>List of directions to move to get to the end</returns>
  /// <remarks>
  /// This algorithm is based on the pseudocode found here:
  /// https://www.redblobgames.com/pathfinding/a-star/introduction.html#astar
  /// </remarks>
  public Vector2 Compute(Vector3Int start, Vector3Int goal) {
    Reset(); // Reset
    startPos = (Vector2Int)start; // Set start position
    goalPos = (Vector2Int)goal; // Set goal position

    // Add start node to open list
    if (currentNode == null) {
      Initialize(); // Initialize
    }

    while (openList.Count > 0 && path == null) {
      List<Node> neighbours = FindNeighbours(currentNode.position); // Get neighbours
      ExamineNeighbours(neighbours, currentNode); // Examine neighbours
      UpdateCurrentTile(ref currentNode); // Update current tile
      path = GeneratePath(currentNode); // Generate path
    }
    //Get Vector2 distance between start and path
    Vector2 step = new Vector2(path.Peek().x - start.x, path.Peek().y - start.y);

    if (GameManager.instance.GetBlockingActorAtLocation(transform.position + (Vector3)step)) {
      return Vector2.zero;
    }

    return step != Vector2.zero ? step : Vector2.zero; // Return step
  }

  /// <summary> Initializes the algorithm </summary>
  private void Initialize() {
    currentNode = GetNode(startPos); // Set current node

    openList = new HashSet<Node>(); // Initialize open list

    closedList = new HashSet<Node>(); // Initialize closed list

    openList.Add(currentNode); // Add current node to open list
  }

  /// <summary> Finds the neighbours of the current node </summary>
  private List<Node> FindNeighbours(Vector2Int parentPosition) {
    List<Node> neighbours = new List<Node>(); // Initialize neighbours

    // For each neighbour in the x position
    for (int x = -1; x <= 1; x++) {
      // For each neighbour in the y position
      for (int y = -1; y <= 1; y++) {
        Vector2Int neighbourPos = new Vector2Int(parentPosition.x - x, parentPosition.y - y);

        // If the neighbour is not the parent node
        if (y != 0 || x != 0) {
          // If the neighbour is not an obstacle tile
          if (neighbourPos != startPos && MapManager.instance.FloorMap.GetTile((Vector3Int)neighbourPos)) {
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

      int gScore = DetermineGScore(neighbours[i].position, currentNode.position); // Determine g score

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
  private int DetermineGScore(Vector2Int neighbour, Vector2Int current) {
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
  private Node GetNode(Vector2Int position) {
    // If node is not in open list
    if (MapManager.instance.Nodes.ContainsKey(position)) {
      return MapManager.instance.Nodes[position]; // Return node
    }
    // else node is not in open list
    else {
      Node node = new Node(position); // Create node
      MapManager.instance.Nodes.Add(position, node); // Add node to all nodes
      return node; // Return node
    }
  }

  /// <summary> Checks if the node is connected diagonally </summary>
  private bool ConnectedDiagonally(Node current, Node neighbour) {
    Vector2Int direct = current.position - neighbour.position; // Set direct

    Vector3Int first = new Vector3Int(currentNode.position.x + (direct.x * -1), currentNode.position.y); // Set first
    Vector3Int second = new Vector3Int(currentNode.position.x, currentNode.position.y + (direct.y * -1)); // Set second

    // If first is in open list or second is in open list
    if (MapManager.instance.ObstacleTiles.Contains(first) || MapManager.instance.ObstacleTiles.Contains(second)) {
      return false; // Return false
    }
    return true; // Return true
  }

  /// <summary> Generates the path </summary>
  private Stack<Vector2Int> GeneratePath(Node current) {
    // If current is the goal
    if (current.position == goalPos) {
      Stack<Vector2Int> finalPath = new Stack<Vector2Int>(); // Initialize final path

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
    path = null;
    currentNode = null;
  }

  private Vector2 GetPath(Node currentNode) {
    // Create a stack to hold the path
    Stack<Vector2> path = new Stack<Vector2>();
    // While there is a parent
    while (currentNode.parent != null) {
      // Add the current node's position to the stack
      path.Push(currentNode.position);
      // Set the current node to the parent
      currentNode = currentNode.parent;
    }
    // Return the path
    return path.Pop();
  }
}

