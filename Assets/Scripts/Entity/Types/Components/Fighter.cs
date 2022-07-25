using UnityEngine;

[RequireComponent(typeof(Actor))]
sealed class Fighter : MonoBehaviour {
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

  private void Die() {
    if (GetComponent<Player>()) {
      UIManager.instance.AddMessage("You died!", "#ff0000"); //Red
    } else {
      UIManager.instance.AddMessage($"{name} is dead!", "#ffa500"); //Light Orange
    }

    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = GameManager.instance.DeadSprite;
    spriteRenderer.color = new Color(191, 0, 0, 1);
    spriteRenderer.sortingOrder = 0;

    name = $"Remains of {name}";
    GetComponent<Actor>().BlocksMovement = false;
    GetComponent<Actor>().IsAlive = false;
    if (!GetComponent<Player>()) {
      GameManager.instance.RemoveActor(this.GetComponent<Actor>());
    }
  }
}
