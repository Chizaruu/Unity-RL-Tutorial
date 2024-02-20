using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class SpellBook : MonoBehaviour
{
  [SerializeField] private int maxMana, mana;
  [SerializeField] private SpellData selectedSpell;
  [SerializeField] private List<SpellData> storedSpells = new List<SpellData>();

  public int Mana
  {
    get => mana; set
    {
      mana = Mathf.Max(0, Mathf.Min(value, maxMana));

      if (GetComponent<Player>())
      {
        UIManager.instance.SetMana(mana, maxMana);
      }
    }
  }

  public int MaxMana
  {
    get => maxMana; set
    {
      maxMana = value;
      if (GetComponent<Player>())
      {
        UIManager.instance.SetManaMax(maxMana);
      }
    }
  }

  public SpellData SelectedSpell { get => selectedSpell; set => selectedSpell = value; }
  public List<SpellData> StoredSpells { get => storedSpells; }

  private void Start()
  {
    if (GetComponent<Player>())
    {
      UIManager.instance.SetManaMax(maxMana);
      UIManager.instance.SetMana(mana, maxMana);
    }
  }

  public void AddSpell(SpellData spell)
  {
    storedSpells.Add(spell);
  }

  public void RemoveSpell(SpellData spell)
  {
    storedSpells.Remove(spell);
  }

  public bool HasSpell(SpellData spell)
  {
    return storedSpells.Contains(spell);
  }

  public void ActivateSpell(SpellData spell)
  {
    if (spell.manaCost > Mana)
    {
      UIManager.instance.AddMessage("You do not have enough mana to activate that spell.", "#FF0000");
      return;
    }

    selectedSpell = spell;

    UIManager.instance.AddMessage($"You chant the words of {spell.name}.", "#FFFFFF");

    if (SpellLibrary.ActivateSpell(spell, GetComponent<Actor>()))
    {
      ConsumeMana(spell.manaCost);
    }

    return;
  }

  public void CastSpell(Actor target)
  {
    if (SpellLibrary.CastSpell(selectedSpell, GetComponent<Actor>(), target))
    {
      ConsumeMana(selectedSpell.manaCost);
    }
  }

  public void CastSpell(List<Actor> targets)
  {
    if (SpellLibrary.CastSpell(selectedSpell, GetComponent<Actor>(), null, targets))
    {
      ConsumeMana(selectedSpell.manaCost);
    }
  }

  public void ConsumeMana(int amount)
  {
    Mana -= amount;
    selectedSpell = null;
  }

  public int RestoreMana(int amount)
  {
    if (mana == maxMana)
    {
      return 0;
    }

    int newManaValue = mana + amount;

    if (newManaValue > maxMana)
    {
      newManaValue = maxMana;
    }

    int amountRecovered = newManaValue - mana;
    Mana = newManaValue;
    return amountRecovered;
  }
}