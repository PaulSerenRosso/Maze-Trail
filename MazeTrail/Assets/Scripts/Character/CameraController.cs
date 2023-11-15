using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float offset;

    void Update()
    {
        bool targetLooksBack = target.GetComponent<CharacterController>().IsLookingBackwards();
        float lerpT = 1 - Mathf.Exp(-5f * Time.deltaTime);
        Vector3 offsetVector = (targetLooksBack ? 1 : -1) * offset * target.forward;
        offsetVector.y = -1.5f;

        var targetRotation = Quaternion.Euler(
            new Vector3(
                target.rotation.eulerAngles.x, 
                target.rotation.eulerAngles.y + (targetLooksBack ? 180.0f : 0.0f), 
                target.rotation.eulerAngles.z)
            );
        
        transform.position = Vector3.Lerp(transform.position, (target.position - offsetVector), lerpT);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpT);
    }
}
