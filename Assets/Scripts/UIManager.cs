using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour {
  public static UIManager instance;
  [SerializeField] private EventSystem eventSystem;
  [SerializeField] private bool isMenuOpen = false; //Read-only

  [Header("Health UI")]
  [SerializeField] private Slider hpSlider;
  [SerializeField] private TextMeshProUGUI hpSliderText;

  [Header("Message UI")]
  [SerializeField] private int sameMessageCount = 0; //Read-only
  [SerializeField] private string lastMessage; //Read-only
  [SerializeField] private bool isMessageHistoryOpen = false; //Read-only
  [SerializeField] private GameObject messageHistory;
  [SerializeField] private GameObject messageHistoryContent;
  [SerializeField] private GameObject lastFiveMessagesContent;

  [Header("Inventory UI")]
  [SerializeField] private bool isInventoryOpen = false; //Read-only
  [SerializeField] private GameObject inventory;
  [SerializeField] private GameObject inventoryContent;

  [Header("Drop Menu UI")]
  [SerializeField] private bool isDropMenuOpen = false; //Read-only
  [SerializeField] private GameObject dropMenu;
  [SerializeField] private GameObject dropMenuContent;

  [Header("Cast Menu UI")]
  [SerializeField] private bool isCastMenuOpen = false; //Read-only
  [SerializeField] private GameObject castMenu;
  [SerializeField] private GameObject castMenuContent;
  public bool IsMenuOpen { get => isMenuOpen; }
  public bool IsMessageHistoryOpen { get => isMessageHistoryOpen; }
  public bool IsInventoryOpen { get => isInventoryOpen; }
  public bool IsDropMenuOpen { get => isDropMenuOpen; }
  public bool IsCastMenuOpen { get => isCastMenuOpen; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
  }

  private void Start() => AddMessage("Hello and welcome, adventurer, to yet another dungeon!", "#0da2ff"); //Light blue

  public void SetHealthMax(int maxHp) {
    hpSlider.maxValue = maxHp;
  }

  public void SetHealth(int hp, int maxHp) {
    hpSlider.value = hp;
    hpSliderText.text = $"HP: {hp}/{maxHp}";
  }

  public void ToggleMenu(Actor actor) {
    if (isCastMenuOpen) {
      ToggleCastMenu();
      ToggleInventory(actor);
      return;
    }

    if (isMenuOpen) {
      isMenuOpen = !isMenuOpen;

      if (isMessageHistoryOpen) {
        ToggleMessageHistory();
      }

      if (isInventoryOpen) {
        ToggleInventory();
      }

      if (isDropMenuOpen) {
        ToggleDropMenu();
      }
      return;
    }
  }

  public void ToggleMessageHistory() {
    messageHistory.SetActive(!messageHistory.activeSelf);
    isMenuOpen = messageHistory.activeSelf;
    isMessageHistoryOpen = messageHistory.activeSelf;
  }

  public void ToggleInventory(Actor actor = null) {
    inventory.SetActive(!inventory.activeSelf);
    if (!isCastMenuOpen) {
      isMenuOpen = inventory.activeSelf;
    }
    isInventoryOpen = inventory.activeSelf;

    if (isMenuOpen) {
      UpdateMenu(actor, inventoryContent);
    }
  }

  public void ToggleDropMenu(Actor actor = null) {
    dropMenu.SetActive(!dropMenu.activeSelf);
    isMenuOpen = dropMenu.activeSelf;
    isDropMenuOpen = dropMenu.activeSelf;

    if (isMenuOpen) {
      UpdateMenu(actor, dropMenuContent);
    }
  }

  public void ToggleCastMenu(Actor actor = null, Consumable consumable = null) {
    castMenu.SetActive(!castMenu.activeSelf);
    isMenuOpen = castMenu.activeSelf;
    isCastMenuOpen = castMenu.activeSelf;
    if (actor == null) {
      return;
    }

    if (isMenuOpen) {
      UpdateMenu(null, castMenuContent);

      char c = 'a';
      int castContentChildNum = 0;
      for (int actorNum = 0; actorNum < GameManager.instance.Actors.Count; actorNum++) {
        Actor target = GameManager.instance.Actors[actorNum];
        Vector3Int targetPosition = MapManager.instance.FloorMap.WorldToCell(target.transform.position);
        if (target.GetComponent<Player>() || !GetComponent<Actor>().FieldOfView.Contains(targetPosition)) {
          continue;
        }
        GameObject castContentChild = castMenuContent.transform.GetChild(castContentChildNum).gameObject;
        castContentChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"({c++}) {target.name}";
        castContentChild.GetComponent<Button>().onClick.AddListener(() => {
          Action.CastAction(actor, target, consumable);
        });
        castContentChild.SetActive(true);
      }
      eventSystem.SetSelectedGameObject(castMenuContent.transform.GetChild(0).gameObject);
    }
  }

  public void AddMessage(string newMessage, string colorHex) {
    if (lastMessage == newMessage) {
      TextMeshProUGUI messageHistoryLastChild = messageHistoryContent.transform.GetChild(messageHistoryContent.transform.childCount - 1).GetComponent<TextMeshProUGUI>();
      TextMeshProUGUI lastFiveHistoryLastChild = lastFiveMessagesContent.transform.GetChild(lastFiveMessagesContent.transform.childCount - 1).GetComponent<TextMeshProUGUI>();
      messageHistoryLastChild.text = $"{newMessage} (x{++sameMessageCount})";
      lastFiveHistoryLastChild.text = $"{newMessage} (x{sameMessageCount})";
      return;
    } else if (sameMessageCount > 0) {
      sameMessageCount = 0;
    }

    lastMessage = newMessage;

    TextMeshProUGUI messagePrefab = Instantiate(Resources.Load<TextMeshProUGUI>("Message")) as TextMeshProUGUI;
    messagePrefab.text = newMessage;
    messagePrefab.color = GetColorFromHex(colorHex);
    messagePrefab.transform.SetParent(messageHistoryContent.transform, false);

    for (int i = 0; i < lastFiveMessagesContent.transform.childCount; i++) {
      if (messageHistoryContent.transform.childCount - 1 < i) {
        return;
      }

      TextMeshProUGUI lastFiveHistoryChild = lastFiveMessagesContent.transform.GetChild(lastFiveMessagesContent.transform.childCount - 1 - i).GetComponent<TextMeshProUGUI>();
      TextMeshProUGUI messageHistoryChild = messageHistoryContent.transform.GetChild(messageHistoryContent.transform.childCount - 1 - i).GetComponent<TextMeshProUGUI>();
      lastFiveHistoryChild.text = messageHistoryChild.text;
      lastFiveHistoryChild.color = messageHistoryChild.color;
    }
  }

  private Color GetColorFromHex(string v) {
    Color color;
    if (ColorUtility.TryParseHtmlString(v, out color)) {
      return color;
    } else {
      Debug.Log("GetColorFromHex: Could not parse color from string");
      return Color.white;
    }
  }

  private void UpdateMenu(Actor actor = null, GameObject menuContent = null) {
    if (menuContent == null) {
      Debug.Log("UpdateMenu: menuContent is null");
      return;
    }

    for (int i = 0; i < menuContent.transform.childCount; i++) {
      GameObject menuContentChild = menuContent.transform.GetChild(i).gameObject;
      menuContentChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
      menuContentChild.GetComponent<Button>().onClick.RemoveAllListeners();
      menuContentChild.SetActive(false);
    }

    if (menuContent != castMenuContent) {
      return;
    }

    char c = 'a';
    for (int i = 0; i < actor.Inventory.Items.Count; i++) {
      GameObject menuContentChild = menuContent.transform.GetChild(i).gameObject;
      menuContentChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"({c++}) {actor.Inventory.Items[i].name}";
      menuContentChild.GetComponent<Button>().onClick.AddListener(() => {
        if (menuContent == inventoryContent) {
          Action.UseAction(actor, i - 1);
        } else if (menuContent == dropMenuContent) {
          Action.DropAction(actor, actor.Inventory.Items[i - 1]);
        }
        UpdateMenu(actor, menuContent);
      });
      menuContentChild.SetActive(true);
    }
    eventSystem.SetSelectedGameObject(menuContent.transform.GetChild(0).gameObject);
  }
}

