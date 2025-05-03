using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] InputActionAsset inputActions;

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
}