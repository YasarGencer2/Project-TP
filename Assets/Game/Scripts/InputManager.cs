using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] Moveable player;
    [SerializeField] LayerMask playerLayerMask;

    [SerializeField] bool onGamepad;
    public bool OnGamepad => onGamepad;


    Action<InputAction.CallbackContext> callBacks;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
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

        inputActions.FindAction("Player/Jump").performed += callBacks => JumpPerformed();
        inputActions.FindAction("Player/Jump").canceled += callBacks => JumpCanceled(); 

    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.FindAction("Player/Jump").performed -= callBacks => JumpPerformed();
        inputActions.FindAction("Player/Jump").canceled -= callBacks => JumpCanceled();
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

    void JumpPerformed()
    {
        var jumpAction = inputActions.FindAction("Player/Jump");
        var value = jumpAction.ReadValue<float>();
        if (value == 0)
            return;
        var device = jumpAction.activeControl?.device;
        SetInputMode(device);
        player.Jump();
    }
    void JumpCanceled()
    {
        player.CancelJump();
    }
    public bool GetCrouchInput()
    {
        var crouchAction = inputActions.FindAction("Player/Crouch");
        var value = crouchAction.ReadValue<float>();
        if (value == 0)
            return false;
        var device = crouchAction.activeControl?.device;
        SetInputMode(device);
        return true;
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