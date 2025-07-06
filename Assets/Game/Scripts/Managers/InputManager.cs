using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.iOS;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] CharacterController player;
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
        var value = !(control.device is not Gamepad);
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

        var jump = inputActions.FindAction("Player/Jump");
        var crouch = inputActions.FindAction("Player/Crouch");
        var basicAttack = inputActions.FindAction("Player/BasicAttack");

        jump.performed += callBacks => JumpPerformed();
        jump.canceled += callBacks => JumpCanceled();
        crouch.performed += callBacks => CrouchPerformed();
        crouch.canceled += callBacks => CrouchCanceled();
        basicAttack.performed += callBacks => BasicAttack();

        jump.performed += callBacks => SetInputMode(jump.activeControl?.device);
        crouch.performed += callBacks => SetInputMode(crouch.activeControl?.device);
        basicAttack.performed += callBacks => SetInputMode(basicAttack.activeControl?.device);
    }

    private void OnDisable()
    {
        inputActions.Disable();

        var jump = inputActions.FindAction("Player/Jump");
        var crouch = inputActions.FindAction("Player/Crouch");
        var basicAttack = inputActions.FindAction("Player/BasicAttack");

        jump.performed -= callBacks => JumpPerformed();
        jump.canceled -= callBacks => JumpCanceled();
        crouch.performed -= callBacks => CrouchPerformed();
        crouch.canceled -= callBacks => CrouchCanceled();
        basicAttack.performed -= callBacks => BasicAttack();
        
        jump.performed -= callBacks => SetInputMode(jump.activeControl?.device);
        crouch.performed -= callBacks => SetInputMode(crouch.activeControl?.device);
        basicAttack.performed -= callBacks => SetInputMode(basicAttack.activeControl?.device);
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
        player.Mover.Jump();
    }
    void JumpCanceled()
    {
        player.Mover.CancelJump();
    }
    void CrouchPerformed()
    { 
        player.Mover.Crouch();
    }
    void CrouchCanceled()
    {
        player.Mover.CancelCrouch();
    }
    void BasicAttack()
    { 
        player.Fighter.Attack(AttackTypes.BasicHit);
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