using UnityEngine;

public class Item : Entity {
  [SerializeField] private Consumable consumable;

  public Consumable Consumable { get => consumable; }

  private void OnValidate() {
    if (GetComponent<Consumable>()) {
      consumable = GetComponent<Consumable>();
    }
  }

  private void Start() => AddToGameManager();

  public override void AddToGameManager() {
    base.AddToGameManager();

    GameManager.instance.AddItem(this);
  }

  public override EntityState SaveState() {
    return new ItemState(
      name: name,
      blocksMovement: BlocksMovement,
      position: transform.position,
      parent: transform.parent != null ? transform.parent.gameObject.name : null,
      siblingIndex: transform.parent != null ? transform.GetSiblingIndex() : 0
    );
  }

  public void LoadState(ItemState state) {
    if (state.parent != "") {
      GameObject parent = GameObject.Find(state.parent);
      transform.SetParent(parent.transform);
      parent.GetComponent<Inventory>().Add(this, state.siblingIndex);
    }
    transform.position = state.Position;
  }
}

[System.Serializable]
public class ItemState : EntityState {
  public string parent;
  public int siblingIndex;

  public string Parent { get => parent; set => parent = value; }
  public int SiblingIndex { get => siblingIndex; set => siblingIndex = value; }

  public ItemState(string name, bool blocksMovement, Vector3 position, string parent, int siblingIndex) : base(name, blocksMovement, position) {
    this.parent = parent;
    this.siblingIndex = siblingIndex;
  }
}