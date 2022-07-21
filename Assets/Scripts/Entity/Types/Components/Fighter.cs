using UnityEngine;

sealed class Fighter : MonoBehaviour {
  [SerializeField] private int maxHp;
  [SerializeField] private int hp;
  [SerializeField] private int defense;
  [SerializeField] private int power;

  public int Hp {
    get => hp; set {
      hp = Mathf.Max(0, Mathf.Min(value, maxHp));
      if (hp == 0)
        Die();
    }
  }

  public int Defense { get => defense; }
  public int Power { get => power; }

  private void Die() {
    if (GetComponent<Player>()) {
      Debug.Log($"You died!");
    } else {
      Debug.Log($"{name} is dead!");
    }

    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = GameManager.instance.DeadSprite;
    spriteRenderer.color = new Color(191, 0, 0, 1);
    spriteRenderer.sortingOrder = 0;

    GetComponent<Actor>().BlocksMovement = false;
    GameManager.instance.RemoveActor(this.GetComponent<Actor>());
    name = $"Remains of {name}";
  }
}
