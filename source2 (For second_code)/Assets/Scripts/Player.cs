using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private InputAction moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Получаем Input Action напрямую
        moveAction = GetComponent<PlayerInput>().actions["Move"];
        Debug.Log("Move Action: " + (moveAction != null));
    }

    void Update()
    {
        // Читаем ввод напрямую каждый кадр
        movement = moveAction.ReadValue<Vector2>();

        if (movement != Vector2.zero)
            Debug.Log("Movement: " + movement);
    }

    void FixedUpdate()
    {
        if (movement != Vector2.zero && rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}