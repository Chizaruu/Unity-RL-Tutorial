using UnityEngine;

/// <summary> A tile on some map. </summary>
[System.Serializable]
public class TileData {
  [SerializeField] private string name;
  [SerializeField] private bool isExplored, isVisible;

  public string Name { get => name; set => name = value; }
  public bool IsExplored { get => isExplored; set => isExplored = value; }
  public bool IsVisible { get => isVisible; set => isVisible = value; }

  public TileData(string name, bool isExplored, bool isVisible) {
    this.name = name;
    this.isExplored = isExplored;
    this.isVisible = isVisible;
  }
}