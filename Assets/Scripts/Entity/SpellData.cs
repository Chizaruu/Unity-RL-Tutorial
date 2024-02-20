using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Spell Data")]
public class SpellData : ScriptableObject
{
 public Spell spell;
 public int levelRequired;
 public int manaCost;
 public int damage;
 public int radius;
 public int numberOfTurns;
 public int healAmount;
 public int maximumRange;
 public bool areaOfEffect;
 public Sprite icon;
 public Color color;
}
