using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MoveableAnimator)), RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour
{
    [HideInInspector] public MoveableAnimator MoveableAnimator;
    [HideInInspector] public Rigidbody Rb;

    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float activeSpeed = 5f;
    [SerializeField] float speedGainTime = 1;
    [SerializeField] float speedLoseTime = .5f;

    [Space(5), Header("Jump")]
    [SerializeField] float jumpForce = 5f;

    bool isMoving;
    bool isGrounded;

    Vector2 movementInput, lastMovementInput;
    bool jumpInput;
    void Awake()
    {
        MoveableAnimator = GetComponent<MoveableAnimator>();
        Rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        HandleMovement();
    }
    void HandleMovement()
    {
        MoveLogic();
        JumpLogic();
    }
    void MoveLogic()
    {
        GetMoveInput();
        HandleSpeed();
        Move();
    }
    void JumpLogic()
    {
        SetIsGrounded();
        GetJumpInput();
        Jump();
    }

    #region Move
    void GetMoveInput()
    {
        movementInput = InputManager.Instance.GetMovementInput();
        isMoving = movementInput.magnitude > 0;
        if (isMoving)
            lastMovementInput = movementInput;
    }
    void HandleSpeed()
    {
        if (isMoving)
        {
            activeSpeed += Time.deltaTime * (speed / speedGainTime);
        }
        else
        {
            activeSpeed -= Time.deltaTime * (speed / speedLoseTime);
        }
        activeSpeed = Mathf.Clamp(activeSpeed, 0, speed);
    }
    void Move()
    {
        var vector = new Vector3(lastMovementInput.x, 0, lastMovementInput.y).normalized;
        transform.position += vector * Time.deltaTime * activeSpeed;
    }
    #endregion
    #region Jump
    void SetIsGrounded()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 0.1f);
        isGrounded = hits.Length > 1;
        if (isGrounded == false && hits.Length > 0)
        {
            var hit = hits[0];
            isGrounded = hit.collider.gameObject != this;
        }
    }
    void GetJumpInput()
    {
        jumpInput = InputManager.Instance.GetJumpInput();
    }
    void Jump()
    {
        if (isGrounded == false)
            return;
        if (jumpInput == false)
            return;
        Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
        Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    #endregion
}
