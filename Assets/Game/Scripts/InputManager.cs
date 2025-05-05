using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] InputActionAsset inputActions;
    [SerializeField] LayerMask playerLayerMask;

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
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    public Vector2 GetMovementInput()
    {
        return inputActions.FindAction("Player/Move").ReadValue<Vector2>();
    }
    public bool GetJumpInput()
    {
        return inputActions.FindAction("Player/Jump").ReadValue<float>() == 1;
    }
    public bool GetDodgeInput()
    {
        return inputActions.FindAction("Player/Dodge").ReadValue<float>() == 1;
    }
    public Vector3 GetMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100, ~playerLayerMask))
        {
            return new Vector3(hit.point.x, 0, hit.point.z);
        }
        var worldPos = ray.GetPoint(100);
        return new Vector3(worldPos.x, 0, worldPos.z);
    }
}