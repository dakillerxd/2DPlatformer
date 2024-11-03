using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    [Header("Mouse Inversion")]
    public bool invertMouseY = false;
    public bool invertMouseX = false;

    [Header("Gameplay")]
    private InputActionMap gameplayActions;
    private InputAction togglePauseAction;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction throwItemAction;
    private InputAction throwWeaponAction;
    private InputAction throwDisable;
    private InputAction cycleItemForwardAction;
    private InputAction cycleItemBackwardAction;

    public event Action OnTogglePause;
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJump;
    public event Action<bool> OnSprint;
    public event Action OnInteract;
    public event Action OnCycleItemForward;
    public event Action OnCycleItemBackward;
    public event Action OnThrowDisable;

    public event Action<InputAction.CallbackContext> OnThrowItemStarted;
    public event Action<InputAction.CallbackContext> OnThrowItemPerformed;
    public event Action<InputAction.CallbackContext> OnThrowItemCanceled;
    public event Action<InputAction.CallbackContext> OnThrowWeaponStarted;
    public event Action<InputAction.CallbackContext> OnThrowWeaponPerformed;
    public event Action<InputAction.CallbackContext> OnThrowWeaponCanceled;

    private Vector2 _currentLookDelta;
    private bool _isSprintPressed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on InputManager GameObject!");
                return;
            }
        }

        InitializeActions();
    }

    private void InitializeActions()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerInput or its actions are null!");
            return;
        }

        gameplayActions = playerInput.actions.FindActionMap("Gameplay", throwIfNotFound: false);

        moveAction = SafeFindAction(gameplayActions, "Move");
        lookAction = SafeFindAction(gameplayActions, "Look");
        jumpAction = SafeFindAction(gameplayActions, "Jump");
        sprintAction = SafeFindAction(gameplayActions, "Sprint");
        interactAction = SafeFindAction(gameplayActions, "Interact");
        togglePauseAction = SafeFindAction(gameplayActions, "TogglePause");
        throwItemAction = SafeFindAction(gameplayActions, "ThrowItem");
        throwDisable = SafeFindAction(gameplayActions, "DisableThrow");
        throwWeaponAction = SafeFindAction(gameplayActions, "ThrowWeapon");
        cycleItemForwardAction = SafeFindAction(gameplayActions, "CycleItemForward");
        cycleItemBackwardAction = SafeFindAction(gameplayActions, "CycleItemBackward");

        SetupActionCallback(moveAction, ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>()));
        SetupActionCallback(lookAction, ctx => OnLook?.Invoke(ctx.ReadValue<Vector2>()));
        SetupActionCallback(jumpAction, ctx => { if (ctx.performed) OnJump?.Invoke(); });
        SetupActionCallback(sprintAction, ctx => 
        {
            _isSprintPressed = ctx.ReadValueAsButton();
            OnSprint?.Invoke(_isSprintPressed);
        });
        SetupActionCallback(interactAction, ctx => { if (ctx.performed) OnInteract?.Invoke(); });
        SetupActionCallback(togglePauseAction, ctx => { if (ctx.performed) OnTogglePause?.Invoke(); });
        SetupActionCallback(throwItemAction, ctx =>
        {
            if (ctx.started) OnThrowItemStarted?.Invoke(ctx);
            if (ctx.performed) OnThrowItemPerformed?.Invoke(ctx);
            if (ctx.canceled) OnThrowItemCanceled?.Invoke(ctx);
        });
        SetupActionCallback(throwWeaponAction, ctx =>
        {
            if (ctx.started) OnThrowWeaponStarted?.Invoke(ctx);
            if (ctx.performed) OnThrowWeaponPerformed?.Invoke(ctx);
            if (ctx.canceled) OnThrowWeaponCanceled?.Invoke(ctx);
        });
        SetupActionCallback(cycleItemForwardAction, ctx => { if (ctx.performed) OnCycleItemForward?.Invoke(); });
        SetupActionCallback(cycleItemBackwardAction, ctx => { if (ctx.performed) OnCycleItemBackward?.Invoke(); });
        SetupActionCallback(throwDisable, ctx => { if (ctx.performed) OnThrowDisable?.Invoke(); });

        
    }

    private InputAction SafeFindAction(InputActionMap actionMap, string actionName)
    {
        var action = actionMap.FindAction(actionName, throwIfNotFound: false);
        if (action == null)
        {
            Debug.LogWarning($"Action '{actionName}' not found in action map '{actionMap.name}'");
        }
        return action;
    }

    private void SetupActionCallback(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        if (action != null)
        {
            action.performed += callback;
            action.canceled += callback;
        }
    }

    private void OnEnable()
    {
        gameplayActions?.Enable();
    }

    private void OnDisable()
    {
        gameplayActions?.Disable();
    }

    public Vector2 GetMovementInput() => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 GetLookInput()
    {
        Vector2 lookInput = lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        lookInput.x *= invertMouseX ? -1 : 1;
        lookInput.y *= invertMouseY ? -1 : 1;
        return lookInput;
    }

    public bool IsJumping() => jumpAction?.IsPressed() ?? false;
    public bool IsSprinting() => _isSprintPressed;
    public bool IsInteracting() => interactAction?.IsPressed() ?? false;

    public bool IsCurrentDeviceMouse()
    {
        return playerInput.currentControlScheme == "KeyboardMouse";
    }

    public void SwitchActionMap(string actionMapName)
    {
        playerInput.SwitchCurrentActionMap(actionMapName);
    }


    public void ToggleMouseYInversion()
    {
        invertMouseY = !invertMouseY;
    }

    public void ToggleMouseXInversion()
    {
        invertMouseX = !invertMouseX;
    }
    
}