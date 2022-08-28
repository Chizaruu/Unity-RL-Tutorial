using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Fighter : MonoBehaviour {
  [SerializeField] private int maxHp, hp, defense, power;
  [SerializeField] private Actor target;

  public int Hp {
    get => hp; set {
      hp = Mathf.Max(0, Mathf.Min(value, maxHp));

      if (GetComponent<Player>()) {
        UIManager.instance.SetHealth(hp, maxHp);
      }

      if (hp == 0)
        Die();
    }
  }

  public int Defense { get => defense; }
  public int Power { get => power; }
  public Actor Target { get => target; set => target = value; }

  private void Start() {
    if (GetComponent<Player>()) {
      UIManager.instance.SetHealthMax(maxHp);
      UIManager.instance.SetHealth(hp, maxHp);
    }
  }

  public void Die() {
    if (GetComponent<Actor>().IsAlive) {
      if (GetComponent<Player>()) {
        UIManager.instance.AddMessage("You died!", "#ff0000"); //Red
      } else {
        UIManager.instance.AddMessage($"{name} is dead!", "#ffa500"); //Light Orange
      }
      GetComponent<Actor>().IsAlive = false;
    }

    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = GameManager.instance.DeadSprite;
    spriteRenderer.color = new Color(191, 0, 0, 1);
    spriteRenderer.sortingOrder = 0;

    name = $"Remains of {name}";
    GetComponent<Actor>().BlocksMovement = false;
    if (!GetComponent<Player>()) {
      GameManager.instance.RemoveActor(this.GetComponent<Actor>());
    }
  }

  public int Heal(int amount) {
    if (hp == maxHp) {
      return 0;
    }

    int newHPValue = hp + amount;

    if (newHPValue > maxHp) {
      newHPValue = maxHp;
    }

    int amountRecovered = newHPValue - hp;
    Hp = newHPValue;
    return amountRecovered;
  }

  public FighterState SaveState() => new FighterState(
      maxHp: maxHp,
      hp: hp,
      defense: defense,
      power: power,
      target: target != null ? target.name : null
    );

  public void LoadState(FighterState state) {
    maxHp = state.MaxHp;
    hp = state.Hp;
    defense = state.Defense;
    power = state.Power;
    target = GameManager.instance.Actors.Find(a => a.name == state.Target);
  }
}

public class FighterState {
  [SerializeField] private int maxHp, hp, defense, power;
  [SerializeField] private string target;

  public int MaxHp { get => maxHp; set => maxHp = value; }
  public int Hp { get => hp; set => hp = value; }
  public int Defense { get => defense; set => defense = value; }
  public int Power { get => power; set => power = value; }
  public string Target { get => target; set => target = value; }

  public FighterState(int maxHp, int hp, int defense, int power, string target) {
    this.maxHp = maxHp;
    this.hp = hp;
    this.defense = defense;
    this.power = power;
    this.target = target;
  }
}