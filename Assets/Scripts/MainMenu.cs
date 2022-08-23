using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {
  [SerializeField] private EventSystem eventSystem;
  [SerializeField] private Button continueButton;

  private void Start() {
    if (!SaveManager.instance.HasSaveAvailable()) {
      continueButton.interactable = false;
    } else {
      eventSystem.SetSelectedGameObject(continueButton.gameObject);
    }
  }

  public void NewGame() {
    SceneManager.LoadScene("Floor 1");
  }

  public void ContinueGame() {
    SaveManager.instance.LoadGame();
  }

  public void QuitGame() {
    Application.Quit();
  }
}
