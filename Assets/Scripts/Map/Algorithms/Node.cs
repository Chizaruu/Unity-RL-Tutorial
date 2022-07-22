using UnityEngine;

public class Node {
  public int f { get; set; }
  public int g { get; set; }
  public int h { get; set; }
  public Node parent { get; set; }
  public Vector2Int position { get; set; }

  public Node(Vector2Int position) {
    this.position = position;
  }
}