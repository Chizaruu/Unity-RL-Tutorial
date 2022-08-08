using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UICastBehaviour : MonoBehaviour, ISelectHandler, IDeselectHandler {
  public virtual void OnSelect(BaseEventData eventData) {
    GameObject child = transform.GetChild(0).gameObject;
    Debug.Log($"{child.GetComponent<TextMeshProUGUI>().text} selected");
  }
  public virtual void OnDeselect(BaseEventData eventData) {
    GameObject child = transform.GetChild(0).gameObject;
    Debug.Log($"{child.GetComponent<TextMeshProUGUI>().text} deselected");
  }
}
