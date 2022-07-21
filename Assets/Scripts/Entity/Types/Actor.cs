using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : Entity {
  [SerializeField] private int fieldOfViewRange = 8;
  [SerializeField] private List<Vector3Int> fieldOfView;
  AdamMilVisibility algorithm;

  private void Start() {
    AddToGameManager();

    if (GetComponent<Player>()) {
      GameManager.instance.InsertActor(this, 0);
    } else {
      GameManager.instance.AddActor(this);
    }

    fieldOfView = new List<Vector3Int>();
    algorithm = new AdamMilVisibility();
    UpdateFieldOfView();
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
