using UnityEngine;

public class Lightning : Consumable {
  [SerializeField] private int damage = 20;
  [SerializeField] private int maximumRange = 5;

  public int Damage { get => damage; }
  public int MaximumRange { get => maximumRange; }

  public override bool Activate(Actor actor) {
    UIManager.instance.ToggleCastMenu(actor, this);
    return true;
  }
}
