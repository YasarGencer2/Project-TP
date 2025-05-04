using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    [SerializeField] bool isMoving;
    Vector2 movementInput, lastMovementInput;


    [Space(5), Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isGrounded, canCheckGrounded = true;
    [SerializeField] bool jumpInput;

    [Space(5), Header("Gravity")]
    [SerializeField] float gravityJumpForce = 9.81f;
    [SerializeField] float gravityFallForce = 9.81f;

    [Space(5), Header("Rotation")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;
    [SerializeField] Vector3 rotationInput;

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
        GravityLogic();
        RotationLogic();
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
    void GravityLogic()
    {
        UpdateGravity();
    }

    #region Move
    void GetMoveInput()
    {
        movementInput = InputManager.Instance.GetMovementInput();
        isMoving = movementInput.magnitude > 0;
        SetLastMovementInput();
    }
    void SetLastMovementInput()
    {
        if (isMoving == false)
            return;
        if (Vector2.Dot(lastMovementInput, movementInput) < 0)
        {
            activeSpeed = 0;
        }
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
        if (canCheckGrounded == false)
            return;
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
        if (isGrounded == false && jumpInput == false)
            return;
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
        isGrounded = false;
        canCheckGrounded = false;
        StartCoroutine(SetCanCheckGrounded(true));
    }
    IEnumerator SetCanCheckGrounded(bool value)
    {
        yield return new WaitForSeconds(0.05f);
        canCheckGrounded = value;
    }
    #endregion
    #region Gravity
    void UpdateGravity()
    {
        if (isGrounded)
            return;
        var force = jumpInput ? gravityJumpForce : gravityFallForce;
        Rb.AddForce(Vector3.down * force * Time.deltaTime, ForceMode.Impulse);
    }
    #endregion
    #region Rotation
    void RotationLogic()
    {
        GetRotationInput(); 
        Rotate();
    }
    void GetRotationInput()
    {
        rotationInput = InputManager.Instance.GetMousePosition();
        if (rotationInput.magnitude == 0)
            return; 
    }
    void Rotate()
{
    var dir = new Vector3(rotationInput.x, 0, rotationInput.z);
    if (dir == Vector3.zero) return;
    var angle = Vector3.SignedAngle(transform.forward, dir.normalized, Vector3.up);
    var rotation = Quaternion.Euler(0, angle, 0);
    transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * rotation, Time.deltaTime * rotationSpeed);
}

    #endregion
}
