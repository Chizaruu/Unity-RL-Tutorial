using System.Collections.Generic;
using UnityEngine;

sealed class ProcGen {

  /// <summary>
  /// Generate a new dungeon map.
  /// </summary>
  public void GenerateDungeon(int mapWidth, int mapHeight, int roomMaxSize, int roomMinSize, int maxRooms, List<RectangularRoom> rooms) {
    // Generate the rooms.
    for (int roomNum = 0; roomNum < maxRooms; roomNum++) {
      int roomWidth = Random.Range(roomMinSize, roomMaxSize);
      int roomHeight = Random.Range(roomMinSize, roomMaxSize);

      int roomX = Random.Range(0, mapWidth - roomWidth - 1);
      int roomY = Random.Range(0, mapHeight - roomHeight - 1);

      RectangularRoom newRoom = new RectangularRoom(roomX, roomY, roomWidth, roomHeight, MapManager.instance.Rooms.Count);

      //Check if this room intersects with any other rooms
      if (newRoom.Overlaps(rooms))
        continue;
      //If there are no intersections then the room is valid.

      //Dig out this rooms inner area.
      for (int x = roomX; x < roomX + roomWidth; x++) {
        for (int y = roomY; y < roomY + roomHeight; y++) {
          if (x == roomX || x == roomX + roomWidth - 1 || y == roomY || y == roomY + roomHeight - 1) {
            if (MapManager.instance.FloorMap.GetTile(new Vector3Int(x, y, 0)))
              continue;
            MapManager.instance.ObstacleMap.SetTile(new Vector3Int(x, y, 0), MapManager.instance.WallTile);
          } else {
            MapManager.instance.FloorMap.SetTile(new Vector3Int(x, y, 0), MapManager.instance.FloorTile);
          }
        }
      }

      if (MapManager.instance.Rooms.Count == 0)
        //The first room, where the player starts.
        MapManager.instance.CreatePlayer(newRoom.Center());
      else
        //Dig out a tunnel between this room and the previous one.
        TunnelBetween(MapManager.instance.Rooms[MapManager.instance.Rooms.Count - 1], newRoom);

      rooms.Add(newRoom);
    }
  }

  /// <summary>
  /// Return an L-shaped tunnel between these two points using Bresenham lines.
  /// </summary>
  private void TunnelBetween(RectangularRoom room1, RectangularRoom room2) {
    int x1 = (int)room1.Center().x, y1 = (int)room1.Center().y;
    int x2 = (int)room2.Center().x, y2 = (int)room2.Center().y;

    int cornerX, cornerY;

    if (Random.value < 0.5f) {
      //Move horizontally, then vertically.
      cornerX = x2;
      cornerY = y1;
    } else {
      //Move vertically, then horizontally.
      cornerX = x1;
      cornerY = y2;
    }

    //Generate the coordinates for this tunnel.
    List<Vector2Int> tunnelCoords = new List<Vector2Int>();
    BresenhamLine(x1, y1, cornerX, cornerY, tunnelCoords);
    BresenhamLine(cornerX, cornerY, x2, y2, tunnelCoords);

    //Set the tiles for this tunnel.
    foreach (Vector2Int coord in tunnelCoords) {
      if (MapManager.instance.ObstacleMap.HasTile(new Vector3Int(coord.x, coord.y, 0)))
        MapManager.instance.ObstacleMap.SetTile(new Vector3Int(coord.x, coord.y, 0), null);

      MapManager.instance.FloorMap.SetTile(new Vector3Int(coord.x, coord.y, 0), MapManager.instance.FloorTile);

      //Set the tiles around this tile to be walls using for loops
      for (int x = coord.x - 1; x <= coord.x + 1; x++) {
        for (int y = coord.y - 1; y <= coord.y + 1; y++) {
          if (!MapManager.instance.FloorMap.HasTile(new Vector3Int(x, y, 0)))
            MapManager.instance.ObstacleMap.SetTile(new Vector3Int(x, y, 0), MapManager.instance.WallTile);
        }
      }
    }
  }

  private void BresenhamLine(int x1, int y1, int cornerX, int cornerY, List<Vector2Int> tunnelCoords) {
    int x = x1, y = y1;
    int dx = Mathf.Abs(cornerX - x1), dy = Mathf.Abs(cornerY - y1);
    int sx = x1 < cornerX ? 1 : -1, sy = y1 < cornerY ? 1 : -1;
    int err = dx - dy;
    while (true) {
      tunnelCoords.Add(new Vector2Int(x, y));
      if (x == cornerX && y == cornerY)
        break;
      int e2 = 2 * err;
      if (e2 > -dy) {
        err -= dy;
        x += sx;
      }
      if (e2 < dx) {
        err += dx;
        y += sy;
      }
    }
  }
}