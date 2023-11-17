using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float offset;

    public void Initialize(Transform targetToFollow)
    {
        target = targetToFollow;
    }
    
    void Update()
    {
        if (!target) return;
        
        float lerpT = 1 - Mathf.Exp(-5f * Time.deltaTime);
        Vector3 offsetVector = -1 * offset * target.forward;
        offsetVector.y = -1.5f;

        var targetRotation = target.rotation;
        
        transform.position = Vector3.Lerp(transform.position, (target.position - offsetVector), lerpT);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpT + lerpT/10);
    }

    public void UnlinkTarget()
    {
        target = null;
    }
}
