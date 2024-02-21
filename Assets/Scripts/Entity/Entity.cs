using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour
{
  [SerializeField] private bool blocksMovement;
  [SerializeField] private SpriteRenderer spriteRenderer;
  [SerializeField] private Vector2Int size = new(1, 1);
  [SerializeField] private Vector3[] occupiedTiles;

  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }
  public SpriteRenderer SpriteRenderer { get => spriteRenderer; set => spriteRenderer = value; }
  public Vector2Int Size { get => size; set => size = value; }
  public Vector3[] OccupiedTiles { get => occupiedTiles; set => occupiedTiles = value; }

  public virtual void AddToGameManager()
  {
    if (GetComponent<Player>())
    {
      GameManager.instance.InsertEntity(this, 0);
    }
    else
    {
      GameManager.instance.AddEntity(this);
    }
  }

  public void Move(Vector2 direction)
  {
    if (!CanMove(direction)) { return; }
    transform.position += (Vector3)direction;

    if (size.x > 1 || size.y > 1)
    {
      occupiedTiles = GetOccupiedTiles();
    }

    MapManager.instance.UpdateTile(this);
  }

  private bool CanMove(Vector2 direction)
  {
    if (size.x > 1 || size.y > 1)
    {
      foreach (Vector3 occupiedTile in OccupiedTiles)
      {
        Vector3 potentialOccupiedTile = occupiedTile + (Vector3)direction;
        Actor actor = GameManager.instance.GetActorAtLocation(potentialOccupiedTile);
        if (!MapManager.instance.IsValidPosition(potentialOccupiedTile) || actor != null && actor != this)
        {
          return false;
        }
      }
    }
    else if (!MapManager.instance.IsValidPosition(transform.position + (Vector3)direction))
    {
      return false;
    }
    return true;
  }

  public Vector3[] GetOccupiedTiles()
  {
    Vector3[] tiles = new Vector3[size.x * size.y];
    for (int i = 0; i < tiles.Length; i++)
    {
      tiles[i] = new Vector3(transform.position.x + i % size.x, transform.position.y + i / size.x);
    }
    return tiles;
  }

  public virtual EntityState SaveState() => new();
}

[System.Serializable]
public class EntityState
{
  public enum EntityType
  {
    Actor,
    Item,
    Other
  }
  [SerializeField] private EntityType entityType;
  [SerializeField] private string name;
  [SerializeField] private bool blocksMovement, isVisible;
  [SerializeField] private Vector3 position;

  public EntityType Type { get => entityType; set => entityType = value; }
  public string Name { get => name; set => name = value; }
  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }
  public bool IsVisible { get => isVisible; set => isVisible = value; }
  public Vector3 Position { get => position; set => position = value; }

  public EntityState(EntityType entityType = EntityType.Other, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3())
  {
    this.entityType = entityType;
    this.name = name;
    this.blocksMovement = blocksMovement;
    this.isVisible = isVisible;
    this.position = position;
  }
}