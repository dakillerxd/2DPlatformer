using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Action Assets")]
    private PlayerInput _playerInput;

    [Header("Player")] 
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool DashWasPressed;
    public static bool RestartWasPressed;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction restartAction;
    
    [Header("UI")] 
    public static bool TogglePauseWasPressed;
    public static bool ClickWasPressed;
    public static bool CancelWasPressed;
    private InputAction togglePauseAction;
    private InputAction clickAction;
    private InputAction cancelAction;

    



    
    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        

        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on InputManager GameObject!");
                return;
            }
        }

        // Player
        moveAction = _playerInput.actions["Move"];
        jumpAction = _playerInput.actions["Jump"];
        dashAction = _playerInput.actions["Dash"];
        restartAction = _playerInput.actions["Restart"];
        
        // UI
        togglePauseAction = _playerInput.actions["TogglePause"];
        clickAction = _playerInput.actions["Click"];
        cancelAction = _playerInput.actions["Cancel"];
        
    }


    private void Update() {
        
        // Player
        Movement = moveAction.ReadValue<Vector2>();
        JumpWasPressed = jumpAction.WasPressedThisFrame();
        JumpIsHeld = jumpAction.IsPressed();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();
        DashWasPressed = dashAction.WasPressedThisFrame();
        TogglePauseWasPressed = togglePauseAction.WasPressedThisFrame();
        
        // UI
        RestartWasPressed = restartAction.WasPressedThisFrame();
        ClickWasPressed = clickAction.WasPressedThisFrame();
        CancelWasPressed = cancelAction.WasPressedThisFrame();
    }
}