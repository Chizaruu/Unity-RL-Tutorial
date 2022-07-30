using UnityEngine;

public class Item : Entity {
  [SerializeField] private Consumable consumable;
  public Consumable Consumable { get => consumable; }

  private void OnValidate() {
    if (GetComponent<Consumable>()) {
      consumable = GetComponent<Consumable>();
    }
  }

  private void Start() {
    AddToGameManager();
  }
}
