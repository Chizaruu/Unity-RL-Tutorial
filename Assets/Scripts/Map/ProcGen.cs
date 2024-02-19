using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SysRandom = System.Random;
using UnityRandom = UnityEngine.Random;

sealed class ProcGen
{
  private readonly List<Tuple<int, int>> maxItemsByFloor = new()
  {
    new Tuple<int, int>(1, 1),
    new Tuple<int, int>(4, 2),
    new Tuple<int, int>(7, 3),
    new Tuple<int, int>(10, 4),
  };
  private readonly List<Tuple<int, int>> maxMonstersByFloor = new()
  {
    new Tuple<int, int>(1, 2),
    new Tuple<int, int>(4, 3),
    new Tuple<int, int>(6, 5),
    new Tuple<int, int>(8, 7),
    new Tuple<int, int>(10, 10),
  };

  private readonly List<Tuple<int, string, int>> itemChances = new()
  {
    new Tuple<int, string, int>(0, "Potion of Health", 35),
    new Tuple<int, string, int>(2, "Confusion Scroll", 10),
    new Tuple<int, string, int>(4, "Lightning Scroll", 25), new Tuple<int, string, int>(4, "Sword", 5),
    new Tuple<int, string, int>(6, "Fireball Scroll", 25), new(6, "Chain Mail", 15),
  };

  private readonly List<Tuple<int, string, int>> monsterChances = new()
  {
    new Tuple<int, string, int>(1, "Orc", 80),
    new Tuple<int, string, int>(1, "Slime", 80),
    new Tuple<int, string, int>(1, "Box", 40),
    new Tuple<int, string, int>(1, "Smaller Box", 60),
    new Tuple<int, string, int>(3, "Troll", 15),
    new Tuple<int, string, int>(5, "Troll", 30),
    new Tuple<int, string, int>(7, "Troll", 60),
  };

  public int GetMaxValueForFloor(List<Tuple<int, int>> values, int floor)
  {
    int currentValue = 0;

    foreach (Tuple<int, int> value in values)
    {
      if (floor >= value.Item1)
      {
        currentValue = value.Item2;
      }
    }

    return currentValue;
  }

  public List<string> GetEntitiesAtRandom(List<Tuple<int, string, int>> chances, int numberOfEntities, int floor)
  {
    List<string> entities = new();
    List<int> weightedChances = new();

    foreach (Tuple<int, string, int> chance in chances)
    {
      if (floor >= chance.Item1)
      {
        entities.Add(chance.Item2);
        weightedChances.Add(chance.Item3);
      }
    }

    SysRandom rnd = new();
    List<string> chosenEntities = rnd.Choices(entities, weightedChances, numberOfEntities);

    return chosenEntities;
  }

  /// <summary>
  /// Generate a new dungeon map.
  /// </summary>

