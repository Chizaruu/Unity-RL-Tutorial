using UnityEngine;

/// <summary>
/// A generic class to represent players, enemies, items, etc.
/// </summary>
public class Entity : MonoBehaviour {
  [SerializeField] private bool blocksMovement;
  public bool BlocksMovement { get => blocksMovement; set => blocksMovement = value; }

  public void AddToGameManager() {
    GameManager.instance.Entities.Add(this);
  }

  public void Move(Vector2 direction) {
    transform.position += (Vector3)direction;
  }
}