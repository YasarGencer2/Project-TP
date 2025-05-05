using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] LayerMask playerLayerMask;

    [SerializeField] bool onGamepad;
    public bool OnGamepad => onGamepad;


    Action<InputAction.CallbackContext> gamepadCallback;
    Action<InputAction.CallbackContext> keyboardCallback;

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
        }
    }
    void SetInputMode(InputControl control)
    {
        var value = control.device is Gamepad;
        SetInputMode(value);
    }
    void SetInputMode(bool value)
    {
        if (onGamepad == value)
            return;
        onGamepad = value;
        Debug.Log("Gamepad mode: " + (value ? "Enabled" : "Disabled"));
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // gamepadCallback = ctx => SetInputMode(true);
        // keyboardCallback = ctx => SetInputMode(false);

        // inputActions.FindAction("Player/GamepadPress").performed += gamepadCallback;
        // inputActions.FindAction("Player/KeyboardPress").performed += keyboardCallback;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        // inputActions.FindAction("Player/GamepadPress").performed -= gamepadCallback;
        // inputActions.FindAction("Player/KeyboardPress").performed -= keyboardCallback;
    }

    public Vector2 GetMovementInput()
    {
        var moveAction = inputActions.FindAction("Player/Move");
        var value = moveAction.ReadValue<Vector2>();
        if (value == Vector2.zero) return value;
        var device = moveAction.activeControl?.device;
        SetInputMode(device);
        return value;
    }

    public bool GetJumpInput()
    {
        var jumpAction = inputActions.FindAction("Player/Jump");
        var value = jumpAction.ReadValue<float>();
        if (value == 0) return false;
        var device = jumpAction.activeControl?.device;
        SetInputMode(device);
        return inputActions.FindAction("Player/Jump").ReadValue<float>() == 1;
    }
    public Vector3 GetMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100, ~playerLayerMask))
        {
            return new Vector3(hit.point.x, 0, hit.point.z);
        }
        var worldPos = ray.GetPoint(100);
        SetInputMode(false);
        return new Vector3(worldPos.x, 0, worldPos.z);
    }
    public Vector2 GetLookInput()
    {
        var lookAction = inputActions.FindAction("Player/Look");
        var value = lookAction.ReadValue<Vector2>();
        SetInputMode(true);
        return value;
    }
}