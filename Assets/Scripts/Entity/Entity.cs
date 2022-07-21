using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour {
  [SerializeField] private bool isSentient, blocksMovement;
  [SerializeField] private int fieldOfViewRange = 8;
  [SerializeField] private List<Vector3Int> fieldOfView;
  AdamMilVisibility algorithm;
  public bool IsSentient { get => isSentient; }
  public bool BlocksMovement { get => blocksMovement; }

  private void Start() {
    if (isSentient) {
      if (GetComponent<Player>()) {
        GameManager.instance.InsertEntity(this, 0);
      } else {
        GameManager.instance.AddEntity(this);
      }

      fieldOfView = new List<Vector3Int>();
      algorithm = new AdamMilVisibility();
      UpdateFieldOfView();
    }
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
      MapManager.instance.SetEntitiesVisibilities();
    }
  }
}