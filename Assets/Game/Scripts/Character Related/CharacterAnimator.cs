using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    public void SetBool(string param, bool value)
    {
        if (Animator) Animator.SetBool(param, value);
    }

    public void SetFloat(string param, float value)
    {
        if (Animator) Animator.SetFloat(param, value);
    }
    public void SetTrigger(string param)
    {
        if (Animator) Animator.SetTrigger(param);
    }
}