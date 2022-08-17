using UnityEngine;

public class Item : Entity {
  [SerializeField] private Consumable consumable;

  public Consumable Consumable { get => consumable; }

  private void OnValidate() {
    if (GetComponent<Consumable>()) {
      consumable = GetComponent<Consumable>();
    }
  }

  private void Start() {
    AddToGameManager();
  }

  public override EntityState GetState() {
    ItemState state = new ItemState();
    state.name = name;

    if (transform.parent != null) {
      state.parent = transform.parent.name;
      state.siblingIndex = transform.GetSiblingIndex();
      state.position = transform.parent.position;
    } else {
      state.position = transform.position;
    }

    return state;
  }

  public void LoadState(ItemState state) {
    if (state.parent != "") {
      GameObject parent = GameObject.Find(state.parent);
      transform.SetParent(parent.transform);
      parent.GetComponent<Inventory>().Add(this, state.siblingIndex);
    }
    transform.position = state.position;
  }
}

[System.Serializable]
public class ItemState : EntityState {
  public string parent;
  public int siblingIndex;
}