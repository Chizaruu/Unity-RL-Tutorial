using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Consumable : MonoBehaviour {
  public virtual bool Activate(Actor actor) => false;
  public virtual bool Cast(Actor actor, Actor target) => false;
  public virtual bool Cast(Actor actor, List<Actor> targets) => false;

  public void Consume(Actor consumer) {
    if (consumer.GetComponent<Inventory>().SelectedConsumable == this) {
      consumer.GetComponent<Inventory>().SelectedConsumable = null;
    }

    GameManager.instance.RemoveItem(GetComponent<Item>());
    consumer.Inventory.Items.Remove(GetComponent<Item>());
    Destroy(gameObject);
  }
}