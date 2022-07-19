using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public static GameManager instance;

  [SerializeField] private float time = 0.1f;
  [SerializeField] private bool isPlayerTurn = true;
  [SerializeField] private int entityNum = 0;
  [SerializeField] private List<Entity> entities = new List<Entity>();
  public bool IsPlayerTurn { get => isPlayerTurn; }
  public List<Entity> Entities { get => entities; }

  private void Awake() {
    if (instance == null)
      instance = this;
    else
      Destroy(gameObject);
  }

  private void StartTurn() {
    //Debug.Log($"{entities[entityNum].name} starts its turn!");
    if (entities[entityNum].GetComponent<Player>())
      isPlayerTurn = true;
    else if (entities[entityNum].IsSentient)
      Action.SkipAction(entities[entityNum]); //We don't have AI logic yet, so just skip their turn.
  }

  public void EndTurn() {
    //Debug.Log($"{entities[entityNum].name} ends its turn!");
    if (entities[entityNum].GetComponent<Player>())
      isPlayerTurn = false;

    if (entityNum == entities.Count - 1)
      entityNum = 0;
    else
      entityNum++;

    StartCoroutine(TurnDelay());
  }

  public IEnumerator TurnDelay() {
    yield return new WaitForSeconds(time);
    StartTurn();
  }

  public void AddEntity(Entity entity) {
    entities.Add(entity);
  }

  public void InsertEntity(Entity entity, int index) {
    entities.Insert(index, entity);
  }

  public Entity GetBlockingEntityAtLocation(Vector3 location) {
    foreach (Entity entity in entities) {
      if (entity.BlocksMovement && entity.transform.position == location) {
        return entity;
      }
    }
    return null;
  }
}
