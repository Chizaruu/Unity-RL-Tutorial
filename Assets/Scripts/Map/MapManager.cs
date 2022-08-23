using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
  public static MapManager instance;

  [Header("Map Settings")]
  [SerializeField] private int width = 80;
  [SerializeField] private int height = 45;
  [SerializeField] private int roomMaxSize = 10;
  [SerializeField] private int roomMinSize = 6;
  [SerializeField] private int maxRooms = 30;
  [SerializeField] private int maxMonstersPerRoom = 2;
  [SerializeField] private int maxItemsPerRoom = 2;

  [Header("Tiles")]
  [SerializeField] private TileBase floorTile;
  [SerializeField] private TileBase wallTile;
  [SerializeField] private TileBase fogTile;

  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorMap;
  [SerializeField] private Tilemap obstacleMap;
  [SerializeField] private Tilemap fogMap;

  [Header("Features")]
  [SerializeField] private List<RectangularRoom> rooms;
  [SerializeField] private List<Vector3Int> visibleTiles = new List<Vector3Int>();
  [SerializeField] private Dictionary<Vector3, TileData> tiles;
  private Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();

  public int Width { get => width; }
  public int Height { get => height; }
  public TileBase FloorTile { get => floorTile; }
  public TileBase WallTile { get => wallTile; }
  public Tilemap FloorMap { get => floorMap; }
  public Tilemap ObstacleMap { get => obstacleMap; }
  public Tilemap FogMap { get => fogMap; }
  public Dictionary<Vector2Int, Node> Nodes { get => nodes; set => nodes = value; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }

    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    bool isNewScene = !SaveManager.instance.Save.Scenes.Exists(x => x.Name == scene.name);
    if (isNewScene) {
      rooms = new List<RectangularRoom>();
      tiles = new Dictionary<Vector3, TileData>();

      ProcGen procGen = new ProcGen();
      procGen.GenerateDungeon(width, height, roomMaxSize, roomMinSize, maxRooms, maxMonstersPerRoom, maxItemsPerRoom, rooms);

      AddTileMapToDictionary(floorMap);
      AddTileMapToDictionary(obstacleMap);
    } else {
      SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.Name == scene.name);
      LoadState(sceneState.MapState);
    }
  }

  private void Start() {
    SetupFogMap();
    Camera.main.transform.position = new Vector3(40, 20.25f, -10);
    Camera.main.orthographicSize = 27;
  }

  ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
  public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;

  public GameObject CreateEntity(string entity, Vector2 position) {
    GameObject entityObject = Instantiate(Resources.Load<GameObject>($"{entity}"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity);
    entityObject.name = entity;
    return entityObject;
  }

  public void UpdateFogMap(List<Vector3Int> playerFOV) {
    foreach (Vector3Int pos in visibleTiles) {
      if (!tiles[pos].IsExplored) {
        tiles[pos].IsExplored = true;
      }

      tiles[pos].IsVisible = false;
      fogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f));
    }

    visibleTiles.Clear();

    foreach (Vector3Int pos in playerFOV) {
      tiles[pos].IsVisible = true;
      fogMap.SetColor(pos, Color.clear);
      visibleTiles.Add(pos);
    }
  }

  public void SetEntitiesVisibilities() {
    foreach (Entity entity in GameManager.instance.Entities) {
      if (entity.GetComponent<Player>()) {
        continue;
      }

      Vector3Int entityPosition = floorMap.WorldToCell(entity.transform.position);

      if (visibleTiles.Contains(entityPosition)) {
        entity.GetComponent<SpriteRenderer>().enabled = true;
      } else {
        entity.GetComponent<SpriteRenderer>().enabled = false;
      }
    }
  }

  public bool IsValidPosition(Vector3 futurePosition) {
    Vector3Int gridPosition = floorMap.WorldToCell(futurePosition);
    if (!InBounds(gridPosition.x, gridPosition.y) || obstacleMap.HasTile(gridPosition)) {
      return false;
    }
    return true;
  }

  private void AddTileMapToDictionary(Tilemap tilemap) {
    foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
      if (!tilemap.HasTile(pos)) {
        continue;
      }

      TileData tile = new TileData(
        name: tilemap.GetTile(pos).name,
        isExplored: false,
        isVisible: false
      );

      tiles.Add(pos, tile);
    }
  }

  private void SetupFogMap() {
    foreach (Vector3 pos in tiles.Keys) {
      Vector3Int gridPosition = floorMap.WorldToCell(pos);
      fogMap.SetTile(gridPosition, fogTile);
      fogMap.SetTileFlags(gridPosition, TileFlags.None);
    }
  }

  public MapState SaveState() => new MapState(tiles, rooms);

  public void LoadState(MapState mapState) {
    rooms = mapState.StoredRooms;
    tiles = mapState.StoredTiles;

    foreach (Vector3 pos in tiles.Keys) {
      Vector3Int gridPosition = floorMap.WorldToCell(pos);

      if (tiles[pos].Name == floorTile.name) {
        floorMap.SetTile(gridPosition, floorTile);
      } else if (tiles[pos].Name == wallTile.name) {
        obstacleMap.SetTile(gridPosition, wallTile);
      }
    }
  }
}

[System.Serializable]
public class MapState {
  [SerializeField] private Dictionary<Vector3, TileData> storedTiles;
  [SerializeField] private List<RectangularRoom> storedRooms;
  public Dictionary<Vector3, TileData> StoredTiles { get => storedTiles; set => storedTiles = value; }
  public List<RectangularRoom> StoredRooms { get => storedRooms; set => storedRooms = value; }

  public MapState(Dictionary<Vector3, TileData> tiles, List<RectangularRoom> rooms) {
    storedTiles = tiles;
    storedRooms = rooms;
  }
}