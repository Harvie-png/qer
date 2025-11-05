using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Pickup Settings")]
    public float respawnTime = 5f;
    public int healAmount = 1;
    public bool activeInFirstPhase = false; // В какой фазе активна

    private bool isActive = true;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2d;
    private BossOLD boss;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        boss = FindObjectOfType<BossOLD>();

        // Сразу скрываем если не должна быть активна в первой фазе
        if (!activeInFirstPhase && boss != null && !boss.IsSecondPhase())
        {
            DeactivatePickup();
        }
    }

    void Update()
    {
        // Автоматически активируем/деактивируем в зависимости от фазы босса
        if (boss != null)
        {
            bool shouldBeActive = activeInFirstPhase || boss.IsSecondPhase();
            if (shouldBeActive && !isActive)
            {
                ActivatePickup();
            }
            else if (!shouldBeActive && isActive)
            {
                DeactivatePickup();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                bool healed = playerHealth.Heal(healAmount);
                if (healed)
                {
                    Debug.Log("Игрок подобрал здоровье!");

                    // Деактивируем хилку
                    DeactivatePickup();

                    // Запускаем респавн
                    Invoke("ActivatePickup", respawnTime);
                }
                else
                {
                    Debug.Log("Игрок пытался подобрать здоровье, но у него полное HP");
                }
            }
        }
    }

    void DeactivatePickup()
    {
        isActive = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (collider2d != null)
            collider2d.enabled = false;
    }

    void ActivatePickup()
    {
        // Проверяем можно ли активировать в текущей фазе
        if (boss != null)
        {
            bool shouldBeActive = activeInFirstPhase || boss.IsSecondPhase();
            if (!shouldBeActive) return;
        }

        isActive = true;
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (collider2d != null)
            collider2d.enabled = true;

        Debug.Log("Хилка восстановилась!");
    }

    // Метод для принудительной активации/деактивации извне
    public void SetActiveState(bool active)
    {
        if (active)
        {
            ActivatePickup();
        }
        else
        {
            DeactivatePickup();
        }
    }
}