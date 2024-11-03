using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Action Assets")]
    private PlayerInput playerInput;

    [Header("Player")] 
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;
    public static bool RestartWasPressed;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction dashAction;
    private InputAction restartAction;
    
    [Header("UI")] 
    public static bool TogglePauseWasPressed;
    private InputAction togglePauseAction;

    



    
    private void Awake() {
        
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on InputManager GameObject!");
                return;
            }
        }

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
        dashAction = playerInput.actions["Dash"];
        togglePauseAction = playerInput.actions["TogglePause"];
        restartAction = playerInput.actions["Restart"];
        
    }


    private void Update() {
        
        Movement = moveAction.ReadValue<Vector2>();
        JumpWasPressed = jumpAction.WasPressedThisFrame();
        JumpIsHeld = jumpAction.IsPressed();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();
        RunIsHeld = runAction.IsPressed();
        DashWasPressed = dashAction.WasPressedThisFrame();
        TogglePauseWasPressed = togglePauseAction.WasPressedThisFrame();
        RestartWasPressed = restartAction.WasPressedThisFrame();
    }


    private void SetAction(InputAction action, string name) {
        
        if (playerInput == null) {
            Debug.LogError("PlayerInput component not found on InputManager GameObject!");
            return; 
        }
        if (playerInput.actions[name] == null) {
            Debug.LogError("InputAction " + name + " not found on PlayerInput component!");
            return; 
        }
        action = playerInput.actions[name];
    }
    
}