using UnityEngine;

[RequireComponent(typeof(Item))]
public class Healing : Consumable {
  [SerializeField] private int amount = 4;

  public int Amount { get => amount; }

  public override bool Activate(Actor actor) {
    int amountRecovered = actor.GetComponent<Fighter>().Heal(amount);

    if (amountRecovered > 0) {
      UIManager.instance.AddMessage($"You consume the {name}, and recover {amountRecovered} HP!", "#00FF00");
      Consume(actor);
      return true;
    } else {
      UIManager.instance.AddMessage("Your health is already full.", "#808080");
      return false;
    }
  }
}