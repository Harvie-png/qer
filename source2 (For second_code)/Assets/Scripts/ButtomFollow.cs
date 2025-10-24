using UnityEngine;

public class UIFollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera gameCamera;
    public Canvas canvas;

    [Header("Offset")]
    public Vector2 screenOffset = new Vector2(50, -50); // отступ от угла

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (player != null && gameCamera != null)
        {
            // Переводим мировую позицию игрока в экранные координаты
            Vector3 screenPoint = gameCamera.WorldToScreenPoint(player.position);

            // Смещаем к левому верхнему углу относительно игрока
            rectTransform.position = new Vector2(
                screenPoint.x + screenOffset.x,
                screenPoint.y + screenOffset.y
            );
        }
    }
}