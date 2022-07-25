using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
  public static UIManager instance;

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

  public bool IsMessageHistoryOpen { get => isMessageHistoryOpen; }

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

  public void ToggleMessageHistory() {
    messageHistory.SetActive(!messageHistory.activeSelf);
    isMessageHistoryOpen = messageHistory.activeSelf;
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
}

