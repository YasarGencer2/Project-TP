using UnityEngine;

public class MoveableAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;


    public void SetSpeed(float speed)
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetFloat("Speed", speed);
    }
    public void SetDirectionForward(int directionForward)
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetFloat("DirectionForward", directionForward);
    }
    public void SetDirectionSideways(int directionSideways)
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetFloat("DirectionSideways", directionSideways);
    }
    public void SetJump()
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetTrigger("Jump");
    }
    public void SetLand()
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetTrigger("Land");
    }
    public void SetDodge(char direction)
    {
        if (_animator == null)
        {
            Debug.LogWarning("Animator is not assigned.");
            return;
        }

        _animator.SetTrigger("Dodge" + direction);
    }
}