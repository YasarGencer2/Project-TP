using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterAnimator)), RequireComponent(typeof(Rigidbody))]
public class Mover : CharacterComponent
{
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
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] float jumpBufferCounter;

    [Space(2), Header("Wall Jump")]
    [SerializeField] float wallJumpForceUp = 5f;
    [SerializeField] float wallJumpForceSide = 5f;
    [SerializeField] float wallJumpTime = 0.25f;
    [SerializeField] bool canCheckHangingOnWall = true;
    [SerializeField] float wallGravityStartTime = 0.25f;
    [SerializeField] float wallGravityStartTimeCounter = 0f;
    [SerializeField] Vector3 wallDirection = Vector3.zero;
    [SerializeField] GameObject wall;
    Coroutine wallJumpCoroutine, canCheckHangingOnWallCoroutine;

    [Space(5), Header("Crouch")]
    [SerializeField] float crouchSpeed = 4f;
    [SerializeField] float normalHeight = 1f;
    [SerializeField] float crouchHeight = 0.5f;

    [SerializeField] bool crouchInput = false;

    [Space(2), Header("Slide")]
    [SerializeField] float slideSpeed = 30f;
    [SerializeField] float slideSpeedLoseTime = 0.5f;
    [SerializeField] bool canSlide = true;
    Coroutine canSlideCoroutine;

    [Space(2), Header("Air Slide")]
    [SerializeField] float airSlideSpeed = 5f;
    [SerializeField] float airSlideSpeedLoseTime = 0.5f;
    [SerializeField] bool canAirSlide = true;
    [SerializeField] float boostUpForce = 5f;
    Coroutine canAirSlideCoroutine;


    [Space(5), Header("Gravity")]
    [SerializeField] float gravity = 0;
    [Space(2)]
    [SerializeField] float gravityJumpForce = 9.81f;
    [SerializeField] float gravityFallForce = 9.81f;
    [SerializeField] float gravityHangOnWallForce = 9.81f;


    [Space(5), Header("Rotation")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;


    // [Space(5), Header("Status")]
    public bool isWalking;
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isGrounded;
    public bool isHangingOnWall;
    public bool isWallJumping;
    public bool isCrouching;
    public bool isSliding;
    public bool isAirSliding;

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
        GetWallHangingMovementInput();
        HandleSpeed();
        Walk();
    }
    void JumpLogic()
    {
        SetIsGrounded();
        CountJumpBuffer();
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
    bool SetCantMoveByAttack()
    {
        if (CharacterController.Fighter.CanMove == false)
        {
            movementInput = Vector2.zero;
            isWalking = false;
            lastMovementInput = Vector2.zero;
            animator.SetFloat("X", 0);
            return true;
        }
        return false;
    }
    void GetWalkInput()
    {
        SetCantMoveByAttack();
        if (isHangingOnWall)
            return;
        var input = InputManager.Instance.GetMovementInput();
        Vector3 dir = cam.transform.right * input.x + cam.transform.forward * input.y;
        dir.y = 0;
        movementInput = new Vector2(dir.x, dir.z);

        isWalking = movementInput.magnitude > 0;
        SetLastWalkInput();

        animator.SetFloat("X", Mathf.Clamp(Mathf.Round(input.x), -1, 1));
    }




    void GetWallHangingMovementInput()
    {
        SetCantMoveByAttack();
        if (isHangingOnWall == false)
            return;
        var input = InputManager.Instance.GetMovementInput();
        Vector3 dir = cam.transform.right * input.x + cam.transform.forward * input.y;
        dir.y = 0;
        input = new Vector2(dir.x, dir.z);
        if (input.magnitude <= 0)
        {
            movementInput = input;
        }
        else
        {
            var direction = new Vector3(input.x, 0, input.y).normalized;
            var wallDir = new Vector3(-wallDirection.x, 0, -wallDirection.z).normalized;
            if (Vector3.Dot(direction, wallDir) > 0.9f)
            {
                movementInput = Vector2.zero;
            }
            else
            {
                movementInput = input;
            }
        }


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
                if (isSliding)
                {
                    activeSpeed -= Time.deltaTime * (slideSpeed / slideSpeedLoseTime);
                }
                else if (isAirSliding)
                {
                    activeSpeed -= Time.deltaTime * (airSlideSpeed / airSlideSpeedLoseTime);
                }
                else
                {
                    activeSpeed -= Time.deltaTime * (maxSpeed / speedLoseTime);
                }
            }
            lastMovingSpeed = activeSpeed;
        }
        else
        {

            var speedLoseTime = this.speedLoseTime;
            if (isSliding)
            {
                speedLoseTime = slideSpeedLoseTime;
            }
            if (activeSpeed > 0)
                activeSpeed -= Time.deltaTime * (lastMovingSpeed / speedLoseTime);
            else
                activeSpeed = 0;
        }
    }
    void Walk()
    {
        var dir = new Vector3(lastMovementInput.x, 0, lastMovementInput.y).normalized;
        var move = dir * Time.deltaTime * activeSpeed;
        if (rb.SweepTest(dir, out RaycastHit hit, move.magnitude))
        {
            move = dir * hit.distance * 0.9f;
        }
        rb.MovePosition(rb.position + move);
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
            wall = null;
        }
        if (isGrounded)
        {
            isJumping = false;
            isDoubleJumping = false;
            isHangingOnWall = false;
            if (jumpBufferCounter > 0)
                Jump(true);
        }
    }
    void CountJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    void HandleJumpBuffer(bool canJump, bool fromBuffer)
    {
        if (fromBuffer == false)
            if (canJump == false)
                jumpBufferCounter = jumpBufferTime;
        jumpBufferCounter = 0;
    }
    public void Jump(bool fromBuffer = false)
    {
        var canJump = isHangingOnWall || isGrounded;
        if (canJump == false)
            return;
        if (CharacterController.Fighter.CanJump == false)
            return;
        HandleJumpBuffer(canJump, fromBuffer);
        jumpInput = true;
        holdingJump = true;
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
            VFXSystem.Instance.PLayVFX(VFXType.Jump, transform.position);
        }

        animator.SetTrigger("Jump");

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
    void JumpFroce(float overridenForce = 0)
    {

        var force = isDoubleJumping ? doubleJumpForce : jumpForce;
        force = overridenForce == 0 ? force : overridenForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        gravity = 0;
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
                    if (wall != hit.collider.gameObject)
                    {
                        wallGravityStartTimeCounter = wallGravityStartTime;
                        rb.linearVelocity = Vector3.zero;
                    }
                    isWalking = false;
                    isDoubleJumping = false;
                    rb.linearDamping = 0;
                }
                wall = hit.collider.gameObject;
                wallDirection = hit.normal;
                isHangingOnWall = true;
                return;
            }
        }
        wallDirection = Vector3.zero;
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
        // crouchInput = InputManager.Instance.GetCrouchInput();
        // if (crouchInput == false)
        //     CancelCrouch();
    }
    public void Crouch()
    {
        crouchInput = true;
    }
    void HandleCrouch()
    {
        if (CharacterController.Fighter.CanSlide == false)
        {
            crouchInput = false;
            return;
        }
        if (crouchInput == false)
            return;
        if (isHangingOnWall)
            return;
        if (isCrouching == false)
            FirstCrouch();
        Crouching();
        Sliding();
        AirSliding();
    }
    void FirstCrouch()
    {
        isCrouching = true;
        // transform.localScale = new Vector3(1, crouchHeight, 1);

        if (activeSpeed > minSpeed)
        {
            if (isGrounded)
                Slide();
            else
                AirSlide();
        }
    }
    void Crouching()
    {
        if (isCrouching == false)
            return;
    }
    public void CancelCrouch()
    {
        crouchInput = false;
        isCrouching = false;
        isSliding = false;
        isAirSliding = false;
        // transform.localScale = new Vector3(1, normalHeight, 1);
    }
    void Slide()
    {
        if (isSliding)
            return;
        if (canSlide == false)
            return;
        isSliding = true;
        activeSpeed = slideSpeed;
        if (canSlideCoroutine != null)
            StopCoroutine(canSlideCoroutine);
        canSlideCoroutine = StartCoroutine(SetCanSlide(true));
        canSlide = false;
        animator.SetTrigger("Slide");
    }
    void Sliding()
    {
        if (isSliding && isCrouching)
        {
            if (activeSpeed < minSpeed)
            {
                isSliding = false;
            }
        }
    }
    void AirSlide()
    {
        if (isAirSliding)
            return;
        if (canAirSlide == false)
            return;
        isAirSliding = true;
        activeSpeed = airSlideSpeed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        JumpFroce(boostUpForce);
        if (canAirSlideCoroutine != null)
            StopCoroutine(canAirSlideCoroutine);
        canAirSlideCoroutine = StartCoroutine(SetCanAirSlide(true));
        canAirSlide = false;
        animator.SetTrigger("AirSlide");
    }
    void AirSliding()
    {
        if (isAirSliding)
        {
            if (activeSpeed < minSpeed)
            {
                isAirSliding = false;
            }
        }
    }
    IEnumerator SetCanSlide(bool value)
    {
        yield return new WaitForSeconds(.5f);
        canSlide = value;
    }
    IEnumerator SetCanAirSlide(bool value)
    {
        yield return new WaitForSeconds(.5f);
        canAirSlide = value;
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
    bool SetCanLookByAttack()
    {
        if (CharacterController.Fighter.CanRotate == false)
        {
            lookInput = Vector2.zero;
            lookTo = Vector3.zero;
            lastLookInput = Vector2.zero;
            lastLookTo = Vector3.zero;
            return true;
        }
        return false;
    }
    void GetLookInput()
    {
        if (SetCanLookByAttack())
            return;
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
        lookInput = movementInput;
        // if (movementInput != Vector2.zero)
        // {
        //     lookInput = movementInput;
        // }
        // else
        // {
        //     lookInput = lastLookInput;
        // }
        lastLookInput = lookInput;
        lastLookTo = lookTo;
        lookTo = new Vector3(lookInput.x, 0, lookInput.y);
        lookTo.Normalize();
    }
    void GetRotationOnGamepad()
    {
        lookInput = movementInput;
        // lookInput = InputManager.Instance.GetLookInput();
        // if (lookInput == Vector2.zero && movementInput != Vector2.zero)
        // {
        // lookInput = movementInput;
        // }
        // else if (lookInput == Vector2.zero && lastMovementInput == Vector2.zero)
        // {
        //     lookInput = lastLookInput;
        // }
        // else
        // {
        //     Vector3 dir = cam.transform.right * lookInput.x + cam.transform.forward * lookInput.y;
        //     dir.y = 0;
        //     lookInput = new Vector2(dir.x, dir.z);
        // }
        lastLookInput = lookInput;
        lastLookTo = lookTo;
        lookTo = new Vector3(lookInput.x, 0, lookInput.y);
        lookTo.Normalize();
    }
    void GetRotationOnWall()
    {
        if (activeSpeed < minSpeed)
        {
            lastLookTo = lookTo;
            lookTo = -wallDirection.normalized;
        }
        else
        {
            lastLookTo = lookTo;
            lookTo = Vector3.Cross(Vector3.up, wallDirection).normalized;
            if (Vector3.Dot(lookTo, lastMovementInput) < 0)
            {
                lookTo = -lookTo;
            }
        }
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
    #region Animations
    void HandleAnimations()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isHanging", isHangingOnWall);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetFloat("Speed", activeSpeed / maxSpeed);
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

        // Debug.DrawRay(transform.position, movementInput * 5, Color.red);
    }
    #endregion
}
