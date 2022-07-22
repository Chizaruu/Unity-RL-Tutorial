
using UnityEngine;

[RequireComponent(typeof(Fighter))]
public class HostileEnemy : AI {
  [SerializeField] private Fighter fighter;
  [SerializeField] private bool isFighting;

  private void OnValidate() {
    AStar = GetComponent<AStar>();
    fighter = GetComponent<Fighter>();
  }

  public void RunAI() {
    if (!fighter.Target) {
      fighter.Target = GameManager.instance.Actors[0];
    }

    Vector3Int targetPosition = MapManager.instance.FloorMap.WorldToCell(fighter.Target.transform.position);

    if (isFighting || GetComponent<Actor>().FieldOfView.Contains(targetPosition)) {
      if (!isFighting) {
        isFighting = true;
      }

      //Get distance to target
      float distance = Vector3.Distance(transform.position, fighter.Target.transform.position);

      //If in range, attack
      if (distance <= 1.5f) {
        Action.MeleeAction(GetComponent<Actor>(), fighter.Target);
        return;
      } else { //If not in range, move towards target
        Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);
        Vector2 step = AStar.Compute(gridPosition, targetPosition);

        Action.MovementAction(GetComponent<Actor>(), step);
        return;
      }
    }

    Action.SkipAction();
  }
}