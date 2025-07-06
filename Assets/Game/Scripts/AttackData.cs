[System.Serializable]
public class AttackData
{
    public string Name;
    public bool CanUseOnAir = false;
    public bool CanUseOnGround = true;
    public bool CanUseWhileSliding = false;

    public float StopMovingFor;
    public float StopJumpingFor;
    public float StopRotatingFor;
    public float StopSlidingFor;
    public float StopAttackingFor;
}