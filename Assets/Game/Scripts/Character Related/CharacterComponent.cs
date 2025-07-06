using UnityEngine;

public class CharacterComponent : MonoBehaviour
{
    protected CharacterController CharacterController;
    protected CharacterAnimator animator => CharacterController.CharacterAnimator;
    protected Rigidbody rb => CharacterController.Rb;
    protected Camera cam => CharacterController.CameraController.Camera;

    protected virtual void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        if (CharacterController == null)
        {
            Debug.LogError("CharacterController component not found on " + gameObject.name);
        }
    }
}