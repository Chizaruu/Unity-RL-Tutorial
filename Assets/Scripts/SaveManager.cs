using UnityEngine;
using OdinSerializer;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour {
  public static SaveManager instance;

  [SerializeField] private int currentFloor = 0;
  [SerializeField] private string saveFileName = "saveThe.koala";
  [SerializeField] private SaveData save = new SaveData();

  public int CurrentFloor { get => currentFloor; set => currentFloor = value; }
  public SaveData Save { get => save; set => save = value; }


  private void Awake() {
    if (SaveManager.instance == null) {
      SaveManager.instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  public bool HasSaveAvailable() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);

    if (!File.Exists(path)) {
      return false;
    }
    return true;
  }

  public void SaveGame(bool tempSave = true) {
    save.SavedFloor = currentFloor;

    bool hasScene = save.Scenes.Find(x => x.FloorNumber == currentFloor) is not null;
    if (hasScene) {
      UpdateScene(SaveState());
    } else {
      AddScene(SaveState());
    }

    if (!tempSave) {
      string path = Path.Combine(Application.persistentDataPath, saveFileName);
      byte[] saveJson = SerializationUtility.SerializeValue(save, DataFormat.JSON); //Serialize the state to JSON
      File.WriteAllBytes(path, saveJson); //Save the state to a file
    }
  }

  public void LoadGame() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    byte[] saveJson = File.ReadAllBytes(path); //Load the state from the file
    save = SerializationUtility.DeserializeValue<SaveData>(saveJson, DataFormat.JSON); //Deserialize the state from JSON

    currentFloor = save.SavedFloor;

    if (SceneManager.GetActiveScene().name is not "Dungeon") {
      SceneManager.LoadScene("Dungeon");
    } else {
      LoadScene();
    }
  }

  public void DeleteSave() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    File.Delete(path);
  }

  public void AddScene(SceneState sceneState) => save.Scenes.Add(sceneState);

  public void UpdateScene(SceneState sceneState) => save.Scenes[currentFloor - 1] = sceneState;

  public void LoadScene(bool canRemovePlayer = true) {
    SceneState sceneState = save.Scenes.Find(x => x.FloorNumber == currentFloor);
    if (sceneState is not null) {
      LoadState(sceneState, canRemovePlayer);
    } else {
      Debug.LogError("No save data for this floor");
    }
  }

  public SceneState SaveState() => new SceneState(
    currentFloor,
    GameManager.instance.SaveState(),
    MapManager.instance.SaveState()
  );

  public void LoadState(SceneState sceneState, bool canRemovePlayer) {
    MapManager.instance.LoadState(sceneState.MapState);
    GameManager.instance.LoadState(sceneState.GameState, canRemovePlayer);
  }
}

[System.Serializable]
public class SaveData {
  [SerializeField] private int savedFloor;

  [SerializeField] private List<SceneState> scenes;

  public int SavedFloor { get => savedFloor; set => savedFloor = value; }
  public List<SceneState> Scenes { get => scenes; set => scenes = value; }

  public SaveData() {
    savedFloor = 0;
    scenes = new List<SceneState>();
  }
}

[System.Serializable]
public class SceneState {
  [SerializeField] private int floorNumber;
  [SerializeField] private GameState gameState;
  [SerializeField] private MapState mapState;
  public int FloorNumber { get => floorNumber; set => floorNumber = value; }
  public GameState GameState { get => gameState; set => gameState = value; }
  public MapState MapState { get => mapState; set => mapState = value; }

  public SceneState(int floorNumber, GameState gameState, MapState mapState) {
    this.floorNumber = floorNumber;
    this.gameState = gameState;
    this.mapState = mapState;
  }
}