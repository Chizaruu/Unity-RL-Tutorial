using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IState<GameState> {
  public static GameManager instance;

  [Header("Time")]
  [SerializeField] private float baseTime = 0.075f;
  [SerializeField] private float delayTime; //Read-only

  [Header("Entities")]
  [SerializeField] private bool isPlayerTurn = true; //Read-only
  [SerializeField] private int actorNum = 0; //Read-only
  [SerializeField] private List<Entity> entities = new List<Entity>();
  [SerializeField] private List<Actor> actors = new List<Actor>();
  [SerializeField] private List<Item> items = new List<Item>();

  [Header("Death")]
  [SerializeField] private Sprite deadSprite;
  public bool IsPlayerTurn { get => isPlayerTurn; }
  public List<Entity> Entities { get => entities; }
  public List<Actor> Actors { get => actors; }
  public Sprite DeadSprite { get => deadSprite; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }

    SaveManager.instance.Save.CurrentScene = SceneManager.GetActiveScene().name;
  }

  private void StartTurn() {
    if (actors[actorNum].GetComponent<Player>()) {
      isPlayerTurn = true;
    } else {
      if (actors[actorNum].AI != null) {
        actors[actorNum].AI.RunAI();
      } else {
        Action.WaitAction();
      }
    }
  }

  public void EndTurn() {
    if (actors[actorNum].GetComponent<Player>()) {
      isPlayerTurn = false;
    }

    if (actorNum == actors.Count - 1) {
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
    if (!entity.gameObject.activeSelf) {
      entity.gameObject.SetActive(true);
    }
    entities.Add(entity);
  }

  public void RemoveEntity(Entity entity) {
    entity.gameObject.SetActive(false);
    entities.Remove(entity);
  }

  public void AddActor(Actor actor) {
    actors.Add(actor);
    delayTime = SetTime();
  }

  public void InsertActor(Actor actor, int index) {
    actors.Insert(index, actor);
    delayTime = SetTime();
  }

  public void RemoveActor(Actor actor) {
    actors.Remove(actor);
    delayTime = SetTime();
  }

  public void AddItem(Item item) {
    items.Add(item);
  }
  public void RemoveItem(Item item) {
    items.Remove(item);
  }

  public Actor GetActorAtLocation(Vector3 location) {
    foreach (Actor actor in actors) {
      if (actor.BlocksMovement && actor.transform.position == location) {
        return actor;
      }
    }
    return null;
  }

  private float SetTime() => baseTime / actors.Count;

  public GameState SaveState() => new GameState(
    entities: entities.ConvertAll(e => e.SaveState()),
    actors: actors.ConvertAll(actor => actor.SaveState() as ActorState),
    items: items.ConvertAll(item => item.SaveState() as ItemState)
  );

  public void LoadState(GameState state) {
    foreach (ActorState actorState in state.Actors) {
      GameObject actor = MapManager.instance.CreateEntity(actorState.Name, actorState.Position);
      actor.GetComponent<Actor>().LoadState(actorState);
    }

    foreach (ItemState itemState in state.Items) {
      GameObject item = MapManager.instance.CreateEntity(itemState.Name, itemState.Position);
      item.GetComponent<Item>().LoadState(itemState);
    }
  }
}

[System.Serializable]
public class GameState {
  [SerializeField] private List<EntityState> entities;
  [SerializeField] private List<ActorState> actors;
  [SerializeField] private List<ItemState> items;

  public List<EntityState> Entities { get => entities; set => entities = value; }
  public List<ActorState> Actors { get => actors; set => actors = value; }
  public List<ItemState> Items { get => items; set => items = value; }

  public GameState(List<EntityState> entities, List<ActorState> actors, List<ItemState> items) {
    this.entities = entities;
    this.actors = actors;
    this.items = items;
  }
}