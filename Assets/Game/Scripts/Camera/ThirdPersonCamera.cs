using UnityEngine;

public class ThirdPersonCamera : CameraController
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float rotateSpeed = 5f;
    public float smoothTime = 0.1f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    Vector3 velocity = Vector3.zero;
    float yaw;
    float pitch;


    void LateUpdate()
    {
        GiveInput(InputManager.Instance.GetLookInput());
        SetPosition();
        SetLook();
    }
    public override void GiveInput(Vector2 input)
    {
        base.GiveInput(input);
        SetYawAndPitch(input);
    }

    void SetYawAndPitch(Vector2 input)
    {
        yaw += input.x * rotateSpeed;
        pitch -= input.y * rotateSpeed;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }
    void SetPosition()
    {
        var rotation = Quaternion.Euler(pitch, yaw, 0);
        var desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
    void SetLook()
    {
        transform.LookAt(target);
    }
}