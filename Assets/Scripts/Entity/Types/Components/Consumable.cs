using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Consumable : MonoBehaviour
{
  [SerializeField] private SpellData spellData;
  [SerializeField] private bool consumeOnActivate;
  [SerializeField] private int amount;

  public bool Activate(Actor consumer)
  {
    consumer.GetComponent<Inventory>().SelectedConsumable = this;

    if (SpellLibrary.ActivateSpell(spellData, consumer, consumeOnActivate) && consumeOnActivate)
    {
      Consume(consumer);
      return true;
    }

    return false;
  }

  public bool Cast(Actor consumer, Actor target)
  {
    if (SpellLibrary.CastSpell(spellData, consumer, target))
    {
      Consume(consumer);
      return true;
    }

    consumer.GetComponent<Inventory>().SelectedConsumable = null;
    return false;
  }

  public bool Cast(Actor consumer, List<Actor> targets)
  {
    if (SpellLibrary.CastSpell(spellData, consumer, null, targets))
    {
      Consume(consumer);
      return true;
    }

    consumer.GetComponent<Inventory>().SelectedConsumable = null;
    return false;
  }

  public void Consume(Actor consumer)
  {
    consumer.GetComponent<Inventory>().SelectedConsumable = null;

    if (amount > 1)
    {
      amount--;
      return;
    }

    consumer.Inventory.Items.Remove(GetComponent<Item>());
    GameManager.instance.RemoveEntity(GetComponent<Item>());
    Destroy(gameObject);
  }
}