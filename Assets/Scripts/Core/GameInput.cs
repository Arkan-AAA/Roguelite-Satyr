using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerAttack;
    public event Action OnPlayerAttackHeld;
    public event Action OnPlayerAttackReleased;
    public event Action OnPlayerDash;

    // События для смены оружия
    public event Action OnNextWeapon;
    public event Action OnPreviousWeapon;
    public event Action<int> OnWeaponSlot; // 0, 1, 2

    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        // Атака
        playerInputActions.Player.Fire.started += PlayerAttack_started;
        playerInputActions.Player.Fire.performed += _ => OnPlayerAttackHeld?.Invoke();
        playerInputActions.Player.Fire.canceled += _ => OnPlayerAttackReleased?.Invoke();

        // Дэш
        playerInputActions.Player.Dash.started += _ => OnPlayerDash?.Invoke();

        // Переключение оружия
        playerInputActions.Player.NextWeapon.performed += _ => OnNextWeapon?.Invoke();
        playerInputActions.Player.PreviousWeapon.performed += _ => OnPreviousWeapon?.Invoke();

        // Слоты 1-3
        playerInputActions.Player.Slot1.performed += _ => OnWeaponSlot?.Invoke(0);
        playerInputActions.Player.Slot2.performed += _ => OnWeaponSlot?.Invoke(1);
        playerInputActions.Player.Slot3.performed += _ => OnWeaponSlot?.Invoke(2);
    }

    private void PlayerAttack_started(InputAction.CallbackContext obj) {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector3 GetMousePosition() {
        return Mouse.current.position.ReadValue();
    }

    public Vector2 GetGamepadLookVector() {
        if (Gamepad.current == null) return Vector2.zero;
        return Gamepad.current.rightStick.ReadValue();
    }

    public bool IsGamepadActive() {
        return Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.1f;
    }

    public Vector2 GetGamepadMoveVector() {
        if (Gamepad.current == null) return Vector2.zero;
        return Gamepad.current.leftStick.ReadValue();
    }

    public bool IsGamepadMoving() {
        return Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f;
    }

    public void DisableInput() {
        playerInputActions.Player.Disable();
    }

    public void EnableInput() {
        playerInputActions.Player.Enable();
    }
}