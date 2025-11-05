using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject playerProjectilePrefab;
    public float shootCooldown = 0.5f;

    private float shootTimer;

    void Update()
    {
        // Стрельба по Space
        shootTimer += Time.deltaTime;
        if (Input.GetKey(KeyCode.Space) && shootTimer >= shootCooldown)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (playerProjectilePrefab != null)
        {
            // Создаем снаряд ВНУТРИ игрока
            GameObject projectile = Instantiate(playerProjectilePrefab, transform.position, Quaternion.identity);

            // Снаряд игрока летит вправо (к ОЛДу)
            Vector2 direction = Vector2.right;
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                projScript.SetShooter(true);
                Debug.Log("Снаряд игрока создан и настроен");
            }
            else
            {
                Debug.LogError("Projectile скрипт не найден на префабе снаряда!");
            }
        }
        else
        {
            Debug.LogError("PlayerProjectilePrefab не назначен!");
        }
    }
}