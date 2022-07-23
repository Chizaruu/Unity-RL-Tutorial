using UnityEngine;

[RequireComponent(typeof(Actor), typeof(AStar))]
public class AI : MonoBehaviour {
  [SerializeField] private AStar aStar;

  public AStar AStar { get => aStar; set => aStar = value; }

  private void OnValidate() => aStar = GetComponent<AStar>();

  public void MoveAlongPath(Vector3Int targetPosition) {
    Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);
    Vector2 direction = aStar.Compute((Vector2Int)gridPosition, (Vector2Int)targetPosition);
    Action.MovementAction(GetComponent<Actor>(), direction);
  }
}
