using System.Collections.Generic;
using UnityEngine;

public class Actor : Entity {
  [SerializeField] private bool isAlive = true; //read-only
  [SerializeField] private int fieldOfViewRange = 8;
  [SerializeField] private List<Vector3Int> fieldOfView = new List<Vector3Int>();
  [SerializeField] private Inventory inventory;
  [SerializeField] private AI aI;
  AdamMilVisibility algorithm;

  public bool IsAlive { get => isAlive; set => isAlive = value; }
  public List<Vector3Int> FieldOfView { get => fieldOfView; }
  public Inventory Inventory { get => inventory; }
  public AI AI { get => aI; set => aI = value; }

  private void OnValidate() {
    if (GetComponent<Inventory>()) {
      inventory = GetComponent<Inventory>();
    }

    if (GetComponent<AI>()) {
      aI = GetComponent<AI>();
    }
  }

  private void Start() {
    AddToGameManager();

    if (GetComponent<Player>()) {
      GameManager.instance.InsertActor(this, 0);
    } else {
      GameManager.instance.AddActor(this);
    }

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

  public override EntityState GetState() {
    ActorState state = new ActorState();
    state.name = name;
    state.isAlive = isAlive;
    state.currentAI = aI.GetState();
    state.position = transform.position;

    return state;
  }

  public void LoadState(ActorState state) {
    name = state.name;
    isAlive = state.isAlive;

    if (state.currentAI != null) {
      if (state.currentAI.type == "HostileEnemy") {
        aI = GetComponent<HostileEnemy>();
      } else if (state.currentAI.type == "ConfusedEnemy") {
        aI = gameObject.AddComponent<ConfusedEnemy>();
      }
      aI.LoadState(state.currentAI);
    }

    transform.position = state.position;
  }
}

[System.Serializable]
public class ActorState : EntityState {
  public bool isAlive;
  public AIState currentAI;
}