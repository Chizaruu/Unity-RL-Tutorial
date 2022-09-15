using System;
using UnityEngine;

public class Item : Entity {
  [SerializeField] private Consumable consumable;
  [SerializeField] private Equippable equippable;

  public Consumable Consumable { get => consumable; }
  public Equippable Equippable { get => equippable; }

  private void OnValidate() {
    if (GetComponent<Consumable>()) {
      consumable = GetComponent<Consumable>();
    }
  }

  private void Start() => AddToGameManager();

  public override EntityState SaveState() => new ItemState(
      name: name,
      blocksMovement: BlocksMovement,
      isVisible: MapManager.instance.VisibleTiles.Contains(MapManager.instance.FloorMap.WorldToCell(transform.position)),
      position: transform.position,
      parent: transform.parent != null ? transform.parent.gameObject.name : ""
    );

  public void LoadState(ItemState state) {
    if (!state.IsVisible) {
      GetComponent<SpriteRenderer>().enabled = false;
    }

    if (state.Parent is not "") {
      GameObject parent = GameObject.Find(state.Parent);
      parent.GetComponent<Inventory>().Add(this);

      if (equippable is not null && state.Name.Contains("(E)")) {
        parent.GetComponent<Equipment>().EquipToSlot(equippable.EquipmentType.ToString(), this, false);
      }
    }

    transform.position = state.Position;
  }
}

[System.Serializable]
public class ItemState : EntityState {
  [SerializeField] private string parent;

  public string Parent { get => parent; set => parent = value; }

  public ItemState(EntityType type = EntityType.Item, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
   string parent = "") : base(type, name, blocksMovement, isVisible, position) {
    this.parent = parent;
  }
}