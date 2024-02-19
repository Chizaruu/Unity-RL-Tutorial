using System.Collections.Generic;

public enum Spell
{
  Confusion,
  Fireball,
  Healing,
  Lightning
}

public static class SpellLibrary
{
  public static bool ActivateSpell(SpellData data, Actor caster)
  {
    switch (data.spell)
    {
      case Spell.Confusion:
        caster.GetComponent<Player>().ToggleTargetMode();
        UIManager.instance.AddMessage($"Select a target to confuse.", "#63FFFF");
        return false;
      case Spell.Fireball:
        caster.GetComponent<Player>().ToggleTargetMode(true, data.radius);
        UIManager.instance.AddMessage($"Select a location to throw a fireball.", "#63FFFF");
        return false;
      case Spell.Healing:
        int amountRecovered = caster.GetComponent<Fighter>().Heal(data.healAmount);

        if (amountRecovered <= 0)
        {
          UIManager.instance.AddMessage("Your health is already full.", "#808080");
          return false;
        }

        UIManager.instance.AddMessage($"You are rejuvenated, and recover {amountRecovered} HP!", "#00FF00");
        return true;
      case Spell.Lightning:
        caster.GetComponent<Player>().ToggleTargetMode();
        UIManager.instance.AddMessage("Select a target to strike.", "#63FFFF");
        return false;
      default:
        return false;
    }
  }

  public static bool CastSpell(SpellData data, Actor caster, Actor target = null, List<Actor> targets = null)
  {
    switch (data.spell)
    {
      case Spell.Confusion:
        if (target.TryGetComponent(out ConfusedEnemy confusedEnemy))
        {
          if (confusedEnemy.TurnsRemaining > 0)
          {
            UIManager.instance.AddMessage($"The {target.name} is already confused.", "#FF0000");
            caster.GetComponent<Player>().ToggleTargetMode();
            return false;
          }
        }
        else
        {
          confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
        }
        confusedEnemy.PreviousAI = target.AI;
        confusedEnemy.TurnsRemaining = data.numberOfTurns;

        UIManager.instance.AddMessage($"The eyes of the {target.name} look vacant, as it starts to stumble around!", "#FF0000");
        target.AI = confusedEnemy;
        caster.GetComponent<Player>().ToggleTargetMode();
        return true;
      case Spell.Fireball:
        foreach (Actor targetToHit in targets)
        {
          UIManager.instance.AddMessage($"The {targetToHit.name} is engulfed in a fiery explosion, taking {data.damage} damage!", "#FF0000");
          targetToHit.GetComponent<Fighter>().Hp -= data.damage;
        }

        caster.GetComponent<Player>().ToggleTargetMode();
        return true;
      case Spell.Healing:
        int amountRecovered = target.GetComponent<Fighter>().Heal(data.healAmount);

        string targetName = target == caster ? caster.name : target.name;

        if (amountRecovered <= 0)
        {
          UIManager.instance.AddMessage($"The {targetName} is already at full health.", "#808080");
          return false;
        }

        UIManager.instance.AddMessage($"The wounds of the {targetName} start to heal, and it recovers {amountRecovered} HP!", "#00FF00");
        return true;
      case Spell.Lightning:
        UIManager.instance.AddMessage($"A lighting bolt strikes the {target.name} with a loud thunder, for {data.damage} damage!", "#FFFFFF");
        target.GetComponent<Fighter>().Hp -= data.damage;
        caster.GetComponent<Player>().ToggleTargetMode();
        return true;
      default:
        return false;
    }
  }
}
