using UnityEngine;
using OdinSerializer;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour, IState<SceneState> {
  public static SaveManager instance;

  [SerializeField] private string saveFileName = "saveThe.koala";
  [SerializeField] private SaveData save;

  public SaveData Save { get => save; set => save = value; }

  private void Awake() {
    if (SaveManager.instance == null) {
      SaveManager.instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }

    if (HasSaveAvailable()) {
      SceneState sceneState = save.Scenes.Find(x => x.Name == SceneManager.GetActiveScene().name);
      if (sceneState != null) {
        LoadState(sceneState);
      }
    } else {
      save = new SaveData();
    }
  }

  public bool HasSaveAvailable() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);

    if (!File.Exists(path)) {
      return false;
    }
    return true;
  }

  public void SaveGame() {
    bool hasScene = save.Scenes.Find(x => x.Name == SceneManager.GetActiveScene().name) != null;
    if (hasScene) {
      UpdateScene(SaveState());
    } else {
      AddScene(SaveState());
    }

    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    byte[] saveJson = SerializationUtility.SerializeValue(save, DataFormat.JSON); //Serialize the state to JSON
    File.WriteAllBytes(path, saveJson); //Save the state to a file
  }

  public void LoadGame() {
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    byte[] saveJson = File.ReadAllBytes(path); //Load the state from the file
    save = SerializationUtility.DeserializeValue<SaveData>(saveJson, DataFormat.JSON); //Deserialize the state from JSON

    SceneManager.LoadScene(save.CurrentScene);
  }

  public void AddScene(SceneState sceneState) {
    save.Scenes.Add(sceneState);
  }

  public void UpdateScene(SceneState sceneState) {
    int index = save.Scenes.FindIndex(x => x.Name == sceneState.Name);
    save.Scenes[index] = sceneState;
  }

  public SceneState SaveState() => new SceneState(
    SceneManager.GetActiveScene().name,
    GameManager.instance.SaveState(),
    MapManager.instance.SaveState()
  );

  public void LoadState(SceneState sceneData) {
    GameManager.instance.LoadState(sceneData.GameState);
    MapManager.instance.LoadState(sceneData.MapState);
  }
}

[System.Serializable]
public class SaveData {
  [SerializeField] private string currentScene;
  [SerializeField] private List<SceneState> scenes;

  public string CurrentScene { get => currentScene; set => currentScene = value; }
  public List<SceneState> Scenes { get => scenes; set => scenes = value; }

  public SaveData() {
    currentScene = "";
    scenes = new List<SceneState>();
  }
}

[System.Serializable]
public class SceneState {
  [SerializeField] private string name;
  [SerializeField] private GameState gameState;
  [SerializeField] private MapState mapState;
  public string Name { get => name; set => name = value; }
  public GameState GameState { get => gameState; set => gameState = value; }
  public MapState MapState { get => mapState; set => mapState = value; }

  public SceneState(string name, GameState gameState, MapState mapState) {
    this.name = name;
    this.gameState = gameState;
    this.mapState = mapState;
  }
}
