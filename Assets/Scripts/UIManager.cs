using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour {
  public static UIManager instance;
  [SerializeField] private EventSystem eventSystem;
  [SerializeField] private bool isMenuOpen = false; //Read-only
  [SerializeField] private TextMeshProUGUI dungeonFloorText;

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

  [Header("Escape Menu UI")]
  [SerializeField] private bool isEscapeMenuOpen = false; //Read-only
  [SerializeField] private GameObject escapeMenu;

  [Header("Character Information Menu UI")]
  [SerializeField] private bool isCharacterInformationMenuOpen = false; //Read-only
  [SerializeField] private GameObject characterInformationMenu;

  [Header("Level Up Menu UI")]
  [SerializeField] private bool isLevelUpMenuOpen = false; //Read-only
  [SerializeField] private GameObject levelUpMenu;
  [SerializeField] private GameObject levelUpMenuContent;

  public bool IsMenuOpen { get => isMenuOpen; }
  public bool IsMessageHistoryOpen { get => isMessageHistoryOpen; }
  public bool IsInventoryOpen { get => isInventoryOpen; }
  public bool IsDropMenuOpen { get => isDropMenuOpen; }
  public bool IsEscapeMenuOpen { get => isEscapeMenuOpen; }
  public bool IsCharacterInformationMenuOpen { get => isCharacterInformationMenuOpen; }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else {
      Destroy(gameObject);
    }
  }

  private void Start() {
    SetDungeonFloorText(SaveManager.instance.CurrentFloor);

    if (SaveManager.instance.Save.SavedFloor is 0) {
      AddMessage("Hello and welcome, adventurer, to yet another dungeon!", "#0da2ff"); //Light blue
    } else {
      AddMessage("Welcome back, adventurer!", "#0da2ff"); //Light blue
    }
  }

  public void SetHealthMax(int maxHp) {
    hpSlider.maxValue = maxHp;
  }

  public void SetHealth(int hp, int maxHp) {
    hpSlider.value = hp;
    hpSliderText.text = $"HP: {hp}/{maxHp}";
  }

  public void SetDungeonFloorText(int floor) {
    dungeonFloorText.text = $"Dungeon floor: {floor}";
  }

  public void ToggleMenu() {
    if (isMenuOpen) {
      isMenuOpen = !isMenuOpen;

      switch (true) {
        case bool _ when isMessageHistoryOpen:
          ToggleMessageHistory();
          break;
        case bool _ when isInventoryOpen:
          ToggleInventory();
          break;
        case bool _ when isDropMenuOpen:
          ToggleDropMenu();
          break;
        case bool _ when isEscapeMenuOpen:
          ToggleEscapeMenu();
          break;
        case bool _ when isCharacterInformationMenuOpen:
          ToggleCharacterInformationMenu();
          break;
        default:
          break;
      }
    }
  }

  public void ToggleMessageHistory() {
    isMessageHistoryOpen = !isMessageHistoryOpen;
    SetBooleans(messageHistory, isMessageHistoryOpen);
  }

  public void ToggleInventory(Actor actor = null) {
    isInventoryOpen = !isInventoryOpen;
    SetBooleans(inventory, isInventoryOpen);

    if (isMenuOpen) {
      UpdateMenu(actor, inventoryContent);
    }
  }

  public void ToggleDropMenu(Actor actor = null) {
    isDropMenuOpen = !isDropMenuOpen;
    SetBooleans(dropMenu, isDropMenuOpen);

    if (isMenuOpen) {
      UpdateMenu(actor, dropMenuContent);
    }
  }

  public void ToggleEscapeMenu() {
    isEscapeMenuOpen = !isEscapeMenuOpen;
    SetBooleans(escapeMenu, isEscapeMenuOpen);

    eventSystem.SetSelectedGameObject(escapeMenu.transform.GetChild(0).gameObject);
  }

  public void ToggleLevelUpMenu(Actor actor) {
    isLevelUpMenuOpen = !isLevelUpMenuOpen;
    SetBooleans(levelUpMenu, isLevelUpMenuOpen);

    GameObject constitutionButton = levelUpMenuContent.transform.GetChild(0).gameObject;
    GameObject strengthButton = levelUpMenuContent.transform.GetChild(1).gameObject;
    GameObject agilityButton = levelUpMenuContent.transform.GetChild(2).gameObject;

    constitutionButton.GetComponent<TextMeshProUGUI>().text = $"a) Constitution (+20 HP, from {actor.GetComponent<Fighter>().MaxHp})";
    strengthButton.GetComponent<TextMeshProUGUI>().text = $"b) Strength (+1 attack, from {actor.GetComponent<Fighter>().Power()})";
    agilityButton.GetComponent<TextMeshProUGUI>().text = $"c) Agility (+1 defense, from {actor.GetComponent<Fighter>().Defense()})";

    foreach (Transform child in levelUpMenuContent.transform) {
      child.GetComponent<Button>().onClick.RemoveAllListeners();

      child.GetComponent<Button>().onClick.AddListener(() => {
        if (constitutionButton == child.gameObject) {
          actor.GetComponent<Level>().IncreaseMaxHp();
        } else if (strengthButton == child.gameObject) {
          actor.GetComponent<Level>().IncreasePower();
        } else if (agilityButton == child.gameObject) {
          actor.GetComponent<Level>().IncreaseDefense();
        } else {
          Debug.LogError("No button found!");
        }
        ToggleLevelUpMenu(actor);
      });
    }

    eventSystem.SetSelectedGameObject(levelUpMenuContent.transform.GetChild(0).gameObject);
  }

  public void ToggleCharacterInformationMenu(Actor actor = null) {
    isCharacterInformationMenuOpen = !isCharacterInformationMenuOpen;
    SetBooleans(characterInformationMenu, isCharacterInformationMenuOpen);

    if (actor is not null) {
      characterInformationMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Level: {actor.GetComponent<Level>().CurrentLevel}";
      characterInformationMenu.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"XP: {actor.GetComponent<Level>().CurrentXp}";
      characterInformationMenu.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"XP for next level: {actor.GetComponent<Level>().XpToNextLevel}";
      characterInformationMenu.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"Attack: {actor.GetComponent<Fighter>().Power()}";
      characterInformationMenu.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"Defense: {actor.GetComponent<Fighter>().Defense()}";
    }
  }

  private void SetBooleans(GameObject menu, bool menuBool) {
    isMenuOpen = menuBool;
    menu.SetActive(menuBool);
  }

  public void Save() {
    SaveManager.instance.SaveGame(false);
    AddMessage("The world stops for a moment as you save your progress.", "#0da2ff"); //Light blue
  }

  public void Load() {
    SaveManager.instance.LoadGame();
    AddMessage("You go back in time to the last time you saved.", "#0da2ff"); //Light blue
    ToggleMenu();
  }

  public void Quit() {
    Application.Quit();
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

  private void UpdateMenu(Actor actor, GameObject menuContent) {
    for (int resetNum = 0; resetNum < menuContent.transform.childCount; resetNum++) {
      GameObject menuContentChild = menuContent.transform.GetChild(resetNum).gameObject;
      menuContentChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
      menuContentChild.GetComponent<Button>().onClick.RemoveAllListeners();
      menuContentChild.SetActive(false);
    }

    char c = 'a';

    for (int itemNum = 0; itemNum < actor.Inventory.Items.Count; itemNum++) {
      GameObject menuContentChild = menuContent.transform.GetChild(itemNum).gameObject;
      Item item = actor.Inventory.Items[itemNum];
      menuContentChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"({c++}) {item.name}";
      menuContentChild.GetComponent<Button>().onClick.AddListener(() => {
        if (menuContent == inventoryContent) {
          if (item.Consumable is not null) {
            Action.UseAction(actor, item);
          } else if (item.Equippable is not null) {
            Action.EquipAction(actor, item);
          }
        } else if (menuContent == dropMenuContent) {
          Action.DropAction(actor, item);
        }
      });
      menuContentChild.SetActive(true);
    }
    eventSystem.SetSelectedGameObject(menuContent.transform.GetChild(0).gameObject);
  }
}

