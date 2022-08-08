using UnityEngine;

[RequireComponent(typeof(Item))]
public class Consumable : MonoBehaviour {
  public virtual bool Activate(Actor actor) => false;
  public virtual void Cast(Actor target) { }

  public virtual void Cast(Actor actor, Actor target) {
  }

  public void Consume(Actor actor) {
    actor.Inventory.Items.Remove(GetComponent<Item>());
    Destroy(this.gameObject);
  }
}