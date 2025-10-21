using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // LateUpdate runs after all other updates, so the camera has already moved
    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            // Make this object's forward direction point towards the camera
            transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                cameraTransform.rotation * Vector3.up);
        }
    }
}