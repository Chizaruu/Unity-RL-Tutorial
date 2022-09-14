using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Equipment : MonoBehaviour {
  [SerializeField] private Equippable weapon;
  [SerializeField] private Equippable armor;

  public Equippable Weapon { get => weapon; set => weapon = value; }
  public Equippable Armor { get => armor; set => armor = value; }

  public int DefenseBonus() {
    int bonus = 0;

    if (weapon is not null && weapon.DefenseBonus > 0) {
      bonus += weapon.DefenseBonus;
    }

    if (armor is not null && armor.DefenseBonus > 0) {
      bonus += armor.DefenseBonus;
    }
    return bonus;
  }

  public int PowerBonus() {
    int bonus = 0;

    if (weapon is not null && weapon.PowerBonus > 0) {
      bonus += weapon.PowerBonus;
    }

    if (armor is not null && armor.PowerBonus > 0) {
      bonus += armor.PowerBonus;
    }

    return bonus;
  }

  public bool ItemIsEquipped(Item item) {
    if (item.Equippable is null) {
      return false;
    }

    return item.Equippable == weapon || item.Equippable == armor;
  }

  public void UnEquipMessage(string name) {
    UIManager.instance.AddMessage($"You remove the {name}.", "#0da2ff");
  }

  public void EquipMessage(string name) {
    UIManager.instance.AddMessage($"You equip the {name}.", "#0da2ff");
  }

  public void EquipToSlot(string slot, Item item, bool addMessage) {
    Equippable currentItem = slot == "weapon" ? weapon : armor;

    if (currentItem is not null) {
      UnEquipMessage(currentItem.name);
    }

    if (slot == "weapon") {
      weapon = item.Equippable;
    } else {
      armor = item.Equippable;
    }

    if (addMessage) {
      EquipMessage(item.name);
    }
  }

  public void UnequipFromSlot(string slot, bool addMessage) {
    Equippable currentItem = slot == "weapon" ? weapon : armor;

    if (addMessage) {
      UnEquipMessage(currentItem.name);
    }

    if (slot == "weapon") {
      weapon = null;
    } else {
      armor = null;
    }
  }

  public void ToggleEquip(Item equippableItem, bool addMessage) {
    string slot = equippableItem.Equippable.EquipmentType == EquipmentType.Weapon ? "weapon" : "armor";

    if (slot == Weapon.name) {
      UnequipFromSlot(slot, addMessage);
    } else {
      EquipToSlot(slot, equippableItem, addMessage);
    }
  }
}