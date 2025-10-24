using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target; // Игрок
    public float smoothSpeed = 0.125f;
    public Vector2 offset = Vector2.zero; // Смещение камеры

    void LateUpdate()
    {
        if (target != null)
        {
            Vector2 desiredPosition = (Vector2)target.position + offset;
            Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Сохраняем Z позицию камеры (-10 для 2D)
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }
}