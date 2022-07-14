using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
  public static MapManager instance;

  [SerializeField] private int width = 80, height = 45;
  [SerializeField] private Color32 darkColor = new Color32(0, 0, 0, 0), lightColor = new Color32(255, 255, 255, 255);
  [SerializeField] private TileBase floorTile, wallTile;
  [SerializeField] private Tilemap floorMap, obstacleMap;

  public Tilemap FloorMap { get => floorMap; }
  public Tilemap ObstacleMap { get => obstacleMap; }

  private void Awake() {
    if (instance == null)
      instance = this;
    else
      Destroy(gameObject);
  }

  private void Start() {
    Vector3Int centerTile = new Vector3Int(width / 2, height / 2, 0);

    BoundsInt wallBounds = new BoundsInt(new Vector3Int(29, 28, 0), new Vector3Int(3, 1, 0));

    for (int x = 0; x < wallBounds.size.x; x++) {
      for (int y = 0; y < wallBounds.size.y; y++) {
        Vector3Int wallPosition = new Vector3Int(wallBounds.min.x + x, wallBounds.min.y + y, 0);
        obstacleMap.SetTile(wallPosition, wallTile);
      }
    }

    Instantiate(Resources.Load<GameObject>("Player"), new Vector3(40 + 0.5f, 25 + 0.5f, 0), Quaternion.identity).name = "Player";
    Instantiate(Resources.Load<GameObject>("NPC"), new Vector3(40 - 5.5f, 25 + 0.5f, 0), Quaternion.identity).name = "NPC";

    Camera.main.transform.position = new Vector3(40, 20.25f, -10);
    Camera.main.orthographicSize = 27;
  }

  ///<summary>Return True if x and y are inside of the bounds of this map. </summary>
  public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;
}
