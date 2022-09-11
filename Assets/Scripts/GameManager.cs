using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
  public static GameManager instance;

  [Header("Time")]
  [SerializeField] private float baseTime = 0.075f;
  [SerializeField] private float delayTime; //Read-only

  [Header("Entities")]
  [SerializeField] private bool isPlayerTurn = true; //Read-only
  [SerializeField] private int actorNum = 0; //Read-only
  [SerializeField] private List<Entity> entities;
  [SerializeField] private List<Actor> actors;

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
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    bool isNewScene = !SaveManager.instance.Save.Scenes.Exists(x => x.Name == scene.name);
    if (isNewScene) {
      entities = new List<Entity>();
      actors = new List<Actor>();
    } else {
      SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.Name == scene.name);
      LoadState(sceneState.GameState);
    }
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

  public void InsertEntity(Entity entity, int index) {
    if (!entity.gameObject.activeSelf) {
      entity.gameObject.SetActive(true);
    }
    entities.Insert(index, entity);
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

  public Actor GetActorAtLocation(Vector3 location) {
    foreach (Actor actor in actors) {
      if (actor.BlocksMovement && actor.transform.position == location) {
        return actor;
      }
    }
    return null;
  }

  private float SetTime() => baseTime / actors.Count;

  public GameState SaveState() => new GameState(entities: entities.ConvertAll(e => e.SaveState()));

  public void LoadState(GameState state) {
    if (entities.Count > 0) {
      foreach (Entity entity in entities) {
        Destroy(entity.gameObject);
      }
      entities.Clear();
      actors.Clear();
    }

    foreach (EntityState entityState in state.Entities) {
      if (entityState.Type == EntityState.EntityType.Actor) {
        ActorState actorState = entityState as ActorState;
        Actor actor;
        if (actorState.Name.Contains("Remains of")) {
          string lastWord = actorState.Name.Substring(actorState.Name.LastIndexOf(' ') + 1);
          actor = MapManager.instance.CreateEntity($"{lastWord}", actorState.Position).GetComponent<Actor>();
        } else {
          actor = MapManager.instance.CreateEntity(actorState.Name, actorState.Position).GetComponent<Actor>();
        }
        actor.LoadState(actorState);
      } else if (entityState.Type == EntityState.EntityType.Item) {
        ItemState itemState = entityState as ItemState;
        Item item = MapManager.instance.CreateEntity(itemState.Name, itemState.Position).GetComponent<Item>();
        item.LoadState(itemState);
      }
    }
  }
}

[System.Serializable]
public class GameState {
  [SerializeField] private List<EntityState> entities;

  public List<EntityState> Entities { get => entities; set => entities = value; }

  public GameState(List<EntityState> entities) {
    this.entities = entities;
  }
}