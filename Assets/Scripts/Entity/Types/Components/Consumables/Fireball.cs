using System.Collections.Generic;
using UnityEngine;

public class Fireball : Consumable {
  [SerializeField] private int damage = 12;
  [SerializeField] private int radius = 3;

  public int Damage { get => damage; }
  public int Radius { get => radius; }

  public override bool Activate(Actor consumer) {
    consumer.GetComponent<Inventory>().SelectedConsumable = this;
    consumer.GetComponent<Player>().ToggleTargetMode(true, radius);
    UIManager.instance.AddMessage($"Select a location to throw a fireball.", "#63FFFF");
    return false;
  }

  public override bool Cast(Actor consumer, Vector3 targetLocation) {
    Bounds targetBounds = new Bounds(targetLocation, Vector3.one * Radius * 2);
    List<Actor> targets = new List<Actor>();

    foreach (Actor target in GameManager.instance.Actors) {
      if (targetBounds.Contains(target.transform.position)) {
        targets.Add(target);
      }
    }

    if (targets.Count == 0) {
      UIManager.instance.AddMessage($"You cast a fireball, but it doesn't reach any targets.", "#FFFFFF");
    } else {
      foreach (Actor target in targets) {
        UIManager.instance.AddMessage($"The fireball explodes, burning {target.name}!", "#FF0000");
        UIManager.instance.AddMessage($"{target.name} takes {damage} damage.", "#FF0000");
        target.GetComponent<Fighter>().Hp -= damage;
      }
    }

    Consume(consumer);
    consumer.GetComponent<Player>().ToggleTargetMode();
    return true;
  }
}
