using UnityEngine;
using OdinSerializer;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour {
  public static SaveManager instance;

  private void Awake() {
    if (SaveManager.instance == null) {
      SaveManager.instance = this;
      DontDestroyOnLoad(gameObject);
    } else {
      Destroy(gameObject);
    }
  }

  public string GetSavePath() {
    string path = Path.Combine(Application.persistentDataPath, "playerSave.humble");

    if (!File.Exists(path)) {
      return "";
    }
    return path;
  }

  public void SaveGame() {
    string path = Path.Combine(Application.persistentDataPath, "playerSave.humble");

    byte[] saveJson = SerializationUtility.SerializeValue("PUT STUFF HERE!", DataFormat.JSON); //Serialize the state to JSON
    File.WriteAllBytes(path, saveJson); //Save the state to a file
  }

  public void LoadGame(string path) {
  }
}

public class SaveData {
  [SerializeField] private string currentFloor;
  [SerializeField] private List<ActorState> actors;
}