  public void GenerateDungeon(int mapWidth, int mapHeight, int roomMaxSize, int roomMinSize, int maxRooms, List<RectangularRoom> rooms, bool isNewGame)
  {
    // Generate the rooms.
    for (int roomNum = 0; roomNum < maxRooms; roomNum++)
    {
      int roomWidth = UnityRandom.Range(roomMinSize, roomMaxSize);
      int roomHeight = UnityRandom.Range(roomMinSize, roomMaxSize);

      int roomX = UnityRandom.Range(0, mapWidth - roomWidth - 1);
      int roomY = UnityRandom.Range(0, mapHeight - roomHeight - 1);

      RectangularRoom newRoom = new(roomX, roomY, roomWidth, roomHeight);

      //Check if this room intersects with any other rooms
      if (newRoom.Overlaps(rooms))
      {
        continue;
      }
      //If there are no intersections then the room is valid.

      //Dig out this rooms inner area and builds the walls.
      for (int x = roomX; x < roomX + roomWidth; x++)
      {
        for (int y = roomY; y < roomY + roomHeight; y++)
        {
          if (x == roomX || x == roomX + roomWidth - 1 || y == roomY || y == roomY + roomHeight - 1)
          {
            if (SetWallTileIfEmpty(new Vector3Int(x, y)))
            {
              continue;
            }
          }
          else
          {
            SetFloorTile(new Vector3Int(x, y));
          }
        }
      }

      if (rooms.Count != 0)
      {
        //Dig out a tunnel between this room and the previous one.
        TunnelBetween(rooms[rooms.Count - 1], newRoom);
      }

      PlaceEntities(newRoom, SaveManager.instance.CurrentFloor);

      rooms.Add(newRoom);
    }

    // After all rooms and tunnels have been created, place the doors.
    foreach (var room in rooms)
    {
      PlaceDoors(room, rooms);
    }

    //Add the stairs to the last room.
    MapManager.instance.FloorMap.SetTile((Vector3Int)rooms[rooms.Count - 1].RandomPoint(), MapManager.instance.DownStairsTile);

    //Add the player to the first room.
    Vector3Int playerPos = (Vector3Int)rooms[0].RandomPoint();
    int maxAttempts = 10, attempts = 0;

    while (GameManager.instance.GetActorAtLocation(new Vector2(playerPos.x + 0.5f, playerPos.y + 0.5f)) is not null)
    {
      playerPos = (Vector3Int)rooms[0].RandomPoint();
      if (attempts >= maxAttempts)
      {
        Actor actor = GameManager.instance.GetActorAtLocation(new Vector2(playerPos.x + 0.5f, playerPos.y + 0.5f));

        if (actor is not null)
        {
          GameManager.instance.RemoveActor(actor);
          GameManager.instance.RemoveEntity(actor);
          GameManager.instance.DestroyEntity(actor);
        }
        break;
      }
      attempts++;
    }

    MapManager.instance.FloorMap.SetTile(playerPos, MapManager.instance.UpStairsTile);

    if (!isNewGame)
    {
      GameManager.instance.Actors[0].transform.position = new Vector3(playerPos.x + 0.5f, playerPos.y + 0.5f, 0);
    }
    else
    {
      GameObject player = MapManager.instance.CreateEntity("Player", (Vector2Int)playerPos);
      Actor playerActor = player.GetComponent<Actor>();

      Item starterWeapon = MapManager.instance.CreateEntity("Dagger", (Vector2Int)playerPos).GetComponent<Item>();
      Item starterArmor = MapManager.instance.CreateEntity("Leather Armor", (Vector2Int)playerPos).GetComponent<Item>();

      playerActor.Inventory.Add(starterWeapon);
      playerActor.Inventory.Add(starterArmor);

      playerActor.Equipment.EquipToSlot("Weapon", starterWeapon, false);
      playerActor.Equipment.EquipToSlot("Armor", starterArmor, false);
    }
  }

  /// <summary>
  /// Return an L-shaped tunnel between these two points using Bresenham lines.
  /// </summary>
  private void TunnelBetween(RectangularRoom oldRoom, RectangularRoom newRoom)
  {
    Vector2Int oldRoomCenter = oldRoom.Center();
    Vector2Int newRoomCenter = newRoom.Center();
    Vector2Int tunnelCorner;

    if (UnityRandom.value < 0.5f)
    {
      //Move horizontally, then vertically.
      tunnelCorner = new Vector2Int(newRoomCenter.x, oldRoomCenter.y);
    }
    else
    {
      //Move vertically, then horizontally.
      tunnelCorner = new Vector2Int(oldRoomCenter.x, newRoomCenter.y);
    }

    //Generate the coordinates for this tunnel.
    List<Vector2Int> tunnelCoords = new();
    BresenhamLine.Compute(oldRoomCenter, tunnelCorner, tunnelCoords);
    BresenhamLine.Compute(tunnelCorner, newRoomCenter, tunnelCoords);

    //Set the tiles for this tunnel.
    for (int i = 0; i < tunnelCoords.Count; i++)
    {
      SetFloorTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y));

