using UnityEngine;

public enum Spell
{
 Confusion,
 Fireball,
 Healing,
 Lightning,
 Mana,
 MagicMissile
}

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
 public Spell spell;
 public int levelRequired;
 public int manaCost;
 public int effectValue;
 public int effectRadius;
 public int duration;
 public bool isAreaEffect;
}
