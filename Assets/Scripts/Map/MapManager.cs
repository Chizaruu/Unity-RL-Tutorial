using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
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
  [Header("Tilemaps")]
  [SerializeField] private Tilemap floorMap;
  [SerializeField] private Tilemap obstacleMap;
  [SerializeField] private Tilemap fogMap;
  [Header("Features")]
  [SerializeField] private List<RectangularRoom> rooms = new List<RectangularRoom>();
  [SerializeField] private List<Vector3Int> visibleTiles = new List<Vector3Int>();
  [SerializeField] private Dictionary<Vector3Int, TileData> tiles = new Dictionary<Vector3Int, TileData>();

  public TileBase FloorTile { get => floorTile; }
  public TileBase WallTile { get => wallTile; }
  public Tilemap FloorMap { get => floorMap; }
  public Tilemap ObstacleMap { get => obstacleMap; }
  public Tilemap FogMap { get => fogMap; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
  }

  private void Start() {
    ProcGen procGen = new ProcGen();
    procGen.GenerateDungeon(width, height, roomMaxSize, roomMinSize, maxRooms, rooms);

    AddTileMapToDictionary(floorMap);
    AddTileMapToDictionary(obstacleMap);

    SetupFogMap();

    Instantiate(Resources.Load<GameObject>("NPC"), new Vector3(40 - 5.5f, 25 + 0.5f, 0), Quaternion.identity).name = "NPC";

    Camera.main.transform.position = new Vector3(40, 20.25f, -10);
    Camera.main.orthographicSize = 27;
  }

  ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
  public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;

  public void CreatePlayer(Vector2 position) {
    Instantiate(Resources.Load<GameObject>("Player"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity).name = "Player";
  }

  private void AddTileMapToDictionary(Tilemap tilemap) {
    foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
      if (!tilemap.HasTile(pos)) {
        continue;
      }

      TileData tile = new TileData(pos);
      tiles.Add(pos, tile);
    }
  }

  private void SetupFogMap() {
    foreach (Vector3Int pos in tiles.Keys) {
      fogMap.SetTile(pos, fogTile);
    }
  }

  public void UpdateFogMap(List<Vector3Int> playerFOV) {
    foreach (Vector3Int pos in visibleTiles) {
      tiles[pos].isVisible = false; // Set the tile to not visible
      tiles[pos].isExplored = true; // Set the tile to explored
      fogMap.SetTileFlags(pos, TileFlags.None); // Set the tile to not be a blocking tile
      fogMap.SetColor(pos, new Color(1.0f, 1.0f, 1.0f, 0.5f)); // Set the tile to be a half transparent tile
    }

    visibleTiles.Clear();

    foreach (Vector3Int pos in playerFOV) {
      fogMap.SetTileFlags(pos, TileFlags.None);
      fogMap.SetColor(pos, Color.clear);
      visibleTiles.Add(pos);
    }
  }
}
