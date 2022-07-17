using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour {
  [SerializeField] private bool isSentient = false;
  [SerializeField] private int fieldOfViewRange = 8;
  [SerializeField] private List<Vector3Int> fieldOfView = new List<Vector3Int>();
  AdamMilVisibility algorithm;
  public bool IsSentient { get => isSentient; }

  private void Start() {
    if (GetComponent<Player>()) {
      GameManager.instance.InsertEntity(this, 0);
    } else if (IsSentient) {
      GameManager.instance.AddEntity(this);
    }
    algorithm = new AdamMilVisibility();
    UpdateFieldOfView();
  }

  public void Move(Vector2 direction) {
    transform.position += (Vector3)direction;
  }

  public void UpdateFieldOfView() {
    Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);

    fieldOfView.Clear();
    algorithm.Compute(gridPosition, fieldOfViewRange, fieldOfView);

    if (GetComponent<Player>()) {
      MapManager.instance.UpdateFogMap(fieldOfView);
    }
  }
}