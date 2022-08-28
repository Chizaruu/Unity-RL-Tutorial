using UnityEngine;

[RequireComponent(typeof(Fighter))]
public class HostileEnemy : AI {
  [SerializeField] private Fighter fighter;
  [SerializeField] private bool isFighting;

  private void OnValidate() {
    fighter = GetComponent<Fighter>();
    AStar = GetComponent<AStar>();
  }

  public override void RunAI() {
    if (!fighter.Target) {
      fighter.Target = GameManager.instance.Actors[0];
    } else if (fighter.Target && !fighter.Target.IsAlive) {
      fighter.Target = null;
    }

    if (fighter.Target) {
      Vector3Int targetPosition = MapManager.instance.FloorMap.WorldToCell(fighter.Target.transform.position);
      if (isFighting || GetComponent<Actor>().FieldOfView.Contains(targetPosition)) {
        if (!isFighting) {
          isFighting = true;
        }

        float targetDistance = Vector3.Distance(transform.position, fighter.Target.transform.position);

        if (targetDistance <= 1.5f) {
          Action.MeleeAction(GetComponent<Actor>(), fighter.Target);
          return;
        } else { //If not in range, move towards target
          MoveAlongPath(targetPosition);
          return;
        }
      }
    }

    Action.WaitAction();
  }

  public override AIState SaveState() => new AIState(
    type: "HostileEnemy"
  );
}