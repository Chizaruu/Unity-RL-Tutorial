using UnityEngine;

[RequireComponent(typeof(Actor), typeof(AStar))]
public class AI : MonoBehaviour {
  [SerializeField] private AStar aStar;

  private void OnValidate() => aStar = GetComponent<AStar>();

  public void MoveAlongPath(Vector3Int targetPosition) {
    Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);
    Vector2 direction = aStar.Compute(gridPosition, targetPosition);
    Action.MovementAction(GetComponent<Actor>(), direction);
  }
}
