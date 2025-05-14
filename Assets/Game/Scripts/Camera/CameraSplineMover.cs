using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class CameraSplineMover : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] SplineContainer spline;
    [SerializeField] Transform target;
    [SerializeField] float speed = 1;
    [SerializeField] float lookSpeed = 1;
    [SerializeField] float minDistance = 0.01f;
    [SerializeField] float currentDistance = 1000;
    [SerializeField] float currentPosOnSpline = 0;
    [SerializeField] Vector3 startAngle;

    [SerializeField] Vector3 activePosition;
    [SerializeField] Vector3 activeDirection;

    void Start()
    {
        activePosition = spline.EvaluatePosition(currentPosOnSpline);
        startAngle = cam.transform.eulerAngles;
    }

    void Update()
    {
        if (spline == null) return;
        if (cam == null) return;
        if (target == null) return;

        if (CheckSides(true))
            CheckSides(false);

        PlaceCameraOnSpline();
        LookAlongSpline();
    }

    void LateUpdate()
    { 
        PlaceCameraOnSpline();
        LookAlongSpline();
    }

    bool CheckSides(bool right)
    {
        var nextPosOnSpline = right ? currentPosOnSpline + minDistance : currentPosOnSpline - minDistance;
        var positionOnSpline = spline.EvaluatePosition(nextPosOnSpline);
        var distance = Vector3.Distance(positionOnSpline, target.position);
        currentDistance = Vector3.Distance(activePosition, target.position);
        if (distance < currentDistance)
        {
            currentDistance = distance;
            currentPosOnSpline = nextPosOnSpline;
            activePosition = spline.EvaluatePosition(currentPosOnSpline);
            activeDirection = spline.EvaluateTangent(currentPosOnSpline);
            return false;
        }
        return true;
    }

    void PlaceCameraOnSpline()
    {
        // Using Slerp for position interpolation
        cam.transform.position = Vector3.Slerp(cam.transform.position, activePosition, Time.deltaTime * speed);
    }

    void LookAlongSpline()
    {
        Quaternion lookRotation = Quaternion.LookRotation(activeDirection);
        lookRotation *= Quaternion.Euler(0, -90, 0);
        lookRotation *= Quaternion.Euler(startAngle.x, startAngle.y, startAngle.z);

        // Using Slerp for smooth rotation
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
    }
}
