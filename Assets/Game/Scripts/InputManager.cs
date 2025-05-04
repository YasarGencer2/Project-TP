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

    public Vector3 GetMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        var wordPos = ray.GetPoint(100);
        return new Vector3(wordPos.x, 0, wordPos.z);
    }
}