using UnityEngine;

[RequireComponent(typeof(Actor))]
public class ConfusedEnemy : AI {
  [SerializeField] private AI previousAI;
  [SerializeField] private int turnsRemaining;

  public AI PreviousAI { get => previousAI; set => previousAI = value; }
  public int TurnsRemaining { get => turnsRemaining; set => turnsRemaining = value; }

  public override void RunAI() {
    if (TurnsRemaining > 0) {
      TurnsRemaining--;
    } else {
      ChangeAI(PreviousAI);
      return;
    }

    Action.WaitAction();
  }
}