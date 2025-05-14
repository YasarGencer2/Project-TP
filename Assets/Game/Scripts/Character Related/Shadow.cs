using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] LayerMask targetsLayerMask;
    [SerializeField] float maxScale = 1.5f;
    [SerializeField] float minScale = 0.5f;
    [SerializeField] float maxDistance = 2f;
    [SerializeField] float currentDistance;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }
    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned.");
            return;
        }
        ShadowTransform();
        ShadowScale();
        ShadowRay();
    }
    void ShadowScale()
    {
        currentDistance = Mathf.Abs(target.position.y - transform.position.y);
        float scale = Mathf.Lerp(maxScale, minScale, currentDistance / maxDistance);
        transform.localScale = new Vector3(scale, scale, scale);
    }
    void ShadowTransform()
    {
        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }
    void ShadowRay()
    {
        Ray ray = new Ray(target.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 25, ~targetsLayerMask))
        { 
            sr.enabled = true;
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
        else
        {
            sr.enabled = false;
        } 
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(target.transform.position, transform.position + Vector3.down * 25); 
    }

}
