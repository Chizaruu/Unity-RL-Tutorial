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

  public void UnequipMessage(string name) {
    UIManager.instance.AddMessage($"You remove the {name}.", "#da8ee7");
  }

  public void EquipMessage(string name) {
    UIManager.instance.AddMessage($"You equip the {name}.", "#a000c8");
  }

  public void EquipToSlot(string slot, Item item, bool addMessage) {
    Equippable currentItem = slot == "Weapon" ? weapon : armor;

    if (currentItem is not null) {
      UnequipFromSlot(slot, addMessage);
    }

    if (slot == "Weapon") {
      weapon = item.Equippable;
    } else {
      armor = item.Equippable;
    }

    if (addMessage) {
      EquipMessage(item.name);
    }

    item.name = $"{item.name} (E)";
  }

  public void UnequipFromSlot(string slot, bool addMessage) {
    Equippable currentItem = slot == "Weapon" ? weapon : armor;
    currentItem.name = currentItem.name.Replace(" (E)", "");

    if (addMessage) {
      UnequipMessage(currentItem.name);
    }

    if (slot == "Weapon") {
      weapon = null;
    } else {
      armor = null;
    }
  }

  public void ToggleEquip(Item equippableItem, bool addMessage = true) {
    string slot = equippableItem.Equippable.EquipmentType == EquipmentType.Weapon ? "Weapon" : "Armor";

    if (ItemIsEquipped(equippableItem)) {
      UnequipFromSlot(slot, addMessage);
    } else {
      EquipToSlot(slot, equippableItem, addMessage);
    }
  }
}
