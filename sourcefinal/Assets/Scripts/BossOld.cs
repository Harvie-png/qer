using UnityEngine;


public class BossOLD : MonoBehaviour
{
    [Header("Boss Settings")]
    public int health = 5;
    public int maxHealth = 5;
    public float moveSpeed = 3f;
    public float shootInterval = 2f;
    public GameObject projectilePrefab;
    public GameObject superProjectilePrefab; // Префаб супер-снаряда

    [Header("Movement Range")]
    public float minY = -8f;
    public float maxY = 8f;

    [Header("Second Phase")]
    public int superShotInterval = 3; // Каждый 3-й выстрел - супер
    public int secondPhaseHealth = 10;

    private float shootTimer;
    private int moveDirection = 1;
    private bool bossFightActive = false;
    private Vector3 startPosition;
    private int shotCount = 0;
    private bool secondPhase = false;

    void Start()
    {
        startPosition = transform.position;
        maxHealth = health;
    }

    public void StartBossFight()
    {
        bossFightActive = true;
        Debug.Log("Бой с боссом начался!");
    }

    public void StopBossFight()
    {
        bossFightActive = false;
        Debug.Log("Бой с боссом остановлен");
    }

    public void ResetBoss()
    {
        transform.position = startPosition;
        health = maxHealth;
        moveDirection = 1;
        shootTimer = 0f;
        shotCount = 0;
        secondPhase = false;
        bossFightActive = false;

        Debug.Log("Босс сброшен в начальное состояние");
    }

    void Update()
    {
        if (!bossFightActive) return;

        MovePattern();

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void MovePattern()
    {
        float newY = transform.position.y + moveDirection * moveSpeed * Time.deltaTime;

        if (newY >= maxY || newY <= minY)
        {
            moveDirection *= -1;
            newY = Mathf.Clamp(newY, minY, maxY);
        }

        transform.position = new Vector2(transform.position.x, newY);
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        shotCount++;
        bool isSuperShot = secondPhase && (shotCount % superShotInterval == 0);

        Debug.Log($"Выстрел {shotCount}, супер-выстрел: {isSuperShot}"); // ← ДОБАВЬ ОТЛАДКУ

        GameObject projectileToUse = isSuperShot ? superProjectilePrefab : projectilePrefab;

        if (projectileToUse != null)
        {
            GameObject projectile = Instantiate(projectileToUse, transform.position, Quaternion.identity);

            Vector2 direction = Vector2.left;
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                projScript.SetShooter(false);
                if (isSuperShot)
                {
                    projScript.SetSuperProjectile(true);
                    Debug.Log("Босс выпустил СУПЕР-снаряд!"); // ← ДОЛЖНО БЫТЬ В КОНСОЛИ
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"=== БОСС ПОЛУЧАЕТ УРОН ===");
        Debug.Log($"Текущее HP: {health}, получает урон: {damage}");

        health -= damage;

        Debug.Log($"Новое HP: {health}");

        if (health <= 0 && !secondPhase)
        {
            StartSecondPhase();
        }
        else if (health <= 0)
        {
            Debug.Log("БОСС ОКОНЧАТЕЛЬНО ПОБЕЖДЕН!");
            Die();
        }
    }

    void DestroyAllProjectiles()
    {
        Projectile[] projectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile projectile in projectiles)
        {
            Destroy(projectile.gameObject);
        }
        Debug.Log($"Уничтожено снарядов: {projectiles.Length}");
    }

    void HealPlayerToFull()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.HealToFull();
            Debug.Log("Игрок полностью вылечен");
        }
    }

    void Die()
    {
        bossFightActive = false;
        Debug.Log("Босс окончательно побежден! Уровень пройден!");
        // TODO: Победа, переход к финальной кат-сцене
    }
    public bool IsSecondPhase()
    {
        return secondPhase;
    }

    void StartSecondPhase()
    {
        Debug.Log("=== НАЧИНАЕТСЯ ВТОРАЯ ФАЗА ===");

        secondPhase = true;
        health = secondPhaseHealth;
        maxHealth = secondPhaseHealth;

        // Возвращаем на стартовую позицию
        transform.position = startPosition;

        // Сбрасываем счетчик выстрелов
        shotCount = 0;

        Debug.Log($"Босс восстановлен до {health} HP, начата вторая фаза!");

        // Уничтожаем все снаряды на сцене
        DestroyAllProjectiles();

        // Полностью лечим игрока
        HealPlayerToFull();

        // Активируем хилки для второй фазы
        ActivateSecondPhasePickups();
    }

    void ActivateSecondPhasePickups()
    {
        HealthPickup[] pickups = FindObjectsOfType<HealthPickup>();
        foreach (HealthPickup pickup in pickups)
        {
            // Активируем все хилки которые должны быть во второй фазе
            if (!pickup.activeInFirstPhase)
            {
                pickup.SetActiveState(true);
            }
        }
        Debug.Log($"Активированы хилки второй фазы: {pickups.Length}");
    }
}