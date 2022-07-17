using UnityEngine;

/// <summary> A tile on some map. </summary>
[System.Serializable]
public class TileData {
  public bool isExplored, isVisible;
  public Vector3Int gridLocation;

  public TileData(Vector3Int gridLocation) {
    this.gridLocation = gridLocation;
    isExplored = false;
    isVisible = false;
  }
}


