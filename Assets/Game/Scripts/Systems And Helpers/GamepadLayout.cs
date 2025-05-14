using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadLayout : MonoBehaviour
{
    [SerializeField] InputActionAsset inputActions;

    [SerializeField] RectTransform leftStick, rightStick;
    [SerializeField] RectTransform aButton, bButton;

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.FindAction("Player/Move").performed += OnMove;
        inputActions.FindAction("Player/Move").canceled += OnMove;
        inputActions.FindAction("Player/Look").performed += OnLook;
        inputActions.FindAction("Player/Look").canceled += OnLook;
        inputActions.FindAction("Player/Jump").performed += OnJump;
        inputActions.FindAction("Player/Jump").canceled += OnJump;
        inputActions.FindAction("Player/Crouch").performed += OnCrouch;
        inputActions.FindAction("Player/Crouch").canceled += OnCrouch;
    }
    void OnDisable()
    {
        inputActions.Disable();
        inputActions.FindAction("Player/Move").performed -= OnMove;
        inputActions.FindAction("Player/Move").canceled -= OnMove;
        inputActions.FindAction("Player/Look").performed -= OnLook;
        inputActions.FindAction("Player/Look").canceled -= OnLook;
        inputActions.FindAction("Player/Jump").performed -= OnJump;
        inputActions.FindAction("Player/Jump").canceled -= OnJump;
        inputActions.FindAction("Player/Crouch").performed -= OnCrouch;
        inputActions.FindAction("Player/Crouch").canceled -= OnCrouch;
    }
    void OnMove(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.OnGamepad == false)
            return;
        Vector2 moveInput = context.ReadValue<Vector2>();
        leftStick.anchoredPosition = moveInput * 20;
    }
    void OnLook(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.OnGamepad == false)
            return;
        Vector2 lookInput = context.ReadValue<Vector2>();
        rightStick.anchoredPosition = lookInput * 20;
    }
    void OnJump(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.OnGamepad == false)
            return;
        if (context.performed)
        {
            aButton.gameObject.SetActive(true);
        }
        else if (context.canceled)
        {
            aButton.gameObject.SetActive(false);
        }
    }
    void OnCrouch(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.OnGamepad == false)
            return;
        if (context.performed)
        {
            bButton.gameObject.SetActive(true);
        }
        else if (context.canceled)
        {
            bButton.gameObject.SetActive(false);
        }
    }


}