      //Set the wall tiles around this tile to be walls.
      for (int x = tunnelCoords[i].x - 1; x <= tunnelCoords[i].x + 1; x++)
      {
        for (int y = tunnelCoords[i].y - 1; y <= tunnelCoords[i].y + 1; y++)
        {
          if (SetWallTileIfEmpty(new Vector3Int(x, y)))
          {
            continue;
          }
        }
      }
    }
  }

  private void PlaceDoors(RectangularRoom room, List<RectangularRoom> allRooms)
  {
    // Get potential door positions along the walls of the room.
    var wallPositions = room.GetWallPositions();

    foreach (var pos in wallPositions)
    {
      Vector3Int position = new(pos.x, pos.y, 0);

      // Check if this wall position is adjacent to a floor tile (which would be a tunnel or another room),
      // and if it's not already part of another room (to avoid placing doors inside rooms).
      if (IsAdjacentToFloor(position) && !IsPartOfAnotherRoom(position, allRooms, room))
      {
        // Remove the floor tile here.
        MapManager.instance.FloorMap.SetTile(position, null);
        // Remove the wall tile here.
        MapManager.instance.ObstacleMap.SetTile(position, null);
        // Place a door here.
        MapManager.instance.InteractableMap.SetTile(position, MapManager.instance.ClosedDoor);
      }
    }
  }

  private bool IsAdjacentToFloor(Vector3Int position)
  {
    // Check the four adjacent tiles (up, down, left, right)
    var adjacentPositions = new Vector3Int[] {
        position + Vector3Int.up,
        position + Vector3Int.down,
        position + Vector3Int.left,
        position + Vector3Int.right
    };

    // Check for floors in a straight line (corridor), not just adjacent
    bool isHorizontalCorridor = MapManager.instance.FloorMap.GetTile(adjacentPositions[2]) && MapManager.instance.FloorMap.GetTile(adjacentPositions[3]);
    bool isVerticalCorridor = MapManager.instance.FloorMap.GetTile(adjacentPositions[0]) && MapManager.instance.FloorMap.GetTile(adjacentPositions[1]);

    // If either horizontal or vertical adjacent tiles are floors, it could be a corridor entrance.
    // We'll further refine this by ensuring one of the orthogonal directions is enclosed by walls, indicating a corridor.
    if (isHorizontalCorridor || isVerticalCorridor)
    {
      // Check the orthogonal direction for walls, confirming it's a corridor.
      Vector3Int orthogonalDirection1 = isHorizontalCorridor ? adjacentPositions[0] : adjacentPositions[2];
      Vector3Int orthogonalDirection2 = isHorizontalCorridor ? adjacentPositions[1] : adjacentPositions[3];

      // Confirm that the adjacent orthogonal tiles are walls, which would mean this is a corridor end.
      if (!MapManager.instance.FloorMap.GetTile(orthogonalDirection1) && !MapManager.instance.FloorMap.GetTile(orthogonalDirection2))
      {
        // This position is an entrance to a corridor, so it is a valid place for a door.
        return true;
      }
    }

    return false;
  }


  private bool IsPartOfAnotherRoom(Vector3Int position, List<RectangularRoom> allRooms, RectangularRoom currentRoom)
  {
    // We need to check if this position is part of any room other than the current one.
    foreach (var room in allRooms)
    {
      if (room != currentRoom)
      {
        var bounds = room.GetBoundsInt();
        if (bounds.Contains(position))
        {
          // If the position is inside the bounds of another room, return true.
          return true;
        }
      }
    }

    return false;
  }

  private bool SetWallTileIfEmpty(Vector3Int pos)
  {
    if (MapManager.instance.FloorMap.GetTile(pos))
    {
      return true;
    }
    else
    {
      MapManager.instance.ObstacleMap.SetTile(pos, MapManager.instance.WallTile);
      return false;
    }
  }

  private void SetFloorTile(Vector3Int pos)
  {
    if (MapManager.instance.ObstacleMap.GetTile(pos))
    {
      MapManager.instance.ObstacleMap.SetTile(pos, null);
    }
    MapManager.instance.FloorMap.SetTile(pos, MapManager.instance.FloorTile);
  }

  private void PlaceEntities(RectangularRoom newRoom, int floorNumber)
  {
    int maxAttempts = 10;
    int numberOfMonsters = UnityRandom.Range(0, GetMaxValueForFloor(maxMonstersByFloor, floorNumber) + 1);
    int numberOfItems = UnityRandom.Range(0, GetMaxValueForFloor(maxItemsByFloor, floorNumber) + 1);

    List<string> monsterNames = GetEntitiesAtRandom(monsterChances, numberOfMonsters, floorNumber);
    List<string> itemNames = GetEntitiesAtRandom(itemChances, numberOfItems, floorNumber);
    List<string> entityNames = monsterNames.Concat(itemNames).ToList();

    foreach (string entityName in entityNames)
    {
      Vector2Int entityPos = Vector2Int.zero;
      Entity entity = MapManager.instance.CreateEntity(entityName, entityPos).GetComponent<Entity>();
      bool canPlace = false;

      for (int attempts = 0; attempts < maxAttempts; attempts++)
      {
        entityPos = newRoom.RandomPoint();
        Vector2 entityPosFloat = new Vector3(entityPos.x + 0.5f, entityPos.y + 0.5f);

        if (entity.Size.x > 1 || entity.Size.y > 1)
        {
          entity.transform.position = entityPosFloat;
          entity.OccupiedTiles = entity.GetOccupiedTiles();

          canPlace = entity.OccupiedTiles.All(pos => MapManager.instance.IsValidPosition(pos)
                          && GameManager.instance.GetActorsAtLocation(pos).Length <= 1);

          if (canPlace)
          {
            break;
          }
        }
        else if (GameManager.instance.GetActorAtLocation(entityPosFloat) == null)
        {
          entity.transform.position = entityPosFloat;
          canPlace = true;
          break;
        }
      }

      if (!canPlace)
      {
        if (entity.GetComponent<Actor>() is not null)
        {
          GameManager.instance.RemoveActor(entity.GetComponent<Actor>());
        }

        GameManager.instance.RemoveEntity(entity);
        GameManager.instance.DestroyEntity(entity);
      }
    }
  }
}