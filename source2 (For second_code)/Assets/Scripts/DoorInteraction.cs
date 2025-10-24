using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public int targetLevelId; // На какой уровень ведет дверь
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;
    private GameManager gameManager;

    void Start()
    {
        // ДИАГНОСТИКА 1: Проверка компонентов двери
        Debug.Log($"=== ДИАГНОСТИКА ДВЕРИ {name} ===");

        Collider2D collider = GetComponent<Collider2D>();
        Debug.Log($"Collider: {collider != null}, IsTrigger: {collider?.isTrigger}");

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Debug.Log($"SpriteRenderer: {renderer != null}");

        // Проверяем тег игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"Player найден: {player != null}, тег: {player?.tag}");

        // Ждем немного чтобы GameManager успел загрузить данные
        Invoke("InitializeDoor", 0.5f);
    }

    void InitializeDoor()
    {
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"GameManager найден: {gameManager != null}");
        UpdateDoorVisual();
    }

    void Update()
    {
        // Проверяем нажатие E только когда игрок рядом
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"Нажата E, playerInRange: {playerInRange}");
            TryInteractWithDoor();
        }
    }

    void UpdateDoorVisual()
    {
        // Меняем цвет двери в зависимости от доступности
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (IsDoorUnlocked())
            {
                renderer.color = Color.green; // Открытая дверь
            }
            else
            {
                renderer.color = Color.red; // Закрытая дверь
            }
        }
    }

    bool IsDoorUnlocked()
    {
        if (gameManager != null)
        {
            // Дверь открыта, если текущий уровень пройден
            bool unlocked = gameManager.IsLevelCompleted(targetLevelId);
            Debug.Log($"Проверка двери: уровень {targetLevelId} пройден = {unlocked}");
            return unlocked;
        }
        Debug.Log("GameManager не найден!");
        return false;
    }

    void TryInteractWithDoor()
    {
        if (IsDoorUnlocked())
        {
            // Переходим на СЛЕДУЮЩИЙ уровень
            int nextLevelId = targetLevelId + 1;
            Debug.Log($"Переход через дверь на уровень {nextLevelId}");

            // Обновляем GameManager
            gameManager.LoadLevel(nextLevelId);
        }
        else
        {
            Debug.Log("Дверь закрыта! Сначала выполните задание текущего уровня.");
            gameManager.ShowFeedback("Дверь закрыта! Выполните задание текущего уровня.", Color.yellow);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D: {other.name} (тег: {other.tag})");
        if (other.CompareTag("Interactable"))
        {
            playerInRange = true;
            Debug.Log("Игрок у двери");

            // Можно показать подсказку
            if (IsDoorUnlocked())
            {
                gameManager.ShowFeedback("Нажмите E чтобы перейти на следующий уровень", Color.white);
            }
            else
            {
                gameManager.ShowFeedback("Дверь закрыта. Выполните задание OLD", Color.yellow);
            }
        }
        else
        {
            Debug.Log($"Объект не игрок: {other.tag}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"OnTriggerExit2D: {other.name} (тег: {other.tag})");
        if (other.CompareTag("Interactable"))
        {
            playerInRange = false;
            Debug.Log("Игрок отошел от двери");
        }
    }
}