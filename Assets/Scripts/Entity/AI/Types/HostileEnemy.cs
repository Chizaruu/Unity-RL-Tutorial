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

        Actor actor = GetComponent<Actor>();
        float targetDistance;
        Vector3 closestTilePosition = transform.position;
  
        if(actor.Size.x > 1 || actor.Size.y > 1) {
          float closestDistance = float.MaxValue;
          for(int i = 0; i < actor.OccupiedTiles.Length; i++) {
            float distance = Vector3.Distance(actor.OccupiedTiles[i], fighter.Target.transform.position);
            if(distance < closestDistance) {
              closestDistance = distance;
              closestTilePosition = actor.OccupiedTiles[i];
            }
          }
          targetDistance = closestDistance;
        } else {
          targetDistance = Vector3.Distance(transform.position, fighter.Target.transform.position);
        }

        if (targetDistance <= 1.5f) {
          Action.MeleeAction(GetComponent<Actor>(), fighter.Target);
          return;
        } else { //If not in range, move towards target
          MoveAlongPath(closestTilePosition, targetPosition);
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