using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Inventory : MonoBehaviour {
  [SerializeField] private int capacity = 0;
  [SerializeField] private Consumable selectedConsumable = null;
  [SerializeField] private List<Item> items = new List<Item>();
  public int Capacity { get => capacity; }
  public Consumable SelectedConsumable { get => selectedConsumable; set => selectedConsumable = value; }
  public List<Item> Items { get => items; }

  public void Add(Item item) {
    items.Add(item);
    item.transform.SetParent(transform);
    item.transform.GetComponent<SpriteRenderer>().enabled = false;
  }

  public void Drop(Item item) {
    items.Remove(item);
    item.transform.SetParent(null);
    item.GetComponent<SpriteRenderer>().enabled = true;
    UIManager.instance.AddMessage($"You dropped the {item.name}.", "#FF0000");
  }
}