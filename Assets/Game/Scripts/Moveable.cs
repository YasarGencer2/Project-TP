using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
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
    [SerializeField] bool holdingJump = false;
    [SerializeField] bool jumpInput = false;

    [Space(2), Header("Wall Jump")]
    [SerializeField] float wallJumpForceUp = 5f;
    [SerializeField] float wallJumpForceSide = 5f;
    [SerializeField] float wallJumpTime = 0.25f;
    [SerializeField] bool canCheckHangingOnWall = true;
    [SerializeField] float wallGravityStartTime = 0.25f;
    [SerializeField] float wallGravityStartTimeCounter = 0f;
    [SerializeField] Vector3 wallDirection = Vector3.zero;
    Coroutine wallJumpCoroutine, canCheckHangingOnWallCoroutine;

    [Space(5), Header("Crouch")]
    [SerializeField] float crouchSpeed = 4f;
    [SerializeField] float normalHeight = 1f;
    [SerializeField] float crouchHeight = 0.5f;

    [SerializeField] bool crouchInput = false;


    [Space(5), Header("Gravity")]
    [SerializeField] float gravity = 0;
    [Space(2)]
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
    [SerializeField] bool isWallJumping;
    [SerializeField] bool isCrouching;

    [Space(10), Header("Debug")]
    [Space(0), Header("Ray Datas")]
    [SerializeField] RayData isGroundedRayData;
    [SerializeField] RayData wallJumpRayData;
    [SerializeField] RayData wallJumpGroundRayData;
    [SerializeField] RayData ledgeBottomRayData;
    [SerializeField] RayData ledgeTopRayData;
    [Space(5), Header("Values")]
    [SerializeField] float lookAngle = 0f;
    [SerializeField] int directionForward = 1;
    [SerializeField] int directionSideways = 0;
    [SerializeField] Directions directionByRotation;
    [SerializeField] Vector2 movementInput, lastMovementInput;
    [SerializeField] Vector2 lookInput, lastLookInput;
    [SerializeField] Vector3 lookTo, lastLookTo;
    [SerializeField] float startLinearDamping;

    void Awake()
    {
        moveableAnimator = GetComponent<MoveableAnimator>();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        startLinearDamping = rb.linearDamping;
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
        CrouchLogic();
        WallLogic();
        GravityLogic();
        LookLogic();
        MovementHelpers();
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
    void WallLogic()
    {
        SetIsHangingOnWall();
        CountWallGravity();
    }
    void CrouchLogic()
    {
        GetCrouchInput();
        HandleCrouch();
    }
    void GravityLogic()
    {
        UpdateGravity();
    }
    void LookLogic()
    {
        GetLookInput();
        Look();
    }
    void MovementHelpers()
    {
        LedgeHelper();
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
            var minSpeed = this.minSpeed;
            var midSpeed = this.midSpeed;
            var maxSpeed = this.maxSpeed;

            if (isCrouching)
            {
                minSpeed = crouchSpeed;
                midSpeed = crouchSpeed;
                maxSpeed = crouchSpeed;
            }

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
            else if (activeSpeed > maxSpeed)
            {
                activeSpeed -= Time.deltaTime * (maxSpeed / speedGainTimeTillMax);
            }
            lastMovingSpeed = activeSpeed;
        }
        else
        {
            if (activeSpeed > 0)
                activeSpeed -= Time.deltaTime * (lastMovingSpeed / speedLoseTime);
            else
                activeSpeed = 0;
        }
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
        RaycastHelper.RaycastAll(out RaycastHit[] hits, transform, isGroundedRayData);
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
        holdingJump = true;
        var canJump = isHangingOnWall || (isGrounded = false || isDoubleJumping == false);
        if (canJump == false)
            return;

        if (isCrouching)
            CancelCrouch();

        if (isHangingOnWall)
        {
            WallJumpForce();
        }
        else
        {
            if (isJumping == true)
                isDoubleJumping = true;
            JumpFroce();
            JumpAnimation();
        }

        isJumping = true;
        isGrounded = false;
        isHangingOnWall = false;
        canCheckGrounded = false;
        canCheckHangingOnWall = false;
        StartCoroutine(SetCanCheckGrounded(true));
        if (canCheckHangingOnWallCoroutine != null)
            StopCoroutine(canCheckHangingOnWallCoroutine);
        canCheckHangingOnWallCoroutine = StartCoroutine(SetCanCheckHanginOnWall(true));
    }
    void JumpFroce()
    {
        var force = isDoubleJumping ? doubleJumpForce : jumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }
    public void CancelJump()
    {
        if (jumpInput == true)
            holdingJump = false;
        jumpInput = false;
    }
    IEnumerator SetCanCheckGrounded(bool value)
    {
        yield return new WaitForSeconds(0.05f);
        canCheckGrounded = value;
    }
    #region Wall
    void CountWallGravity()
    {
        if (isHangingOnWall)
        {
            wallGravityStartTimeCounter -= Time.deltaTime;
        }
    }
    void SetIsHangingOnWall()
    {
        if (isGrounded)
        {
            isHangingOnWall = false;
            rb.linearDamping = startLinearDamping;
            return;
        }
        if (canCheckHangingOnWall == false)
            return;

        if (RaycastHelper.RaycastAll(out RaycastHit[] hits, transform, wallJumpGroundRayData) == false)
        {
            RaycastHelper.Raycast(out RaycastHit hit, transform, wallJumpRayData);
            if (hit.collider != null && hit.collider.gameObject != this.gameObject)
            {
                if (isHangingOnWall == false)
                {
                    wallGravityStartTimeCounter = wallGravityStartTime;
                    isWalking = false;
                    isDoubleJumping = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.linearDamping = 0;
                }
                wallDirection = hit.normal;
                isHangingOnWall = true;
                return;
            }
        }
        rb.linearDamping = startLinearDamping;
        isHangingOnWall = false;
    }
    void WallJumpForce()
    {
        var up = Vector3.up * wallJumpForceUp;
        var movementDirection = new Vector3(movementInput.x, 0, movementInput.y).normalized;
        var dir = movementDirection == Vector3.zero ? wallDirection : movementDirection;
        var forward1 = wallDirection * wallJumpForceSide;
        var forward2 = forward1 / 2 + (dir * wallJumpForceSide / 2);
        var forward = forward1.magnitude > forward2.magnitude ? forward1 : forward2;

        rb.linearDamping = startLinearDamping;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(up + forward, ForceMode.Impulse);

        isWallJumping = true;
        if (wallJumpCoroutine != null)
            StopCoroutine(wallJumpCoroutine);
        wallJumpCoroutine = StartCoroutine(SetIsWallJumping(false));
    }
    IEnumerator SetCanCheckHanginOnWall(bool value)
    {
        yield return new WaitForSeconds(0.15f);
        canCheckHangingOnWall = value;
    }
    IEnumerator SetIsWallJumping(bool value)
    {
        yield return new WaitForSeconds(wallJumpTime);
        isWallJumping = value;
    }
    #endregion
    #endregion
    #region Crouch
    void GetCrouchInput()
    {
        crouchInput = InputManager.Instance.GetCrouchInput();
        if (crouchInput == false)
            CancelCrouch();
    }
    void HandleCrouch()
    {
        if (crouchInput == false)
            return;
        if (isCrouching == false && isGrounded)
        {
            isCrouching = true;
            // first crouch
            transform.localScale = new Vector3(1, crouchHeight, 1);
        }

        if (isCrouching)
        {

        }
    }
    void CancelCrouch()
    {
        isCrouching = false;
        transform.localScale = new Vector3(1, normalHeight, 1);
    }
    #endregion
    #region Gravity
    void UpdateGravity()
    {
        if (isGrounded)
            return;
        gravity = holdingJump ? gravityJumpForce : gravityFallForce;
        if (isHangingOnWall)
            gravity = wallGravityStartTimeCounter <= 0 ? gravityHangOnWallForce : 0;
        rb.AddForce(Vector3.down * gravity * Time.fixedDeltaTime, ForceMode.Impulse);
    }
    #endregion
    #region Look
    void GetLookInput()
    {
        if (isHangingOnWall)
        {
            GetRotationOnWall();
            return;
        }
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
    void GetRotationOnWall()
    {
        // this face along
        lastLookTo = lookTo;
        lookTo = Vector3.Cross(Vector3.up, wallDirection).normalized;
        if (Vector3.Dot(lookTo, lastMovementInput) < 0)
        {
            lookTo = -lookTo;
        }

        // this face away
        // lastLookTo = lookTo;
        // lookTo = wallDirection.normalized;
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
    #endregion
    #region Movement Helpers
    void LedgeHelper()
    {
        if (isHangingOnWall)
            return;
        if (isGrounded)
            return;
        if (rb.linearVelocity.y < 0)
            return;
        RaycastHelper.Raycast(out RaycastHit hitTop, transform, ledgeTopRayData);
        RaycastHelper.Raycast(out RaycastHit hitBottom, transform, ledgeBottomRayData);
        if (hitTop.collider == null && hitBottom.collider != null)
        {
            print("Ledge");
            rb.MovePosition(rb.position + transform.up * (ledgeTopRayData.OriginOffset.y - ledgeBottomRayData.OriginOffset.y));
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

    #region DEBUG
    void OnDrawGizmos()
    {
        RaycastHelper.DrawRay(transform, isGroundedRayData);
        RaycastHelper.DrawRay(transform, wallJumpRayData);
        RaycastHelper.DrawRay(transform, wallJumpGroundRayData);
        RaycastHelper.DrawRay(transform, ledgeBottomRayData);
        RaycastHelper.DrawRay(transform, ledgeTopRayData);
    }
    #endregion
}
