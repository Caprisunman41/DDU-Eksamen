using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static bool IsBlocked;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;
    public static bool AttackWasPressed;
    public static bool SpellWasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _attackAction;
    private InputAction _spellAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _attackAction = PlayerInput.actions["Attack"];
        _spellAction = PlayerInput.actions["Spell"];
        
    }

    private void Update()
    {
        if (IsBlocked)
        {
            Movement = Vector2.zero;
            JumpWasPressed = false;
            JumpIsHeld = false;
            JumpWasReleased = false;
            RunIsHeld = false;
            DashWasPressed = false;
            AttackWasPressed = false;
            SpellWasPressed = false;
            return;
        }

        Movement = _moveAction.ReadValue<Vector2>();

        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
        DashWasPressed = _dashAction.WasPressedThisFrame();
        AttackWasPressed = _attackAction.WasPressedThisFrame();
        SpellWasPressed = _spellAction.WasPressedThisFrame();
    }
}
