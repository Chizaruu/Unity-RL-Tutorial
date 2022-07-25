using UnityEngine;

static public class Action {
  static public void EscapeAction() {
    Debug.Log("Quit");
    //Application.Quit();
  }

  static public void SkipAction() {
    GameManager.instance.EndTurn();
  }

  static public bool BumpAction(Actor actor, Vector2 direction) {
    Actor target = GameManager.instance.GetBlockingActorAtLocation(actor.transform.position + (Vector3)direction);

    if (target) {
      MeleeAction(actor, target);
      return false;
    } else {
      MovementAction(actor, direction);
      return true;
    }
  }

  static public void MovementAction(Actor actor, Vector2 direction) {
    actor.Move(direction);
    actor.UpdateFieldOfView();
    GameManager.instance.EndTurn();
  }

  static public void MeleeAction(Actor actor, Actor target) {
    int damage = actor.GetComponent<Fighter>().Power - target.GetComponent<Fighter>().Defense;

    string attackDesc = $"{actor.name} attacks {target.name}";

    string colorHex = "";

    if (actor.GetComponent<Player>()) {
      colorHex = "#ffffff"; // white
    } else {
      colorHex = "#d1a3a4"; // light red
    }

    if (damage > 0) {
      UIManager.instance.AddMessage($"{attackDesc} for {damage} hit points.", colorHex);
      target.GetComponent<Fighter>().Hp -= damage;
    } else {
      UIManager.instance.AddMessage($"{attackDesc} but does no damage.", colorHex);
    }
    GameManager.instance.EndTurn();
  }
}