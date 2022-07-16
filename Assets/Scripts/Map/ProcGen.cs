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
      if (newRoom.Overlaps(rooms)) {
        continue;
      }
      //If there are no intersections then the room is valid.

      //Dig out this rooms inner area and builds the walls.
      for (int x = roomX; x < roomX + roomWidth; x++) {
        for (int y = roomY; y < roomY + roomHeight; y++) {
          if (x == roomX || x == roomX + roomWidth - 1 || y == roomY || y == roomY + roomHeight - 1) {
            if (SetWallTileIfEmpty(new Vector3Int(x, y, 0))) {
              continue;
            }
          } else {
            if (MapManager.instance.ObstacleMap.GetTile(new Vector3Int(x, y, 0))) {
              MapManager.instance.ObstacleMap.SetTile(new Vector3Int(x, y, 0), null);
            }
            MapManager.instance.FloorMap.SetTile(new Vector3Int(x, y, 0), MapManager.instance.FloorTile);
          }
        }
      }

      if (MapManager.instance.Rooms.Count == 0) {
        //The first room, where the player starts.
        MapManager.instance.CreatePlayer(newRoom.Center());
      } else {
        //Dig out a tunnel between this room and the previous one.
        TunnelBetween(MapManager.instance.Rooms[MapManager.instance.Rooms.Count - 1], newRoom);
      }

      rooms.Add(newRoom);
    }
  }

  /// <summary>
  /// Return an L-shaped tunnel between these two points using Bresenham lines.
  /// </summary>
  private void TunnelBetween(RectangularRoom oldRoom, RectangularRoom newRoom) {
    Vector2Int oldRoomCenter = oldRoom.Center();
    Vector2Int newRoomCenter = newRoom.Center();
    Vector2Int tunnelCorner;

    if (Random.value < 0.5f) {
      //Move horizontally, then vertically.
      tunnelCorner = new Vector2Int(newRoomCenter.x, oldRoomCenter.y);
    } else {
      //Move vertically, then horizontally.
      tunnelCorner = new Vector2Int(oldRoomCenter.x, newRoomCenter.y);
    }

    //Generate the coordinates for this tunnel.
    List<Vector2Int> tunnelCoords = new List<Vector2Int>();
    BresenhamLine(oldRoomCenter, tunnelCorner, tunnelCoords);
    BresenhamLine(tunnelCorner, newRoomCenter, tunnelCoords);

    //Set the tiles for this tunnel.
    for (int i = 0; i < tunnelCoords.Count; i++) {
      if (MapManager.instance.ObstacleMap.HasTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y, 0))) {
        MapManager.instance.ObstacleMap.SetTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y, 0), null);
      }

      //Set the floor tile.
      MapManager.instance.FloorMap.SetTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y, 0), MapManager.instance.FloorTile);

      //Set the wall tiles around this tile to be walls.
      for (int x = tunnelCoords[i].x - 1; x <= tunnelCoords[i].x + 1; x++) {
        for (int y = tunnelCoords[i].y - 1; y <= tunnelCoords[i].y + 1; y++) {
          if (SetWallTileIfEmpty(new Vector3Int(x, y, 0))) {
            continue;
          }
        }
      }
    }
  }

  private void BresenhamLine(Vector2Int roomCenter, Vector2Int tunnelCorner, List<Vector2Int> tunnelCoords) {
    int x = roomCenter.x, y = roomCenter.y;
    int dx = Mathf.Abs(tunnelCorner.x - roomCenter.x), dy = Mathf.Abs(tunnelCorner.y - roomCenter.y);
    int sx = roomCenter.x < tunnelCorner.x ? 1 : -1, sy = roomCenter.y < tunnelCorner.y ? 1 : -1;
    int err = dx - dy;
    while (true) {
      tunnelCoords.Add(new Vector2Int(x, y));
      if (x == tunnelCorner.x && y == tunnelCorner.y) {
        break;
      }
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

  private bool SetWallTileIfEmpty(Vector3Int pos) {
    if (MapManager.instance.FloorMap.GetTile(new Vector3Int(pos.x, pos.y, 0))) {
      return true;
    } else {
      MapManager.instance.ObstacleMap.SetTile(new Vector3Int(pos.x, pos.y, 0), MapManager.instance.WallTile);
      return false;
    }
  }
}