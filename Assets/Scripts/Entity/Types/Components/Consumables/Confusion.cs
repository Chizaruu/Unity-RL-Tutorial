using UnityEngine;

public class Confusion : Consumable {
  [SerializeField] private int numberOfTurns = 10;

  public int NumberOfTurns { get => numberOfTurns; }

  public override bool Activate(Actor actor) {
    UIManager.instance.ToggleCastMenu(actor, this);
    return true;
  }

  public override void Cast(Actor target) {
    if (target.TryGetComponent(out ConfusedEnemy confusedEnemy)) {
      if (confusedEnemy.TurnsRemaining > 0) {
        UIManager.instance.AddMessage("The target is already confused.", "#FF0000");
        return;
      }

      confusedEnemy.PreviousAI = target.AI;
      confusedEnemy.TurnsRemaining = numberOfTurns;
    } else {
      confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
    }
    confusedEnemy.PreviousAI = target.AI;
    confusedEnemy.TurnsRemaining = NumberOfTurns;

    UIManager.instance.AddMessage($"{target.Name} is confused!", "#FF0000");
  }
}
