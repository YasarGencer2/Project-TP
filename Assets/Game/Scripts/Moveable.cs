using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MoveableAnimator)), RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour
{
    [HideInInspector] public MoveableAnimator MoveableAnimator;
    [HideInInspector] public Rigidbody Rb;

    [Header("Movement")]
    [SerializeField] float activeSpeed = 5f;
    [SerializeField] float minSpeed = 2f, midSpeed = 5f, maxSpeed = 10f;
    [SerializeField] float speedGainTimeTillMid = 1, speedGainTimeTillMax = 1;
    [SerializeField] float speedLoseTime = .5f;
    float lastMovingSpeed;


    [Space(5), Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float doubleJumpForce = 5f;
    [SerializeField] bool isGrounded, canCheckGrounded = true;
    public bool jumpInput = false;


    [Space(5), Header("Gravity")]
    [SerializeField] float gravityJumpForce = 9.81f;
    [SerializeField] float gravityFallForce = 9.81f;


    [Space(5), Header("Rotation")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;
    [SerializeField] Vector3 lookTo;


    [Space(5), Header("Status")]
    [SerializeField] bool isWalking;
    [SerializeField] bool isJumping;
    [SerializeField] bool isDoubleJumping;

    [Space(5), Header("Debug")]
    [SerializeField] float lookAngle = 0f;
    [SerializeField] int directionForward = 1;
    [SerializeField] int directionSideways = 0;
    [SerializeField] Directions directionByRotation;
    [SerializeField] Vector2 movementInput, lastMovementInput;
    [SerializeField] Vector2 lookInput, lastLookInput;

    void Awake()
    {
        MoveableAnimator = GetComponent<MoveableAnimator>();
        Rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        HandleMovement();
        HandleAnimations();
    }
    #region MOVEMENT
    void HandleMovement()
    {
        MoveLogic();
        JumpLogic();
        GravityLogic();
        LookLogic();
    }
    void MoveLogic()
    {
        GetWalkInput();
        HandleSpeed();
        Walk();
    }
    void JumpLogic()
    {
        SetIsGrounded();
    }
    void GravityLogic()
    {
        UpdateGravity();
    }

    #region Walk
    void GetWalkInput()
    {
        movementInput = InputManager.Instance.GetMovementInput();
        isWalking = movementInput.magnitude > 0;
        SetLastWalkInput();
    }
    void SetLastWalkInput()
    {
        if (isWalking == false)
            return;
        if (Vector2.Dot(lastMovementInput, movementInput) < 0)
        {
            activeSpeed = 0;
        }
        lastMovementInput = movementInput;
    }
    void HandleSpeed()
    {
        if (isWalking)
        {
            if (activeSpeed < minSpeed)
                activeSpeed = minSpeed;
            if (activeSpeed < midSpeed)
            {
                activeSpeed += Time.deltaTime * (maxSpeed / speedGainTimeTillMid);
            }
            else if (activeSpeed < maxSpeed)
            {
                activeSpeed += Time.deltaTime * (maxSpeed / speedGainTimeTillMax);
            }
            lastMovingSpeed = activeSpeed;
        }
        else
        {
            activeSpeed -= Time.deltaTime * (lastMovingSpeed / speedLoseTime);
        }
        activeSpeed = Mathf.Clamp(activeSpeed, 0, maxSpeed);
    }
    void Walk()
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
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 0.05f);
        isGrounded = hits.Length > 1;
        if (isGrounded == false && hits.Length > 0)
        {
            var hit = hits[0];
            isGrounded = hit.collider.gameObject != this;
        }
        if (isGrounded)
        {
            isJumping = false;
            isDoubleJumping = false;
            canCheckGrounded = false;
        }
    }
    public void Jump()
    {
        jumpInput = true;
        if (isDoubleJumping == true && isGrounded == false)
            return;
        if (isJumping == true)
            isDoubleJumping = true;
        isJumping = true;
        isGrounded = false;
        StartCoroutine(SetCanCheckGrounded(true));

        var force = isDoubleJumping ? doubleJumpForce : jumpForce;
        Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
        Rb.AddForce(Vector3.up * force, ForceMode.Impulse);

        JumpAnimation();
    }
    public void CancelJump()
    {
        jumpInput = false;
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
        var force = (jumpInput && Rb.linearVelocity.y > 0) ? gravityJumpForce : gravityFallForce;
        Rb.AddForce(Vector3.down * force * Time.deltaTime, ForceMode.Impulse);
    }
    #endregion
    #region Look
    void LookLogic()
    {
        GetLookInput();
        Look();
        SetDirectionByLookDirection();
    }
    void GetLookInput()
    {
        if (InputManager.Instance.OnGamepad)
        {
            GetRotationOnGamepad();
        }
        else
        {
            GetRotationOnKeyboardAndMouse();
        }
    }
    void GetRotationOnKeyboardAndMouse()
    {
        var mousePos = InputManager.Instance.GetMousePosition();
        lookTo = mousePos - transform.position;
        lookTo.y = 0;
    }
    void GetRotationOnGamepad()
    {
        lookInput = InputManager.Instance.GetLookInput();
        if (lookInput == Vector2.zero && movementInput != Vector2.zero)
        {
            lookInput = movementInput;
        }
        else if (lookInput == Vector2.zero && lastMovementInput == Vector2.zero)
        {
            lookInput = lastLookInput;
        }
        lastLookInput = lookInput;
        lookTo = new Vector3(lookInput.x, 0, lookInput.y);
    }
    void Look()
    {
        if (lookTo == Vector3.zero)
            return;

        lookAngle = Vector3.SignedAngle(transform.forward, lookTo.normalized, Vector3.up);
        var a = transform.rotation;
        var b = transform.rotation * Quaternion.Euler(0, lookAngle, 0);
        var t = Time.deltaTime * rotationSpeed;

        transform.rotation = Quaternion.Slerp(a, b, t);
    }
    void SetDirectionByLookDirection()
    {
        var moveDirection = new Vector3(lastMovementInput.x, 0, lastMovementInput.y);
        var lookDirection = transform.forward;
        var direction = Vector3.Dot(moveDirection, lookDirection);
        if (direction > 0.5f)
        {
            directionByRotation = Directions.Forward;
        }
        else if (direction < -0.5f)
        {
            directionByRotation = Directions.Backward;
        }
        else if (Vector3.Dot(moveDirection, transform.right) > 0.5f)
        {
            directionByRotation = Directions.Right;
        }
        else if (Vector3.Dot(moveDirection, -transform.right) > 0.5f)
        {
            directionByRotation = Directions.Left;
        }
        else
        {
            directionByRotation = Directions.Forward;
        }
    }
    #endregion
    #endregion

    #region ANIMATIONS
    void HandleAnimations()
    {
        WalkAnimation();
        LandAnimation();
    }
    void WalkAnimation()
    {
        HandleAnimationWalkSpeed();
        HandleAnimationWalkDirection();
    }
    void HandleAnimationWalkSpeed()
    {
        MoveableAnimator.SetSpeed(activeSpeed);
    }
    void HandleAnimationWalkDirection()
    {
        var look = transform.forward;
        var move = new Vector3(lastMovementInput.x, 0, lastMovementInput.y).normalized;

        if (move.magnitude > 0)
        {
            var angle = Vector3.SignedAngle(look, move, Vector3.up);
            directionForward = Mathf.Abs(angle) < 90 ? 1 : -1;
            directionSideways = angle > 0 ? 1 : (angle < 0 ? -1 : 0);
        }

        MoveableAnimator.SetDirectionForward(directionForward);
        MoveableAnimator.SetDirectionSideways(directionSideways);
    }
    void LandAnimation()
    {
        if (isGrounded)
            MoveableAnimator.SetLand();
    }
    void JumpAnimation()
    {
        MoveableAnimator.SetJump();
    }
    #endregion
}


enum Directions
{
    Forward,
    Backward,
    Left,
    Right
}