using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Death UI")]
    public GameObject deathPanel;
    public TMP_Text deathText;
    public Button restartButton;
    // Убрал menuButton - используем существующую из PlayerController

    private Vector3 startPosition;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;

        if (deathPanel != null)
            deathPanel.SetActive(false);

        // Настраиваем только кнопку рестарта
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        Debug.Log($"=== ИГРОК ПОЛУЧАЕТ УРОН ===");
        Debug.Log($"Было HP: {currentHealth}, урон: {damage}");

        currentHealth -= damage;

        Debug.Log($"Стало HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;
        Debug.Log("Игрок погиб!");

        // Останавливаем бой
        StopAllFighting();

        // Показываем панель смерти
        ShowDeathPanel();
    }

    void StopAllFighting()
    {
        // Останавливаем босса
        BossOLD boss = FindObjectOfType<BossOLD>();
        if (boss != null)
        {
            boss.StopBossFight();
        }

        // Отключаем управление и стрельбу
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;

        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
            playerCombat.enabled = false;
    }

    void ShowDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            if (deathText != null)
                deathText.text = "Вы погибли!\nХотите попробовать снова?";
        }
    }

    void RestartLevel()
    {
        Debug.Log("Перезапуск уровня...");

        // Сбрасываем здоровье
        currentHealth = maxHealth;
        isDead = false;

        // Возвращаем игрока на старт
        transform.position = startPosition;

        // Скрываем панель смерти
        if (deathPanel != null)
            deathPanel.SetActive(false);

        // Уничтожаем все снаряды
        DestroyAllProjectiles();

        // Перезапускаем кат-сцену и бой
        RestartBossFight();
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

    void RestartBossFight()
    {
        // Сбрасываем босса
        BossOLD boss = FindObjectOfType<BossOLD>();
        if (boss != null)
        {
            boss.ResetBoss();
        }

        // Включаем управление
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = true;

        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
            playerCombat.enabled = true;

        // Запускаем кат-сцену заново
        CutsceneManager cutscene = FindObjectOfType<CutsceneManager>();
        if (cutscene != null)
        {
            cutscene.RestartCutscene();
        }
    }

    public bool Heal(int amount)
    {
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Нельзя лечиться - полное здоровье!");
            return false; // Не лечим если полное здоровье
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Игрок вылечен на {amount}. Теперь HP: {currentHealth}/{maxHealth}");
        return true; // Успешно полечились
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
        isDead = false;
        Debug.Log($"Игрок полностью вылечен! HP: {currentHealth}/{maxHealth}");
    }
}