using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] Mover mover;
    [SerializeField] Fighter fighter;
    [SerializeField] CameraController cameraController;
    CharacterAnimator characterAnimator;
    Rigidbody rb;

    public Mover Mover => mover;
    public CameraController CameraController => cameraController;
    public Fighter Fighter => fighter;

    public CharacterAnimator CharacterAnimator
    {
        get
        {
            if (characterAnimator == null)
            {
                characterAnimator = GetComponent<CharacterAnimator>();
                if (characterAnimator == null)
                {
                    Debug.LogError("CharacterAnimator component not found on " + gameObject.name);
                }
            }
            return characterAnimator;
        }
    }
    public Rigidbody Rb
    {
        get
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    Debug.LogError("Rigidbody component not found on " + gameObject.name);
                }
            }
            return rb;
        }
    }

}