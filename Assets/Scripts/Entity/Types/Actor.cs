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
    algorithm = new AdamMilVisibility();

    AddToGameManager();
    UpdateFieldOfView();
  }

  public override void AddToGameManager() {
    base.AddToGameManager();

    if (GetComponent<Player>()) {
      GameManager.instance.InsertActor(this, 0);
    } else {
      GameManager.instance.AddActor(this);
    }
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

  public override EntityState SaveState() => new ActorState(
    name: name,
    blocksMovement: BlocksMovement,
    position: transform.position,
    isAlive: IsAlive,
    currentAI: aI != null ? AI.SaveState() : null
  );

  public void LoadState(ActorState state) {
    transform.position = state.position;
    IsAlive = state.IsAlive;
    if (!IsAlive) {
      GameManager.instance.RemoveActor(this);
    }

    if (state.CurrentAI != null) {
      if (state.CurrentAI.Type == "HostileEnemy") {
        aI = GetComponent<HostileEnemy>();
      } else if (state.CurrentAI.Type == "ConfusedEnemy") {
        aI = gameObject.AddComponent<ConfusedEnemy>();
      }
      aI.LoadState(state.CurrentAI);
    }
  }
}

[System.Serializable]
public class ActorState : EntityState {
  public bool IsAlive { get; set; }
  public AIState CurrentAI { get; set; }

  public ActorState(string name, bool blocksMovement, Vector3 position, bool isAlive = true, AIState currentAI = null) : base(name, blocksMovement, position) {
    IsAlive = isAlive;
    CurrentAI = currentAI;
  }
}