using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Actor))]
sealed class Player : MonoBehaviour, Controls.IPlayerActions {
  private Controls controls;

  [SerializeField] private bool moveKeyHeld; //read-only
  [SerializeField] private bool targetMode; //read-only
  [SerializeField] private GameObject targetObject;

  private void Awake() => controls = new Controls();

  private void OnEnable() {
    controls.Player.SetCallbacks(this);
    controls.Player.Enable();
  }

  private void OnDisable() {
    controls.Player.SetCallbacks(null);
    controls.Player.Disable();
  }

  void Controls.IPlayerActions.OnMovement(InputAction.CallbackContext context) {
    if (context.started && GetComponent<Actor>().IsAlive)
      moveKeyHeld = true;
    else if (context.canceled)
      moveKeyHeld = false;
  }

  void Controls.IPlayerActions.OnExit(InputAction.CallbackContext context) {
    if (context.performed) {
      if (UIManager.instance.IsMenuOpen) {
        UIManager.instance.ToggleMenu(GetComponent<Actor>());
      } else if (targetMode) {
        ToggleTargetMode();
      }
    }
  }

  public void OnView(InputAction.CallbackContext context) {
    if (context.performed) {
      if (CanAct() || UIManager.instance.IsMessageHistoryOpen) {
        UIManager.instance.ToggleMessageHistory();
      }
    }
  }

  public void OnPickup(InputAction.CallbackContext context) {
    if (context.performed && CanAct()) {
      Action.PickupAction(GetComponent<Actor>());
    }
  }

  public void OnInventory(InputAction.CallbackContext context) {
    if (context.performed) {
      if (CanAct() || UIManager.instance.IsInventoryOpen) {
        if (GetComponent<Inventory>().Items.Count > 0) {
          UIManager.instance.ToggleInventory(GetComponent<Actor>());
        } else {
          UIManager.instance.AddMessage("You have no items.", "#808080");
        }
      }
    }
  }

  public void OnDrop(InputAction.CallbackContext context) {
    if (context.performed) {
      if (CanAct() || UIManager.instance.IsDropMenuOpen) {
        if (GetComponent<Inventory>().Items.Count > 0) {
          UIManager.instance.ToggleDropMenu(GetComponent<Actor>());
        } else {
          UIManager.instance.AddMessage("You have no items.", "#808080");
        }
      }
    }
  }

  public void OnConfirm(InputAction.CallbackContext context) {
    if (context.performed) {
      if (targetMode) {
        Action.CastAction(GetComponent<Actor>(), targetObject.transform.position, GetComponent<Inventory>().SelectedConsumable);
      }
    }
  }

  public void ToggleTargetMode(bool isArea = false, int radius = 1) {
    targetMode = !targetMode;

    if (targetMode) {
      if (targetObject.transform.position != transform.position) {
        targetObject.transform.position = transform.position;
      }

      if (isArea) {
        targetObject.transform.GetChild(0).gameObject.SetActive(true);
        targetObject.transform.GetChild(0).localScale = Vector3.one * (radius + 1); //+1 to account for the center
      }

      targetObject.SetActive(true);
    } else {
      if (targetObject.transform.GetChild(0).gameObject.activeSelf) {
        targetObject.transform.GetChild(0).gameObject.SetActive(false);
      }
      targetObject.SetActive(false);
      GetComponent<Inventory>().SelectedConsumable = null;
    }
  }

  private void FixedUpdate() {
    if (!UIManager.instance.IsMenuOpen) {
      if (GameManager.instance.IsPlayerTurn && moveKeyHeld && GetComponent<Actor>().IsAlive) {
        Move();
      }
    }
  }

  private void Move() {
    Vector2 direction = controls.Player.Movement.ReadValue<Vector2>();
    Vector2 roundedDirection = new Vector2(Mathf.Round(direction.x), Mathf.Round(direction.y));
    Vector3 futurePosition;

    if (targetMode) {
      futurePosition = targetObject.transform.position + (Vector3)roundedDirection;
    } else {
      futurePosition = transform.position + (Vector3)roundedDirection;
    }

    if (targetMode) {
      Vector3Int targetGridPosition = MapManager.instance.FloorMap.WorldToCell(futurePosition);

      if (MapManager.instance.IsValidPosition(futurePosition) && GetComponent<Actor>().FieldOfView.Contains(targetGridPosition)) {
        targetObject.transform.position = futurePosition;
      }
    } else {
      moveKeyHeld = Action.BumpAction(GetComponent<Actor>(), roundedDirection); //If we bump into an entity, moveKeyHeld is set to false.
    }
  }

  private bool CanAct() {
    if (targetMode || UIManager.instance.IsMenuOpen || !GetComponent<Actor>().IsAlive) {
      return false;
    } else {
      return true;
    }
  }
}
