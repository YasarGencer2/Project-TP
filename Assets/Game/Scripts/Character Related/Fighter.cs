using UnityEngine;

public class Fighter : CharacterComponent
{
    [SerializeField] AttackData attackData;
    float notMoveTImer = 0;
    float notJumpTimer = 0;
    float notRotateTimer = 0;
    float notSlideTimer = 0;
    float notAttackTimer = 0;

    public bool CanMove => notMoveTImer <= 0;
    public bool CanJump => notJumpTimer <= 0;
    public bool CanRotate => notRotateTimer <= 0;
    public bool CanSlide => notSlideTimer <= 0;
    public bool CanAttack => notAttackTimer <= 0;

    void Update()
    {
        HandleTimers();
    }
    void HandleTimers()
    {
        if (notMoveTImer > 0)
        {
            notMoveTImer -= Time.deltaTime;
        }
        if (notJumpTimer > 0)
        {
            notJumpTimer -= Time.deltaTime;
        } 
        if (notRotateTimer > 0)
        {
            notRotateTimer -= Time.deltaTime;
        }
        if (notSlideTimer > 0)
        {
            notSlideTimer -= Time.deltaTime;
        }
        if (notAttackTimer > 0)
        {
            notAttackTimer -= Time.deltaTime;
        }
    }
    void EffectTimers(AttackData attackData)
    {
        notMoveTImer = attackData.StopMovingFor;
        notJumpTimer = attackData.StopJumpingFor;
        notRotateTimer = attackData.StopRotatingFor;
        notSlideTimer = attackData.StopSlidingFor;
        notAttackTimer = attackData.StopAttackingFor;
    }

    public void Attack(AttackTypes attackType)
    {
        if (CanAttack == false)
        {
            Debug.LogWarning("Cannot attack right now. Attack cooldown active.");
            return;
        }
        switch (attackType)
        {
            case AttackTypes.BasicHit:
                if (CheckRestrictions(attackData))
                    BasicHit();
                break;
            default:
                Debug.LogWarning("Attack type not implemented: " + attackType);
                break;
        }
    }
    bool CheckRestrictions(AttackData attackData)
    {
        var isGrounded = CharacterController.Mover.isGrounded;
        var isSliding = CharacterController.Mover.isSliding;
        if (!attackData.CanUseOnAir && !isGrounded)
        {
            Debug.LogWarning("Cannot use this attack in the air.");
            return false;
        }
        if (!attackData.CanUseOnGround && isGrounded)
        {
            Debug.LogWarning("Cannot use this attack on the ground.");
            return false;
        }
        if (!attackData.CanUseWhileSliding && isSliding)
        {
            Debug.LogWarning("Cannot use this attack while sliding.");
            return false;
        }
        return true;
    }
    private void BasicHit()
    {
        Debug.Log("Basic hit executed.");
        animator.SetTrigger("Hit1");
        EffectTimers(attackData);
    }
}

public enum AttackTypes
{
    BasicHit,
}