using System.Collections.Generic;
using UnityEngine;

public class Fireball : Consumable {
  [SerializeField] private int damage = 12;
  [SerializeField] private int radius = 3;

  public int Damage { get => damage; }
  public int Radius { get => radius; }

  public override bool Activate(Actor actor) {
    UIManager.instance.ToggleCastMenu(actor, this);
    return true;
  }
}
