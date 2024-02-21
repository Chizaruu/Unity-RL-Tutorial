using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  public static GameManager instance;

  [Header("Time")]
  [SerializeField] private float baseTime = 0.075f;
  [SerializeField] private float delayTime; //Read-only
  private Queue<Actor> actorQueue = new();

  [Header("Entities")]
  [SerializeField] private bool isPlayerTurn = true; //Read-only
  [SerializeField] private List<Entity> entities;
  [SerializeField] private List<Actor> actors;

  [Header("Death")]
  [SerializeField] private Sprite deadSprite;
  public bool IsPlayerTurn { get => isPlayerTurn; }
  public List<Entity> Entities { get => entities; }
  public List<Actor> Actors { get => actors; }
  public Queue<Actor> ActorQueue { get => actorQueue; set => actorQueue = value; }
  public Sprite DeadSprite { get => deadSprite; }

  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    SceneState sceneState = SaveManager.instance.Save.Scenes.Find(x => x.FloorNumber == SaveManager.instance.CurrentFloor);

    if (sceneState != null)
    {
      LoadState(sceneState.GameState, true);
    }
    else
    {
      actorQueue = new Queue<Actor>();
      entities = new List<Entity>();
      actors = new List<Actor>();
    }
  }

  private void StartTurn()
  {
    Actor actor = actorQueue.Peek();
    if (actor.GetComponent<Player>())
    {
      isPlayerTurn = true;
    }
    else
    {
      if (actor.AI != null)
      {
        actor.AI.RunAI();
      }
      else
      {
        Action.WaitAction();
      }
    }
  }

  public void EndTurn()
  {
    Actor actor = actorQueue.Dequeue();

    if (actor.GetComponent<Player>())
    {
      isPlayerTurn = false;
    }

    actorQueue.Enqueue(actor);

    StartCoroutine(TurnDelay());
  }

  public IEnumerator TurnDelay()
  {
    yield return new WaitForSeconds(delayTime);
    StartTurn();
  }

  public void AddEntity(Entity entity)
  {
    if (!entity.gameObject.activeSelf)
    {
      entity.gameObject.SetActive(true);
    }
    entities.Add(entity);
  }

  public void InsertEntity(Entity entity, int index)
  {
    if (!entity.gameObject.activeSelf)
    {
      entity.gameObject.SetActive(true);
    }
    entities.Insert(index, entity);
  }

  public void RemoveEntity(Entity entity)
  {
    entity.gameObject.SetActive(false);
    entities.Remove(entity);
  }

  public void DestroyEntity(Entity entity)
  {
    Destroy(entity.gameObject);
  }

  public void AddActor(Actor actor)
  {
    actors.Add(actor);
    delayTime = SetTime();
    actorQueue.Enqueue(actor);
  }

  public void InsertActor(Actor actor, int index)
  {
    actors.Insert(index, actor);
    delayTime = SetTime();
    actorQueue.Enqueue(actor);
  }

  public void RemoveActor(Actor actor)
  {
    if (actor.GetComponent<Player>())
    {
      return;
    }
    actors.Remove(actor);
    delayTime = SetTime();
    actorQueue = new Queue<Actor>(actorQueue.Where(x => x != actor));
  }

  public void RefreshPlayer()
  {
    actors[0].UpdateFieldOfView();
  }

  public Actor GetActorAtLocation(Vector3 location)
  {
    foreach (Actor actor in actors)
    {
      if (!actor.IsAlive) continue; // Skip dead actors

      if (actor.Size.x == 1 && actor.Size.y == 1)
      {
        if (actor.transform.position == location)
        {
          return actor;
        }
      }
      else
      {
        if (actor.OccupiedTiles.Contains(location))
        {
          return actor;
        }
      }
    }
    return null;
  }

  public Actor[] GetActorsAtLocation(Vector3 location)
  {
    List<Actor> actorsAtLocation = new();

    foreach (Actor actor in actors)
    {
      if (!actor.IsAlive) continue; // Skip dead actors

      if (actor.Size.x == 1 && actor.Size.y == 1)
      {
        if (actor.transform.position == location)
        {
          actorsAtLocation.Add(actor);
        }
      }
      else
      {
        if (actor.OccupiedTiles.Contains(location))
        {
          actorsAtLocation.Add(actor);
        }
      }
    }
    return actorsAtLocation.ToArray();
  }

  private float SetTime() => baseTime / actors.Count;

  public GameState SaveState()
  {
    List<EntityState> itemStates = actors[0].Inventory.Items.Select(item => item.SaveState()).ToList();
    List<EntityState> actorStates = actors.Select(actor => actor.SaveState()).ToList();

    GameState gameState = new(actorStates.Concat(itemStates).ToList());

    return gameState;
  }

  public void LoadState(GameState state, bool canRemovePlayer)
  {
    isPlayerTurn = false;
    ResetManager(canRemovePlayer);
    StartCoroutine(LoadEntityStates(state.Entities, canRemovePlayer));
  }

  private IEnumerator LoadEntityStates(List<EntityState> entityStates, bool canPlacePlayer)
  {
    foreach (var entityState in entityStates)
    {
      yield return new WaitForEndOfFrame();
      ProcessEntityState(entityState, canPlacePlayer);
    }

    isPlayerTurn = true;
  }
  private void ProcessEntityState(EntityState entityState, bool canPlacePlayer)
  {
    string entityName = entityState.Name;
    if (entityState.Type == EntityState.EntityType.Actor && entityState.Name.Contains("Remains of"))
    {
      entityName = entityState.Name[(entityState.Name.LastIndexOf(' ') + 1)..];
    }
    else if (entityState.Type == EntityState.EntityType.Item)
    {
      entityName = entityState.Name.Contains("(E)") ? entityState.Name[..^4] : entityState.Name;
    }

    if (entityState.Type == EntityState.EntityType.Actor)
    {
      if (entityName == "Player" && !canPlacePlayer)
      {
        actors[0].transform.position = entityState.Position;
        RefreshPlayer();
        return;
      }

      var actor = MapManager.instance.CreateEntity(entityName, entityState.Position).GetComponent<Actor>();
      actor.LoadState((ActorState)entityState);
    }
    else if (entityState.Type == EntityState.EntityType.Item)
    {
      if (((ItemState)entityState).Parent == "Player" && !canPlacePlayer)
      {
        return;
      }

      var item = MapManager.instance.CreateEntity(entityName, entityState.Position).GetComponent<Item>();
      item.LoadState((ItemState)entityState);
    }
  }

  public void ResetManager(bool canRemovePlayer)
  {
    foreach (var entity in entities)
    {
      if (!canRemovePlayer && entity.GetComponent<Player>())
      {
        continue;
      }
      Destroy(entity.gameObject);
    }

    entities.Clear();
    if (canRemovePlayer)
    {
      actors.Clear();
      actorQueue.Clear();
    }
    else
    {
      entities.Add(actors[0]);
      actors = new List<Actor> { actors[0] };
      actorQueue = new Queue<Actor>(actorQueue.Where(x => x.GetComponent<Player>()));
    }
  }
}

[System.Serializable]
public class GameState
{
  [SerializeField] private List<EntityState> entities;

  public List<EntityState> Entities { get => entities; set => entities = value; }

  public GameState(List<EntityState> entities)
  {
    this.entities = entities;
  }
}