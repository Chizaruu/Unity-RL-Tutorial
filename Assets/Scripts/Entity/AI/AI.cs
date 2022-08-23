using UnityEngine;

[RequireComponent(typeof(Actor), typeof(AStar))]
public class AI : MonoBehaviour, IState<AIState> {
  [SerializeField] private AStar aStar;

  public AStar AStar { get => aStar; set => aStar = value; }

  private void OnValidate() => aStar = GetComponent<AStar>();

  public virtual void RunAI() { }

  public void MoveAlongPath(Vector3Int targetPosition) {
    Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);
    Vector2 direction = aStar.Compute((Vector2Int)gridPosition, (Vector2Int)targetPosition);
    Action.MovementAction(GetComponent<Actor>(), direction);
  }

  public virtual AIState SaveState() => new AIState();
  public virtual void LoadState(AIState state) { }
}

[System.Serializable]
public class AIState {
  [SerializeField] private string type;
  [SerializeField] private int currentAction;

  public string Type { get => type; set => type = value; }
  public int CurrentAction { get => currentAction; set => currentAction = value; }

  public AIState(string type = "", int currentAction = 0) {
    this.type = type;
    this.currentAction = currentAction;
  }
}
