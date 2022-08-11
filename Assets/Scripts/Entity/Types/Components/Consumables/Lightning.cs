using UnityEngine;

public class Lightning : Consumable {
  [SerializeField] private int damage = 20;
  [SerializeField] private int maximumRange = 5;

  public int Damage { get => damage; }
  public int MaximumRange { get => maximumRange; }

  public override bool Activate(Actor consumer) {
    consumer.GetComponent<Inventory>().SelectedConsumable = this;
    consumer.GetComponent<Player>().ToggleTargetMode();
    UIManager.instance.AddMessage("Select a target to strike.", "#63FFFF");
    return false;
  }

  public override bool Cast(Actor consumer, Actor target) {
    UIManager.instance.AddMessage($"A lighting bolt strikes the {target.name} with a loud thunder, for {damage} damage!", "#FFFFFF");
    target.GetComponent<Fighter>().Hp -= damage;
    Consume(consumer);
    consumer.GetComponent<Player>().ToggleTargetMode();
    return true;
  }
}
