using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A* Pathfinding algorithm </summary>
public class AStar : MonoBehaviour {
  private Node currentNode;
  private HashSet<Node> openList, closedList;

  /// <summary> Finds the shortest path between two points </summary>
  /// <param name="current">Start position</param>
  /// <param name="goal">End position</param>
  /// <returns>List of directions to move to get to the end</returns>
  public Vector2 Compute(Vector2Int start, Vector2Int goal) {
    currentNode = GetNode(start);
    openList = new HashSet<Node>();
    closedList = new HashSet<Node>();
    openList.Add(currentNode);
    Stack<Vector2Int> path = null;

    while (openList.Count > 0 && path == null) {
      List<Node> neighbours = FindNeighbours(currentNode.position, start);
      ExamineNeighbours(neighbours, currentNode, goal);
      UpdateCurrentTile(ref currentNode);
      path = GeneratePath(currentNode, start, goal);
    }
    Vector2 stepDirection = new Vector2(path.Peek().x - start.x, path.Peek().y - start.y);

    if (GameManager.instance.GetActorAtLocation(transform.position + (Vector3)stepDirection)) {
      return Vector2.zero;
    }

    return stepDirection;
  }

  /// <summary> Finds the neighbours of the current node </summary>
  private List<Node> FindNeighbours(Vector2Int parentPosition, Vector2Int start) {
    List<Node> neighbours = new List<Node>();

    for (int x = -1; x <= 1; x++) {
      for (int y = -1; y <= 1; y++) {
        Vector2Int neighbourPos = new Vector2Int(parentPosition.x - x, parentPosition.y - y);
        if (y != 0 || x != 0) {
          if (neighbourPos != start && MapManager.instance.FloorMap.GetTile((Vector3Int)neighbourPos)) {
            Node neighbour = GetNode(neighbourPos);
            neighbours.Add(neighbour);
          }
        }
      }
    }
    return neighbours;
  }

  /// <summary> Examine neighbours </summary>
  private void ExamineNeighbours(List<Node> neighbours, Node current, Vector2Int goal) {
    for (int i = 0; i < neighbours.Count; i++) {
      Node neighbour = neighbours[i];

      int gScore = DetermineGScore(neighbours[i].position, currentNode.position);

      if (openList.Contains(neighbour)) {
        if (current.g + gScore < neighbour.g) {
          CalcValues(current, neighbour, gScore, goal);
        }
      } else if (!closedList.Contains(neighbour)) {
        CalcValues(current, neighbour, gScore, goal);
        openList.Add(neighbour);
      }
    }
  }

  /// <summary> Calculates the values of the neighbour </summary>
  private void CalcValues(Node parent, Node neighbour, int cost, Vector2Int goal) {
    neighbour.parent = parent;
    neighbour.g = parent.g + cost;
    neighbour.h = ((Mathf.Abs((neighbour.position.x - goal.x)) + Mathf.Abs((neighbour.position.y - goal.y))) * 10);
    neighbour.f = neighbour.g + neighbour.h;
  }

  /// <summary> Determines the g score </summary>
  private int DetermineGScore(Vector2Int neighbour, Vector2Int current) {
    int gScore = 0;

    Actor actor = GameManager.instance.GetActorAtLocation((Vector3Int)neighbour);

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

  /// <summary> Generates the path </summary>
  private Stack<Vector2Int> GeneratePath(Node current, Vector2Int start, Vector2Int goal) {
    if (current.position == goal) {
      Stack<Vector2Int> finalPath = new Stack<Vector2Int>();

      while (current.position != start) {
        finalPath.Push(current.position);

        current = current.parent;
      }
      return finalPath;
    }
    return null;
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

