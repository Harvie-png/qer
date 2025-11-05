using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using SQLite;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public int targetLevelId;
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;
    private GameManager gameManager;
    private bool developerMode = false;

    void Start()
    {
        Debug.Log($"=== ДИАГНОСТИКА ДВЕРИ {name} ===");

        Collider2D collider = GetComponent<Collider2D>();
        Debug.Log($"Collider: {collider != null}, IsTrigger: {collider?.isTrigger}");

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Debug.Log($"SpriteRenderer: {renderer != null}");

        // Проверяем режим разработчика
        CheckDeveloperMode();

        Invoke("InitializeDoor", 0.5f);
    }

    void CheckDeveloperMode()
    {
        try
        {
            using (var db = new SQLiteConnection("escapeRoomBase.sqlite", SQLiteOpenFlags.ReadWrite))
            {
                var settings = db.Table<SettingsManager.Setting>().FirstOrDefault();
                developerMode = settings?.debug_mode == 1;
                Debug.Log($"Режим разработчика: {developerMode}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка проверки режима разработчика: {e.Message}");
            developerMode = false;
        }
    }

    void InitializeDoor()
    {
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"GameManager найден: {gameManager != null}");
        UpdateDoorVisual();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"Нажата E, playerInRange: {playerInRange}");
            TryInteractWithDoor();
        }
    }

    void UpdateDoorVisual()
    {
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
        // В режиме разработчика все двери открыты
        if (developerMode)
        {
            Debug.Log($"Режим разработчика: дверь {targetLevelId} разблокирована");
            return true;
        }

        if (gameManager != null)
        {
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
            int nextLevelId = targetLevelId + 1;
            Debug.Log($"Переход через дверь на уровень {nextLevelId}");

            // Показываем сообщение в режиме разработчика
            if (developerMode && gameManager != null)
            {
                gameManager.ShowFeedback($"РЕЖИМ РАЗРАБОТЧИКА: переход на уровень {nextLevelId}", Color.cyan);
            }

            gameManager.LoadLevel(nextLevelId);
        }
        else
        {
            Debug.Log("Дверь закрыта! Сначала выполните задание текущего уровня.");
            if (gameManager != null)
            {
                gameManager.ShowFeedback("Дверь закрыта! Выполните задание текущего уровня.", Color.yellow);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D: {other.name} (тег: {other.tag})");
        if (other.CompareTag("Interactable"))
        {
            playerInRange = true;
            Debug.Log("Игрок у двери");

            if (gameManager != null)
            {
                if (IsDoorUnlocked())
                {
                    string message = "Нажмите E чтобы перейти на следующий уровень";
                    if (developerMode)
                    {
                        message += " (РЕЖИМ РАЗРАБОТЧИКА)";
                    }
                    gameManager.ShowFeedback(message, Color.white);
                }
                else
                {
                    gameManager.ShowFeedback("Дверь закрыта. Выполните задание OLD", Color.yellow);
                }
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