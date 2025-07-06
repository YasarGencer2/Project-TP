using UnityEngine;

public class Fighter : CharacterComponent
{
    public void Attack(AttackTypes attackType)
    {
        switch (attackType)
        {
            case AttackTypes.BasicHit:
                BasicHit();
                break;
            default:
                Debug.LogWarning("Attack type not implemented: " + attackType);
                break;
        }
    }
    private void BasicHit()
    {
        Debug.Log("Basic hit executed.");
        animator.SetTrigger("Hit1");
    }
}

public enum AttackTypes
{
    BasicHit,
}