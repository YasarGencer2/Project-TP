using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera camera;
    public Camera Camera => camera;

    public virtual void GiveInput(Vector2 input)
    {
    }
}