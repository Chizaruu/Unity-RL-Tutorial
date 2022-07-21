using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
  public static GameManager instance;

  [SerializeField] private float baseTime = 0.075f;
  [SerializeField] private float delayTime;
  [SerializeField] private bool isPlayerTurn = true;
  [SerializeField] private int actorNum = 0;
  [SerializeField] private List<Entity> entities = new List<Entity>();
  [SerializeField] private List<Actor> actors = new List<Actor>();
  [SerializeField] private Sprite deadSprite;
  private Dictionary<Vector3Int, Node> allNodes = new Dictionary<Vector3Int, Node>();
  public bool IsPlayerTurn { get => isPlayerTurn; }
  public List<Entity> Entities { get => entities; }
  public List<Actor> Actors { get => actors; }
  public Sprite DeadSprite { get => deadSprite; }
  public Dictionary<Vector3Int, Node> AllNodes { get => allNodes; set => allNodes = value; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
  }

  private void StartTurn() {
    //Debug.Log($"{Actors[actorNum].name} starts its turn!");
    if (Actors[actorNum].GetComponent<Player>()) {
      isPlayerTurn = true;
    } else {
      Action.SkipAction(Actors[actorNum]); //We don't have AI logic yet, so just skip their turn.
    }
  }

  public void EndTurn() {
    //Debug.Log($"{Actors[actorNum].name} ends its turn!");
    if (Actors[actorNum].GetComponent<Player>()) {
      isPlayerTurn = false;
    }

    if (actorNum == Actors.Count - 1) {
      actorNum = 0;
    } else {
      actorNum++;
    }

    StartCoroutine(TurnDelay());
  }

  public IEnumerator TurnDelay() {
    yield return new WaitForSeconds(delayTime);
    StartTurn();
  }

  public void AddEntity(Entity entity) {
    entities.Add(entity);
  }

  public void RemoveEntity(Entity entity) {
    entities.Remove(entity);
  }

  public void AddActor(Actor actor) {
    Actors.Add(actor);
    delayTime = SetTime();
  }

  public void InsertActor(Actor actor, int index) {
    Actors.Insert(index, actor);
    delayTime = SetTime();
  }

  public void RemoveActor(Actor actor) {
    Actors.Remove(actor);
    delayTime = SetTime();
  }

  public Actor GetBlockingActorAtLocation(Vector3 location) {
    foreach (Actor actor in Actors) {
      if (actor.BlocksMovement && actor.transform.position == location) {
        return actor;
      }
    }
    return null;
  }

  private float SetTime() => baseTime / Actors.Count;
}
