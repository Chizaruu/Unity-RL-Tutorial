using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RectangularRoom
{
  [SerializeField] private int x, y, width, height;
  public int X { get => x; set => x = value; }
  public int Y { get => y; set => y = value; }
  public int Width { get => width; set => width = value; }
  public int Height { get => height; set => height = value; }

  public RectangularRoom(int x, int y, int width, int height)
  {
    this.x = x;
    this.y = y;
    this.width = width;
    this.height = height;
  }

  /// <summary>
  ///  Return the center of the room
  /// </summary>
  public Vector2Int Center() => new(x + width / 2, y + height / 2);

  /// <summary>
  /// Return a random inner position inside the room
  /// </summary>
  public Vector2Int RandomPoint() => new(Random.Range(x + 1, x + width - 1), Random.Range(y + 1, y + height - 1));

  /// <summary>
  ///  Return the area of this room as a Bounds.
  /// </summary>
  public Bounds GetBounds() => new(new Vector3(x, y, 0), new Vector3(width, height, 0));

  /// <summary>
  /// Return the area of this room as BoundsInt
  /// </summary>
  public BoundsInt GetBoundsInt() => new(new Vector3Int(x, y, 0), new Vector3Int(width, height, 0));

  /// <summary>
  /// Return True if this room overlaps with another RectangularRoom.
  /// </summary>
  public bool Overlaps(List<RectangularRoom> otherRooms)
  {
    foreach (RectangularRoom otherRoom in otherRooms)
    {
      if (GetBounds().Intersects(otherRoom.GetBounds()))
      {
        return true;
      }
    }
    return false;
  }

  /// <summary>
  /// Return a list of positions along the walls of the room.
  /// </summary>
  public List<Vector2Int> GetWallPositions()
  {
    List<Vector2Int> wallPositions = new();

    // Top and bottom walls
    for (int i = x; i < x + width; i++)
    {
      wallPositions.Add(new Vector2Int(i, y)); // Bottom wall
      wallPositions.Add(new Vector2Int(i, y + height - 1)); // Top wall
    }

    // Left and right walls
    for (int j = y + 1; j < y + height - 1; j++)
    {
      wallPositions.Add(new Vector2Int(x, j)); // Left wall
      wallPositions.Add(new Vector2Int(x + width - 1, j)); // Right wall
    }

    return wallPositions;
  }
}