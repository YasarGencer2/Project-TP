using System.Collections;
using System.Collections.Generic;
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


    [Space(5), Header("Jump")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isGrounded, canCheckGrounded = true;
    [SerializeField] bool jumpInput;

    [Space(5), Header("Dodge")]
    [SerializeField] float dodgeForce = 5f;
    [SerializeField] float dodgeCooldown = 0.5f;
    [SerializeField] float dodgeTime = 0.5f;
    [SerializeField] bool dodgeInput;
    [SerializeField] bool canDodge = true;

    [Space(5), Header("Gravity")]
    [SerializeField] float gravityJumpForce = 9.81f;
    [SerializeField] float gravityFallForce = 9.81f;

    [Space(5), Header("Rotation")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float rotationSmoothTime = 0.1f;
    [SerializeField] Vector3 rotateTo;

    [Space(5), Header("Status")]
    [SerializeField] bool isWalking;
    [SerializeField] bool isJumping;
    [SerializeField] bool isDodging;

    [Space(5), Header("Debug")]
    [SerializeField] float lookAngle = 0f;
    [SerializeField] int directionForward = 1;
    [SerializeField] int directionSideways = 0;
    [SerializeField] Directions directionByRotation;
    [SerializeField] Vector2 movementInput, lastMovementInput;

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
        DodgeLogic();
        GravityLogic();
        RotationLogic();
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
        GetJumpInput();
        Jump();
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
            activeSpeed += Time.deltaTime * (speed / speedGainTime);
        }
        else
        {
            activeSpeed -= Time.deltaTime * (speed / speedLoseTime);
        }
        activeSpeed = Mathf.Clamp(activeSpeed, 0, speed);
    }
    void Walk()
    {
        var vector = new Vector3(lastMovementInput.x, 0, lastMovementInput.y).normalized;
        transform.position += vector * Time.deltaTime * activeSpeed;
    }
    #endregion
    #region Jump
    void GetJumpInput()
    {
        if (isGrounded == false && jumpInput == false)
            return;
        jumpInput = InputManager.Instance.GetJumpInput();
    }
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
            canCheckGrounded = false;
        }
    }
    void Jump()
    {
        if (isGrounded == false)
            return;
        if (jumpInput == false)
            return;

        isJumping = true;
        isGrounded = false;
        StartCoroutine(SetCanCheckGrounded(true));

        Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
        Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        JumpAnimation();
    }
    IEnumerator SetCanCheckGrounded(bool value)
    {
        yield return new WaitForSeconds(0.05f);
        canCheckGrounded = value;
    }
    #endregion
    #region Dodge
    void DodgeLogic()
    {
        GetDodgeInput();
        Dodge();
    }
    void GetDodgeInput()
    {
        if (canDodge == false)
            return;
        dodgeInput = InputManager.Instance.GetDodgeInput();
    }
    void Dodge()
    {
        if (isGrounded == false)
            return;
        if (dodgeInput == false)
            return;

        isDodging = true;
        canDodge = false;
        dodgeInput = false;
        StartCoroutine(SetDodgeStatus());
        StartCoroutine(SetCanDodge(true));

        var direction = new Vector3(lastMovementInput.x, 0, lastMovementInput.y);
        Rb.AddForce(direction.normalized * dodgeForce, ForceMode.Impulse);
        DodgeAnimation();
    }
    IEnumerator SetDodgeStatus()
    {
        yield return new WaitForSeconds(dodgeTime);
        isDodging = false;
    }
    IEnumerator SetCanDodge(bool value)
    {
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = value;
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
        SetDirectionByRotation();
    }
    void GetRotationInput()
    {
        var mousePos = InputManager.Instance.GetMousePosition();
        rotateTo = (mousePos - transform.position);
        rotateTo.y = 0;
    }
    void Rotate()
    {
        if (rotateTo == Vector3.zero)
            return;

        lookAngle = Vector3.SignedAngle(transform.forward, rotateTo.normalized, Vector3.up);
        var a = transform.rotation;
        var b = transform.rotation * Quaternion.Euler(0, lookAngle, 0);
        var t = Time.deltaTime * rotationSpeed;

        transform.rotation = Quaternion.Slerp(a, b, t);
    }
    void SetDirectionByRotation()
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
    void DodgeAnimation()
    {
        MoveableAnimator.SetDodge(directionByRotation.ToString().ToCharArray()[0]);
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