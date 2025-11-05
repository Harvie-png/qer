using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target; 
    public float smoothSpeed = 0.125f;
    public Vector2 offset = Vector2.zero; 

    void LateUpdate()
    {
        if (target != null)
        {
            Vector2 desiredPosition = (Vector2)target.position + offset;
            Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, smoothSpeed);

            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }
}