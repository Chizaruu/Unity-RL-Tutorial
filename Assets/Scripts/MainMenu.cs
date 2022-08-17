using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {
  [SerializeField] private EventSystem eventSystem;
  [SerializeField] private string saveFilePath = "";
  [SerializeField] private Button continueButton;

  private void Start() {
    saveFilePath = SaveManager.instance.GetSavePath();

    if (saveFilePath == "") {
      continueButton.interactable = false;
    } else {
      eventSystem.SetSelectedGameObject(continueButton.gameObject);
    }
  }

  public void NewGame() {
    SceneManager.LoadScene("Floor 1");
  }

  public void ContinueGame() {
    SaveManager.instance.LoadGame(saveFilePath);
  }

  public void QuitGame() {
    Application.Quit();
  }
}
