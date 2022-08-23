using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour, IState<EntityState> {
  [SerializeField] private bool blocksMovement;
  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }

  public virtual void AddToGameManager() {
    GameManager.instance.AddEntity(this);
  }

  public void Move(Vector2 direction) {
    if (MapManager.instance.IsValidPosition(transform.position + (Vector3)direction)) {
      transform.position += (Vector3)direction;
    }
  }

  public virtual EntityState SaveState() => new EntityState(name, BlocksMovement, transform.position);

  public virtual void LoadState(EntityState state) { }
}

[System.Serializable]
public class EntityState {
  public string name;
  public bool blocksMovement;
  public Vector3 position;

  public string Name { get => name; set => name = value; }
  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }
  public Vector3 Position { get => position; set => position = value; }

  public EntityState(string name, bool blocksMovement, Vector3 position) {
    this.name = name;
    this.blocksMovement = blocksMovement;
    this.position = position;
  }
}