using UnityEngine;

[RequireComponent(typeof(MoveableAnimator))]
public class Moveable : MonoBehaviour
{
    [HideInInspector] public MoveableAnimator MoveableAnimator;
    int activeDirection = 1;
    bool run = false;
    void Awake()
    {
        MoveableAnimator = GetComponent<MoveableAnimator>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            run = true; 
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            run = false; 
        }
        if (Input.GetKey(KeyCode.D))
        {
            MoveableAnimator.SetDirection(1);
            var walkSpeed = run ? 2 : 1;
            MoveableAnimator.SetSpeed(walkSpeed);
            activeDirection = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            MoveableAnimator.SetDirection(-1);
            var walkSpeed = run ? 2 : 1;
            MoveableAnimator.SetSpeed(walkSpeed);
            activeDirection = -1;
        }
        else if ((Input.GetKeyUp(KeyCode.D) && activeDirection == 1) || (Input.GetKeyUp(KeyCode.A) && activeDirection == -1))
        {
            MoveableAnimator.SetSpeed(0);
            activeDirection = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveableAnimator.SetJump();
        }
    }
}
