using System;
using System.Collections.Generic;
using UnityEngine;

public class RaycastHelper : MonoBehaviour
{
    public static bool Raycast(out RaycastHit hit, Transform transform, RayData rayData)
    {
        hit = new RaycastHit();
        foreach (var direction in rayData.Directions)
        {
            Vector3 directionVector = GetDirectionVector(transform, direction);

            Ray ray = new Ray(transform.position + rayData.OriginOffset, directionVector);
            Debug.DrawRay(ray.origin, ray.direction * rayData.Distance, rayData.Color);

            if (Physics.Raycast(ray, out hit, rayData.Distance, rayData.Mask, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
        }

        return false;
    }
    public static bool RaycastAll(out RaycastHit[] hits, Transform transform, RayData rayData)
    {
        hits = new RaycastHit[0];
        List<RaycastHit> hitList = new List<RaycastHit>();

        foreach (var direction in rayData.Directions)
        {
            Vector3 directionVector = GetDirectionVector(transform, direction);

            Ray ray = new Ray(transform.position + rayData.OriginOffset, directionVector);
            Debug.DrawRay(ray.origin, ray.direction * rayData.Distance, rayData.Color);

            RaycastHit[] hitResults = Physics.RaycastAll(ray, rayData.Distance, rayData.Mask, QueryTriggerInteraction.Ignore);
            if (hitResults.Length > 0)
            {
                hitList.AddRange(hitResults);
            }
        }

        hits = hitList.ToArray();
        return hits.Length > 0;
    }

    public static void DrawRay(Transform transform, RayData rayData)
    {
        foreach (var direction in rayData.Directions)
        {
            Vector3 directionVector = GetDirectionVector(transform, direction);
            Ray ray = new Ray(transform.position + rayData.OriginOffset, directionVector);
            DrawRay(ray.origin, ray.direction * rayData.Distance, rayData.Color, rayData.DrawThick);
        }
    }
    static void DrawRay(Vector3 origin, Vector3 vector3, Color color, bool drawThick)
    {
        if (drawThick)
        {
            float thickness = 0.05f;
            Debug.DrawLine(origin + Vector3.up * thickness, origin + Vector3.up * thickness + vector3, color);
            Debug.DrawLine(origin - Vector3.up * thickness, origin - Vector3.up * thickness + vector3, color);
            Debug.DrawLine(origin + Vector3.right * thickness, origin + Vector3.right * thickness + vector3, color);
            Debug.DrawLine(origin - Vector3.right * thickness, origin - Vector3.right * thickness + vector3, color);
        }
        Debug.DrawLine(origin, origin + vector3, color);
    }
    public static Vector3 GetDirectionVector(Transform transform, Directions direction)
    {
        var directionVector = Vector3.zero;
        switch (direction)
        {
            case Directions.Forward:
                directionVector = transform.forward;
                break;
            case Directions.Backward:
                directionVector = -transform.forward;
                break;
            case Directions.Left:
                directionVector = -transform.right;
                break;
            case Directions.Right:
                directionVector = transform.right;
                break;
            case Directions.Up:
                directionVector = transform.up;
                break;
            case Directions.Down:
                directionVector = -transform.up;
                break;
            case Directions.ForwardLeft:
                directionVector = (transform.forward - transform.right).normalized;
                break;
            case Directions.ForwardRight:
                directionVector = (transform.forward + transform.right).normalized;
                break;
            case Directions.BackwardLeft:
                directionVector = (-transform.forward - transform.right).normalized;
                break;
            case Directions.BackwardRight:
                directionVector = (-transform.forward + transform.right).normalized;
                break;
            case Directions.UpLeft:
                directionVector = (transform.up - transform.right).normalized;
                break;
            case Directions.UpRight:
                directionVector = (transform.up + transform.right).normalized;
                break;
            case Directions.DownLeft:
                directionVector = (-transform.up - transform.right).normalized;
                break;
            case Directions.DownRight:
                directionVector = (-transform.up + transform.right).normalized;
                break;
        }
        return directionVector;
    }

}
[Serializable]
public struct RayData
{
    public List<Directions> Directions;
    public Vector3 OriginOffset;
    public float Distance;
    public bool DrawThick;
    public Color Color;
    public LayerMask Mask;
}
public enum Directions
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down,
    ForwardLeft,
    ForwardRight,
    BackwardLeft,
    BackwardRight,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}