using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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
  public bool IsPlayerTurn { get => isPlayerTurn; }
  public List<Entity> Entities { get => entities; }
  public List<Actor> Actors { get => actors; }

  [Header("Entity GameObject Pools")]
  [SerializeField] private string entityToBeCreated;
  [SerializeField] private Vector3 entityToBeCreatedPosition;
  [SerializeField] private ObjectPool<GameObject> actorPool;
  [SerializeField] private ObjectPool<GameObject> itemPool;
  public string EntityToBeCreated { get => entityToBeCreated; set => entityToBeCreated = value; }
  public Vector3 EntityToBeCreatedPosition { get => entityToBeCreatedPosition; set => entityToBeCreatedPosition = value; }
  public ObjectPool<GameObject> ActorPool { get => actorPool; }
  public ObjectPool<GameObject> ItemPool { get => itemPool; }

  [Header("Death")]
  [SerializeField] private Sprite deadSprite;
  public Sprite DeadSprite { get => deadSprite; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
    SceneManager.sceneLoaded += OnSceneLoaded;
    actorPool = new ObjectPool<GameObject>(CreateEntity, OnTakeEntityFromPool, OnReturnEntityToPool);
    itemPool = new ObjectPool<GameObject>(CreateEntity, OnTakeEntityFromPool, OnReturnEntityToPool);
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
    SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.FloorNumber == SaveManager.instance.CurrentFloor);

    if (sceneState is not null) {
      LoadState(sceneState.GameState);
    } else {
      entities = new List<Entity>();
      actors = new List<Actor>();
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

  //Entity Pool Methods (Create, Take, Return) 
  //Start
  private GameObject CreateEntity() {
    GameObject entityObject = Instantiate(Resources.Load<GameObject>($"{entityToBeCreated}"), entityToBeCreatedPosition, Quaternion.identity);
    entityObject.name = entityToBeCreated;
    return entityObject;
  }

  private void OnTakeEntityFromPool(GameObject entity) {
    entity.name = entityToBeCreated;
    entity.transform.position = entityToBeCreatedPosition;
    entity.SetActive(true);

  }

  private void OnReturnEntityToPool(GameObject entity) {
    entity.SetActive(false);
  }
  //End

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

  public GameState SaveState() {
    foreach (Item item in actors[0].Inventory.Items) {
      if (entities.Contains(item)) {
        continue; //This is a hacky way to prevent the player from duplicating item references in the save file
      }
      AddEntity(item);
    }

    GameState gameState = new GameState(entities: entities.ConvertAll(x => x.SaveState()));

    foreach (Item item in actors[0].Inventory.Items) {
      RemoveEntity(item);
    }

    return gameState;
  }

  public void LoadState(GameState state) {
    if (entities.Count > 0) {
      foreach (Entity entity in entities) {
        if (entity is Actor actor) {
          actorPool.Release(entity.gameObject);
        } else if (entity is Item item) {
          itemPool.Release(entity.gameObject);
        }
      }
      Debug.Log("Actor Pool Count: " + actorPool.CountInactive);
      Debug.Log("Item Pool Count: " + itemPool.CountInactive);
      entities.Clear();
      actors.Clear();
    }

    foreach (EntityState entityState in state.Entities) {
      entityToBeCreated = entityState.Name.Contains("Remains of") ?
        entityState.Name.Substring(entityState.Name.LastIndexOf(' ') + 1) : entityState.Name;
      entityToBeCreatedPosition = entityState.Position;

      if (entityState.Type == EntityState.EntityType.Actor) {
        ActorState actorState = entityState as ActorState;
        Actor actor = actorPool.Get().GetComponent<Actor>();

        actor.LoadState(actorState);
      } else if (entityState.Type == EntityState.EntityType.Item) {
        ItemState itemState = entityState as ItemState;
        Item item = itemPool.Get().GetComponent<Item>();

        item.LoadState(itemState);
      }
    }

    entityToBeCreated = "";
    entityToBeCreatedPosition = Vector3.zero;
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