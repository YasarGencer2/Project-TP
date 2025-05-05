using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MoveableAnimator)), RequireComponent(typeof(Rigidbody))]
public class Moveable : MonoBehaviour
{
    [HideInInspector] MoveableAnimator moveableAnimator;
    [HideInInspector] Rigidbody rb;

    [Header("Movement")]
    [SerializeField] float activeSpeed = 5f;
    [SerializeField] float minSpeed = 2f, midSpeed = 5f, maxSpeed = 10f;
    [SerializeField] float speedGainTimeTillMid = 1, speedGainTimeTillMax = 1;
    [SerializeField] float speedLoseTime = .5f;
    float lastMovingSpeed;


    [Space(5), Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float doubleJumpForce = 5f;
    [SerializeField] bool canCheckGrounded = true;
    public bool jumpInput = false;

    [Space(2), Header("Wall Jump")]
    [SerializeField] float wallJumpForce = 5f;
    [SerializeField] bool canCheckHangingOnWall = true;
    [SerializeField] Vector3 wallDirection = Vector3.zero;


    [Space(5), Header("Gravity")]
    [SerializeField] float gravityJumpForce = 9.81f;
    [SerializeField] float gravityFallForce = 9.81f;
    [SerializeField] float gravityHangOnWallForce = 9.81f;


    [Space(5), Header("Rotation")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;


    [Space(5), Header("Status")]
    [SerializeField] bool isWalking;
    [SerializeField] bool isJumping;
    [SerializeField] bool isDoubleJumping;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isHangingOnWall;

    [Space(5), Header("Debug")]
    [SerializeField] float lookAngle = 0f;
    [SerializeField] int directionForward = 1;
    [SerializeField] int directionSideways = 0;
    [SerializeField] Directions directionByRotation;
    [SerializeField] Vector2 movementInput, lastMovementInput;
    [SerializeField] Vector2 lookInput, lastLookInput;
    [SerializeField] Vector3 lookTo, lastLookTo;

    void Awake()
    {
        moveableAnimator = GetComponent<MoveableAnimator>();
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        HandleMovement();
    }
    void Update()
    {
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
        SetIsHangingOnWall();
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
        if (isHangingOnWall)
        {
            activeSpeed = 0;
            return;
        }
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
        rb.MovePosition(rb.position + vector * Time.deltaTime * activeSpeed);
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
            isHangingOnWall = false;

        }
    }
    public void Jump()
    {
        jumpInput = true;
        var canJump = isHangingOnWall || isGrounded || isDoubleJumping == false;
        if (canJump == false)
            return;
        if (isHangingOnWall)
        {
            WallJumpForce();
        }
        else
        {
            JumpFroce();
            JumpAnimation();
            if (isJumping == true)
                isDoubleJumping = true;
        }

        isJumping = true;
        isGrounded = false;
        isHangingOnWall = false;
        canCheckGrounded = false;
        canCheckHangingOnWall = false;
        StartCoroutine(SetCanCheckGrounded(true));
        StartCoroutine(SetCanCheckHanginOnWall(true));
    }
    void JumpFroce()
    {
        var force = isDoubleJumping ? doubleJumpForce : jumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
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
    #region Wall Jump
    void SetIsHangingOnWall()
    {
        if (isGrounded || isJumping == false)
        {
            isHangingOnWall = false;
            return;
        }
        if (canCheckHangingOnWall == false)
            return;
        RaycastHit hit;
        Vector3[] directions = { transform.forward, transform.right, -transform.right, -transform.forward };

        foreach (var direction in directions)
        {
            Debug.DrawRay(transform.position, direction * 0.75f, Color.red);
            if (Physics.Raycast(transform.position, direction, out hit, 0.75f))
            {
                if (hit.collider != null && hit.collider.gameObject != this.gameObject)
                {
                    isHangingOnWall = true;
                    isWalking = false;
                    wallDirection = hit.normal;
                    return;
                }
            }
        }
        isHangingOnWall = false;
    }
    void WallJumpForce()
    {
        var up = Vector3.up * jumpForce;
        var forward = wallDirection * wallJumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(up + forward, ForceMode.Impulse);
    }
    IEnumerator SetCanCheckHanginOnWall(bool value)
    {
        yield return new WaitForSeconds(0.15f);
        canCheckHangingOnWall = value;
    }
    #endregion
    #endregion

    #region Gravity
    void UpdateGravity()
    {
        if (isGrounded)
            return;
        var force = isJumping ? gravityJumpForce : gravityFallForce;
        if (isHangingOnWall)
            force = gravityHangOnWallForce;
        rb.AddForce(Vector3.down * force * Time.deltaTime, ForceMode.Impulse);
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
        lastLookTo = lookTo;
        lookTo = mousePos - transform.position;
        lookTo.Normalize();
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
        lastLookTo = lookTo;
        lookTo = new Vector3(lookInput.x, 0, lookInput.y);
        lookTo.Normalize();
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
        moveableAnimator.SetSpeed(activeSpeed);
    }
    void HandleAnimationWalkDirection()
    {
        var movingDirection = new Vector3(lastMovementInput.x, 0, lastMovementInput.y);
        var lookDirection = transform.forward;

        directionForward = Vector3.Dot(movingDirection.normalized, lookDirection) > 0.5f ? 1 : -1;

        moveableAnimator.SetDirectionForward(directionForward);
    }
    void LandAnimation()
    {
        if (isGrounded)
            moveableAnimator.SetLand();
    }
    void JumpAnimation()
    {
        moveableAnimator.SetJump();
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