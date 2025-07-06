using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] Mover mover;
    [SerializeField] CameraController cameraController;

    public Mover Mover => mover;
    public CameraController CameraController => cameraController;

    
}