using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour {
  [SerializeField] private bool blocksMovement;

  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }

  public virtual void AddToGameManager() {
    if (GetComponent<Player>()) {
      GameManager.instance.InsertEntity(this, 0);
    } else {
      GameManager.instance.AddEntity(this);
    }
  }

  public void Move(Vector2 direction) {
    if (MapManager.instance.IsValidPosition(transform.position + (Vector3)direction)) {
      transform.position += (Vector3)direction;
    }
  }

  public virtual EntityState SaveState() => new EntityState();
}

[System.Serializable]
public class EntityState {
  public enum EntityType {
    Actor,
    Item,
    Other
  }
  [SerializeField] private EntityType type;
  [SerializeField] private string name;
  [SerializeField] private bool blocksMovement, isVisible;
  [SerializeField] private Vector3 position;

  public EntityType Type { get => type; set => type = value; }
  public string Name { get => name; set => name = value; }
  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }
  public bool IsVisible { get => isVisible; set => isVisible = value; }
  public Vector3 Position { get => position; set => position = value; }

  public EntityState(EntityType type = EntityType.Other, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3()) {
    this.type = type;
    this.name = name;
    this.blocksMovement = blocksMovement;
    this.isVisible = isVisible;
    this.position = position;
  }
}