using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Actor))]
sealed class Player : MonoBehaviour, Controls.IPlayerActions {
  private Controls controls;

  [SerializeField] private bool moveKeyDown; //read-only
  [SerializeField] private bool targetMode; //read-only
  [SerializeField] private bool isSingleTarget; //read-only
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
    if (context.started && GetComponent<Actor>().IsAlive) {
      if (targetMode && !moveKeyDown) {
        moveKeyDown = true;
        Move();
      } else if (!targetMode) {
        moveKeyDown = true;
      }
    } else if (context.canceled) {
      moveKeyDown = false;
    }
  }

  void Controls.IPlayerActions.OnExit(InputAction.CallbackContext context) {
    if (context.performed) {
      if (targetMode) {
        ToggleTargetMode();
      } else if (!UIManager.instance.IsEscapeMenuOpen && !UIManager.instance.IsMenuOpen) {
        UIManager.instance.ToggleEscapeMenu();
      } else if (UIManager.instance.IsMenuOpen) {
        UIManager.instance.ToggleMenu();
      }
    }
  }

  public void OnView(InputAction.CallbackContext context) {
    if (context.performed) {
      if (!UIManager.instance.IsMenuOpen || UIManager.instance.IsMessageHistoryOpen) {
        UIManager.instance.ToggleMessageHistory();
      }
    }
  }

  public void OnPickup(InputAction.CallbackContext context) {
    if (context.performed) {
      if (CanAct()) {
        Action.PickupAction(GetComponent<Actor>());
      }
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
        if (isSingleTarget) {
          Actor target = SingleTargetChecks(targetObject.transform.position);

          if (target != null) {
            Action.CastAction(GetComponent<Actor>(), target, GetComponent<Inventory>().SelectedConsumable);
          }
        } else {
          List<Actor> targets = AreaTargetChecks(targetObject.transform.position);

          if (targets != null) {
            Action.CastAction(GetComponent<Actor>(), targets, GetComponent<Inventory>().SelectedConsumable);
          }
        }
      } else if (CanAct()) {
        Action.TakeStairsAction(GetComponent<Actor>());
      }
    }
  }

  public void OnInfo(InputAction.CallbackContext context) {
    if (context.performed) {
      if (CanAct() || UIManager.instance.IsCharacterInformationMenuOpen) {
        UIManager.instance.ToggleCharacterInformationMenu(GetComponent<Actor>());
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
        isSingleTarget = false;
        targetObject.transform.GetChild(0).localScale = Vector3.one * (radius + 1); //+1 to account for the center
        targetObject.transform.GetChild(0).gameObject.SetActive(true);
      } else {
        isSingleTarget = true;
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
    if (!UIManager.instance.IsMenuOpen && !targetMode) {
      if (GameManager.instance.IsPlayerTurn && moveKeyDown && GetComponent<Actor>().IsAlive) {
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
      moveKeyDown = Action.BumpAction(GetComponent<Actor>(), roundedDirection); //If we bump into an entity, moveKeyHeld is set to false.
    }
  }

  private bool CanAct() {
    if (targetMode || UIManager.instance.IsMenuOpen || !GetComponent<Actor>().IsAlive) {
      return false;
    } else {
      return true;
    }
  }

  private Actor SingleTargetChecks(Vector3 targetPosition) {
    Actor target = GameManager.instance.GetActorAtLocation(targetPosition);

    if (target == null) {
      UIManager.instance.AddMessage("You must select an enemy to target.", "#FFFFFF");
      return null;
    }

    if (target == GetComponent<Actor>()) {
      UIManager.instance.AddMessage("You can't target yourself!", "#FFFFFF");
      return null;
    }

    return target;
  }

  private List<Actor> AreaTargetChecks(Vector3 targetPosition) {
    //Take away 1 to account for the center
    int radius = (int)targetObject.transform.GetChild(0).localScale.x - 1;

    Bounds targetBounds = new Bounds(targetPosition, Vector3.one * radius * 2);
    List<Actor> targets = new List<Actor>();

    foreach (Actor target in GameManager.instance.Actors) {
      if (targetBounds.Contains(target.transform.position)) {
        targets.Add(target);
      }
    }

    if (targets.Count == 0) {
      UIManager.instance.AddMessage("There are no targets in the radius.", "#FFFFFF");
      return null;
    }

    return targets;
  }
}
