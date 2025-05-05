using UnityEngine;

public class Shadow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float maxScale = 1.5f;
    [SerializeField] float minScale = 0.5f;
    [SerializeField] float startDistance;
    [SerializeField] float maxDistance = 2f;
    [SerializeField] float currentDistance;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startDistance = Mathf.Abs(target.position.y - transform.position.y);
    }
    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned.");
            return;
        }
        currentDistance = Mathf.Abs(target.position.y - transform.position.y);
        float scale = Mathf.Lerp(maxScale, minScale, currentDistance / maxDistance); 
        transform.localScale = new Vector3(scale, scale, 1);

        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }

}
