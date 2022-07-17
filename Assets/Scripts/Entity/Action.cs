using UnityEngine;

static public class Action {
  static public void EscapeAction() {
    Debug.Log("Quit");
    //Application.Quit();
  }

  static public void MovementAction(Entity entity, Vector2 direction) {
    //Debug.Log($"{entity.name} moves {direction}!");
    entity.Move(direction);
    entity.UpdateFieldOfView();
    GameManager.instance.EndTurn();
  }

  static public void SkipAction(Entity entity) {
    //Debug.Log($"{entity.name} skipped their turn!");
    GameManager.instance.EndTurn();
  }
}