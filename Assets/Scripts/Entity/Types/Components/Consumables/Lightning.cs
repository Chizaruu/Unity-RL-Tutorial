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

  public override bool Cast(Actor consumer, Vector3 targetLocation) {
    Actor target = GameManager.instance.GetBlockingActorAtLocation(targetLocation);

    UIManager.instance.AddMessage($"You strike {target.name} with a lightning bolt.", "#FFFFFF");
    UIManager.instance.AddMessage($"{target.name} takes {damage} damage.", "#FFFFFF");
    target.GetComponent<Fighter>().Hp -= damage;
    Consume(consumer);
    consumer.GetComponent<Player>().ToggleTargetMode();
    return true;
  }
}
