using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
  public static MapManager instance;

  [Header("Map Settings")]
  [SerializeField] private int width = 80;
  [SerializeField] private int height = 45;
  [SerializeField] private int roomMaxSize = 10;
  [SerializeField] private int roomMinSize = 6;
  [SerializeField] private int maxRooms = 30;

  [Header("Tiles")]
  [SerializeField] private TileBase floorTile;
  [SerializeField] private TileBase wallTile;
  [SerializeField] private TileBase fogTile;
  [SerializeField] private TileBase upStairsTile;
  [SerializeField] private TileBase downStairsTile;
  [SerializeField] private TileBase closedDoor;
  [SerializeField] private TileBase openDoor;

  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorMap;
  [SerializeField] private Tilemap interactableMap;
  [SerializeField] private Tilemap obstacleMap;
  [SerializeField] private Tilemap fogMap;

  [Header("Features")]
  [SerializeField] private List<RectangularRoom> rooms;
  [SerializeField] private List<Vector3Int> visibleTiles;
  [SerializeField] private Dictionary<Vector3Int, TileData> tiles;
  private Dictionary<Vector2Int, Node> nodes = new();

  public int Width { get => width; }
  public int Height { get => height; }
  public TileBase FloorTile { get => floorTile; }
  public TileBase WallTile { get => wallTile; }
  public TileBase UpStairsTile { get => upStairsTile; }
  public TileBase DownStairsTile { get => downStairsTile; }
  public TileBase ClosedDoor { get => closedDoor; }
  public TileBase OpenDoor { get => openDoor; }
  public Tilemap FloorMap { get => floorMap; }
  public Tilemap InteractableMap { get => interactableMap; }
  public Tilemap ObstacleMap { get => obstacleMap; }
  public Tilemap FogMap { get => fogMap; }
  public List<Vector3Int> VisibleTiles { get => visibleTiles; }
  public Dictionary<Vector2Int, Node> Nodes { get => nodes; set => nodes = value; }

  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
    }
    else
    {
      Destroy(gameObject);
    }

    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.FloorNumber == SaveManager.instance.CurrentFloor);

    if (sceneState != null)
    {
      LoadState(sceneState.MapState);
    }
    else
    {
      GenerateDungeon(true);
    }
  }

  private void Start()
  {
    Camera.main.transform.position = new Vector3(40, 20.25f, -10);
    Camera.main.orthographicSize = 27;
  }

  public void GenerateDungeon(bool isNewGame = false)
  {
    if (floorMap.cellBounds.size.x > 0)
    {
      Reset();
    }
    else
    {
      rooms = new List<RectangularRoom>();
      tiles = new Dictionary<Vector3Int, TileData>();
      visibleTiles = new List<Vector3Int>();
    }

    ProcGen procGen = new();
    procGen.GenerateDungeon(width, height, roomMaxSize, roomMinSize, maxRooms, rooms, isNewGame);

    AddTileMapToDictionary(floorMap);
    AddTileMapToDictionary(interactableMap);
    AddTileMapToDictionary(obstacleMap);
    SetupFogMap();

    if (!isNewGame)
    {
      GameManager.instance.RefreshPlayer();
    }
  }

  ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
  public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;

  public GameObject CreateEntity(string entity, Vector2 position)
  {
    var entityPosition = new Vector2(
      Mathf.Floor(position.x) + 0.5f,
      Mathf.Floor(position.y) + 0.5f
    );

    GameObject entityObject = Instantiate(Resources.Load<GameObject>($"{entity}"), entityPosition, Quaternion.identity);
    entityObject.name = entity;

    if (entityObject.GetComponent<Actor>() != null)
    {
      entityObject.GetComponent<Actor>().AddToGameManager();
    }
    else if (entityObject.GetComponent<Item>() != null)
    {
      entityObject.GetComponent<Item>().AddToGameManager();
    }

    return entityObject;
  }

  public void UpdateFogMap(List<Vector3Int> playerFOV)
  {
    foreach (Vector3Int pos in visibleTiles)
    {
      if (!tiles[pos].IsExplored)
      {
        tiles[pos].IsExplored = true;
      }

      tiles[pos].IsVisible = false;
      fogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f));
    }

    visibleTiles.Clear();

    foreach (Vector3Int pos in playerFOV)
    {
      tiles[pos].IsVisible = true;
      fogMap.SetColor(pos, Color.clear);
      visibleTiles.Add(pos);
    }
  }

  public void SetEntitiesVisibilities()
  {
    foreach (Entity entity in GameManager.instance.Entities)
    {
      if (entity.GetComponent<Player>())
      {
        continue;
      }

      bool isVisible = false;
      Vector3Int entityPosition;

      if (entity.Size.x > 1 || entity.Size.y > 1)
      {
        foreach (Vector3 pos in entity.OccupiedTiles)
        {
          entityPosition = floorMap.WorldToCell(pos);
          if (visibleTiles.Contains(entityPosition))
          {
            isVisible = true;
            break;
          }
        }
      }
      else
      {
        entityPosition = floorMap.WorldToCell(entity.transform.position);
        if (visibleTiles.Contains(entityPosition))
        {
          isVisible = true;
        }
      }

      entity.SpriteRenderer.enabled = isVisible;
    }
  }

  public bool IsValidPosition(Vector3 futurePosition)
  {
    Vector3Int gridPosition = floorMap.WorldToCell(futurePosition);
    if (!InBounds(gridPosition.x, gridPosition.y) || obstacleMap.HasTile(gridPosition))
    {
      return false;
    }
    return true;
  }

  private void AddTileMapToDictionary(Tilemap tilemap)
  {
    foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
    {
      if (!tilemap.HasTile(pos))
      {
        continue;
      }

      TileData tile = new(
        name: tilemap.GetTile(pos).name,
        isExplored: false,
        isVisible: false
      );

      tiles.Add(pos, tile);
    }
  }

  private void SetupFogMap()
  {
    foreach (Vector3Int pos in tiles.Keys)
    {
      if (!fogMap.HasTile(pos))
      {
        fogMap.SetTile(pos, fogTile);
        fogMap.SetTileFlags(pos, TileFlags.None);
      }

      if (tiles[pos].IsExplored)
      {
        fogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f));
      }
      else
      {
        fogMap.SetColor(pos, Color.white);
      }
    }
  }
  public void UpdateTile(Entity entity)
  {
    Vector3Int gridPosition;

    if (entity.Size.x > 1 || entity.Size.y > 1)
    {
      foreach (Vector3 pos in entity.OccupiedTiles)
      {
        gridPosition = floorMap.WorldToCell(pos);
        if (tiles.ContainsKey(gridPosition))
        {
          if (tiles[gridPosition].Name == closedDoor.name)
          {
            interactableMap.SetTile(gridPosition, openDoor);
          }
        }
      }
    }
    else
    {
      gridPosition = floorMap.WorldToCell(entity.transform.position);
      if (tiles.ContainsKey(gridPosition))
      {
        if (tiles[gridPosition].Name == closedDoor.name)
        {
          interactableMap.SetTile(gridPosition, openDoor);
        }
      }
    }
  }

  private void Reset()
  {
    rooms.Clear();
    tiles.Clear();
    visibleTiles.Clear();
    nodes.Clear();

    floorMap.ClearAllTiles();
    interactableMap.ClearAllTiles();
    obstacleMap.ClearAllTiles();
    fogMap.ClearAllTiles();
  }

  public MapState SaveState() => new(tiles, rooms);

  public void LoadState(MapState mapState)
  {
    if (floorMap.cellBounds.size.x > 0)
    {
      Reset();
    }

    rooms = mapState.StoredRooms;
    tiles = mapState.StoredTiles.ToDictionary(x => new Vector3Int((int)x.Key.x, (int)x.Key.y, (int)x.Key.z), x => x.Value);
    if (visibleTiles.Count > 0)
    {
      visibleTiles.Clear();
    }

    foreach (Vector3Int pos in tiles.Keys)
    {
      if (tiles[pos].Name == floorTile.name)
      {
        floorMap.SetTile(pos, floorTile);
      }
      else if (tiles[pos].Name == wallTile.name)
      {
        obstacleMap.SetTile(pos, wallTile);
      }
      else if (tiles[pos].Name == upStairsTile.name)
      {
        floorMap.SetTile(pos, upStairsTile);
      }
      else if (tiles[pos].Name == downStairsTile.name)
      {
        floorMap.SetTile(pos, downStairsTile);
      }
      else if (tiles[pos].Name == closedDoor.name)
      {
        interactableMap.SetTile(pos, closedDoor);
      }
      else if (tiles[pos].Name == openDoor.name)
      {
        interactableMap.SetTile(pos, openDoor);
      }
    }
    SetupFogMap();
  }
}

[System.Serializable]
public class MapState
{
  [SerializeField] private Dictionary<Vector3, TileData> storedTiles;
  [SerializeField] private List<RectangularRoom> storedRooms;
  public Dictionary<Vector3, TileData> StoredTiles { get => storedTiles; set => storedTiles = value; }
  public List<RectangularRoom> StoredRooms { get => storedRooms; set => storedRooms = value; }

  public MapState(Dictionary<Vector3Int, TileData> tiles, List<RectangularRoom> rooms)
  {
    storedTiles = tiles.ToDictionary(x => (Vector3)x.Key, x => x.Value);
    storedRooms = rooms;
  }
}