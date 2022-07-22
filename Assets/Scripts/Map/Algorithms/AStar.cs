using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A* Pathfinding algorithm </summary>
public class AStar : MonoBehaviour {
  private Vector2Int startPos, goalPos;
  private Node currentNode;
  private HashSet<Node> openList, closedList;
  private Stack<Vector2Int> path;

  /// <summary> Finds the shortest path between two points </summary>
  /// <param name="current">Start position</param>
  /// <param name="goal">End position</param>
  /// <returns>List of directions to move to get to the end</returns>
  public Vector2 Compute(Vector3Int start, Vector3Int goal) {
    Reset();
    startPos = (Vector2Int)start;
    goalPos = (Vector2Int)goal;

    if (currentNode == null) {
      Initialize();
    }

    while (openList.Count > 0 && path == null) {
      List<Node> neighbours = FindNeighbours(currentNode.position);
      ExamineNeighbours(neighbours, currentNode);
      UpdateCurrentTile(ref currentNode);
      path = GeneratePath(currentNode);
    }
    Vector2 stepDirection = new Vector2(path.Peek().x - start.x, path.Peek().y - start.y);

    if (GameManager.instance.GetBlockingActorAtLocation(transform.position + (Vector3)stepDirection)) {
      return Vector2.zero;
    }

    return stepDirection;
  }

  /// <summary> Initializes the algorithm </summary>
  private void Initialize() {
    currentNode = GetNode(startPos);
    openList = new HashSet<Node>();
    closedList = new HashSet<Node>();
    openList.Add(currentNode);
  }

  /// <summary> Finds the neighbours of the current node </summary>
  private List<Node> FindNeighbours(Vector2Int parentPosition) {
    List<Node> neighbours = new List<Node>();

    for (int x = -1; x <= 1; x++) {
      for (int y = -1; y <= 1; y++) {
        Vector2Int neighbourPos = new Vector2Int(parentPosition.x - x, parentPosition.y - y);
        if (y != 0 || x != 0) {
          if (neighbourPos != startPos && MapManager.instance.FloorMap.GetTile((Vector3Int)neighbourPos)) {
            Node neighbour = GetNode(neighbourPos);
            neighbours.Add(neighbour);
          }
        }
      }
    }
    return neighbours;
  }

  /// <summary> Examine neighbours </summary>
  private void ExamineNeighbours(List<Node> neighbours, Node current) {
    for (int i = 0; i < neighbours.Count; i++) {
      Node neighbour = neighbours[i];

      if (!ConnectedDiagonally(current, neighbour)) {
        continue;
      }

      int gScore = DetermineGScore(neighbours[i].position, currentNode.position);

      if (openList.Contains(neighbour)) {
        if (current.g + gScore < neighbour.g) {
          CalcValues(current, neighbour, gScore);
        }
      } else if (!closedList.Contains(neighbour)) {
        CalcValues(current, neighbour, gScore);
        openList.Add(neighbour);
      }
    }
  }

  /// <summary> Calculates the values of the neighbour </summary>
  private void CalcValues(Node parent, Node neighbour, int cost) {
    neighbour.parent = parent;
    neighbour.g = parent.g + cost;
    neighbour.h = ((Mathf.Abs((neighbour.position.x - goalPos.x)) + Mathf.Abs((neighbour.position.y - goalPos.y))) * 10);
    neighbour.f = neighbour.g + neighbour.h;
  }

  /// <summary> Determines the g score </summary>
  private int DetermineGScore(Vector2Int neighbour, Vector2Int current) {
    int gScore = 0;

    Actor actor = GameManager.instance.GetBlockingActorAtLocation((Vector3Int)neighbour);

    if (actor) {
      gScore = 10;
    }

    int x = current.x - neighbour.x;
    int y = current.y - neighbour.y;

    if (Mathf.Abs(x - y) % 2 == 1) {
      gScore += 10;
    } else {
      gScore += 14;
    }

    return gScore;
  }

  /// <summary> Updates the current tile </summary>
  private void UpdateCurrentTile(ref Node current) {
    openList.Remove(current);
    closedList.Add(current);

    if (openList.Count > 0) {
      current = openList.OrderBy(x => x.f).First();
    }
  }

  /// <summary> Gets the node </summary>
  private Node GetNode(Vector2Int position) {
    if (MapManager.instance.Nodes.ContainsKey(position)) {
      return MapManager.instance.Nodes[position];
    } else {
      Node node = new Node(position);
      MapManager.instance.Nodes.Add(position, node);
      return node;
    }
  }

  /// <summary> Checks if the node is connected diagonally </summary>
  private bool ConnectedDiagonally(Node current, Node neighbour) {
    Vector2Int direct = current.position - neighbour.position;
    Vector3Int first = new Vector3Int(currentNode.position.x + (direct.x * -1), currentNode.position.y);
    Vector3Int second = new Vector3Int(currentNode.position.x, currentNode.position.y + (direct.y * -1));

    if (MapManager.instance.ObstacleTiles.Contains(first) || MapManager.instance.ObstacleTiles.Contains(second)) {
      return false;
    }
    return true;
  }

  /// <summary> Generates the path </summary>
  private Stack<Vector2Int> GeneratePath(Node current) {
    if (current.position == goalPos) {
      Stack<Vector2Int> finalPath = new Stack<Vector2Int>();

      while (current.position != startPos) {
        finalPath.Push(current.position);

        current = current.parent;
      }
      return finalPath;
    }
    return null;
  }

  /// <summary> Resets the path </summary>
  public void Reset() {
    path = null;
    currentNode = null;
  }

  private Vector2 GetPath(Node currentNode) {
    Stack<Vector2> path = new Stack<Vector2>();

    while (currentNode.parent != null) {
      path.Push(currentNode.position);
      currentNode = currentNode.parent;
    }
    return path.Pop();
  }
}

