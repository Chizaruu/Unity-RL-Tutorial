using System.Collections.Generic;

public static class SpellLibrary
{
  public delegate bool SpellActivation(Actor caster, SpellData data, bool targetSelf = false);
  public delegate bool SpellCasting(Actor caster, Actor target, SpellData data, List<Actor> targets = null);

  private static Dictionary<Spell, SpellActivation> activationStrategies;
  private static Dictionary<Spell, SpellCasting> castingStrategies;

  static SpellLibrary()
  {
    InitializeStrategies();
  }

  private static void InitializeStrategies()
  {
    activationStrategies = new Dictionary<Spell, SpellActivation>
        {
            { Spell.Confusion, ActivateConfusion },
            { Spell.Fireball, ActivateFireball },
            { Spell.Healing, ActivateHealing },
            { Spell.Lightning, ActivateLightning },
            { Spell.Mana, ActivateMana }
        };

    castingStrategies = new Dictionary<Spell, SpellCasting>
        {
            { Spell.Confusion, CastConfusion },
            { Spell.Fireball, CastFireball },
            { Spell.Healing, CastHealing },
            { Spell.Lightning, CastLightning }
        };
  }

  public static bool ActivateSpell(SpellData data, Actor caster, bool targetSelf = false)
  {
    if (activationStrategies.TryGetValue(data.spell, out SpellActivation activation))
    {
      return activation(caster, data, targetSelf);
    }
    return false;
  }

  public static bool CastSpell(SpellData data, Actor caster, Actor target = null, List<Actor> targets = null)
  {
    if (castingStrategies.TryGetValue(data.spell, out SpellCasting casting))
    {
      bool hasCasted = casting(caster, target, data, targets);

      caster.GetComponent<Player>().ToggleTargetMode();
      return hasCasted;
    }
    return false;
  }

  #region Confusion

  private static bool ActivateConfusion(Actor caster, SpellData data, bool targetSelf = false)
  {
    caster.GetComponent<Player>().ToggleTargetMode();
    UIManager.instance.AddMessage($"Select a target to confuse.", "#63FFFF");
    return true;
  }

  private static bool CastConfusion(Actor caster, Actor target, SpellData data, List<Actor> targets)
  {
    if (target != null)
    {
      if (target.TryGetComponent(out ConfusedEnemy confusedEnemy))
      {
        if (confusedEnemy.TurnsRemaining > 0)
        {
          UIManager.instance.AddMessage($"The {target.name} is already confused.", "#FF0000");
          return false;
        }
      }
      else
      {
        confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
      }

      confusedEnemy.PreviousAI = target.AI;
      confusedEnemy.TurnsRemaining = data.duration;

      UIManager.instance.AddMessage($"The eyes of the {target.name} look vacant, as it starts to stumble around!", "#FF0000");
      target.AI = confusedEnemy;
      return true;
    }
    return false;
  }

  #endregion

  #region Fireball

  private static bool ActivateFireball(Actor caster, SpellData data, bool targetSelf = false)
  {
    caster.GetComponent<Player>().ToggleTargetMode(data.isAreaEffect, data.effectRadius);
    UIManager.instance.AddMessage($"Select a location to throw a fireball.", "#FF0000");
    return true;
  }

  private static bool CastFireball(Actor caster, Actor target, SpellData data, List<Actor> targets)
  {
    if (targets != null)
    {
      foreach (var t in targets)
      {
        t.GetComponent<Fighter>().Hp -= data.effectValue;
        UIManager.instance.AddMessage($"The {t.name} is engulfed in a fiery explosion, taking {data.effectValue} damage!", "#FF0000");
      }
      return true;
    }
    return false;
  }
  #endregion

  #region Healing

  private static bool ActivateHealing(Actor caster, SpellData data, bool targetSelf = false)
  {
    if (!targetSelf)
    {
      caster.GetComponent<Player>().ToggleTargetMode();
      UIManager.instance.AddMessage("Select a target to heal.", "#63FFFF");
      return false;
    }
    else
    {
      return CastHealing(caster, caster, data, null);
    }
  }

  private static bool CastHealing(Actor caster, Actor target, SpellData data, List<Actor> targets)
  {
    Actor healingTarget = target != null ? target : caster;
    int amountRecovered = healingTarget.GetComponent<Fighter>().Heal(data.effectValue);

    string targetName = healingTarget == caster ? "Player" : healingTarget.name;

    if (amountRecovered <= 0)
    {
      string message = healingTarget == caster ? "Your health is already full." : $"The {targetName} is already at full health.";
      UIManager.instance.AddMessage(message, "#808080");
      return false;
    }

    UIManager.instance.AddMessage($"The wounds of the {targetName} start to heal, and {targetName.ToLower()} recovers {amountRecovered} HP!", "#00FF00");
    return true;
  }

  #endregion

  #region Lightning

  private static bool ActivateLightning(Actor caster, SpellData data, bool targetSelf = false)
  {
    caster.GetComponent<Player>().ToggleTargetMode();
    UIManager.instance.AddMessage("Select a target to strike.", "#63FFFF");
    return false;
  }

  private static bool CastLightning(Actor caster, Actor target, SpellData data, List<Actor> targets)
  {
    UIManager.instance.AddMessage($"A lighting bolt strikes the {target.name} with a loud thunder, for {data.effectValue} damage!", "#FFFFFF");
    target.GetComponent<Fighter>().Hp -= data.effectValue;
    return true;
  }

  #endregion

  #region Mana

  private static bool ActivateMana(Actor caster, SpellData data, bool targetSelf = false)
  {
    UIManager.instance.AddMessage($"You feel a surge of energy as you regain {data.effectValue} mana!", "#00FF00");
    caster.GetComponent<SpellBook>().Mana += data.effectValue;
    return true;
  }

  #endregion
}