using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;

    private Vector2 direction;
    private bool isPlayerProjectile = false;
    private bool isSuperProjectile = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    public void SetShooter(bool isPlayer)
    {
        isPlayerProjectile = isPlayer;

        if (isPlayer)
        {
            gameObject.tag = "PlayerProjectile";
        }
        else
        {
            gameObject.tag = "BossProjectile";
        }
    }

    public void SetSuperProjectile(bool isSuper)
    {
        isSuperProjectile = isSuper;
        if (isSuper)
        {
            // Визуальное отличие супер-снаряда
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // УБРАЛ ЗАЩИТУ ОТ ДВОЙНОГО УРОНА - damageApplied

        int damage = isSuperProjectile ? 2 : 1;

        Debug.Log($"Снаряд столкнулся: обычный={!isSuperProjectile}, супер={isSuperProjectile}, урон={damage}");

        // СНАРЯД ИГРОКА
        if (isPlayerProjectile)
        {
            if (other.CompareTag("Boss"))
            {
                BossOLD boss = other.GetComponent<BossOLD>();
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                    Debug.Log($"Нанесен урон боссу: {damage} (супер: {isSuperProjectile})");
                }
                Destroy(gameObject);
            }
            else if (other.CompareTag("Player") || other.CompareTag("PlayerProjectile"))
            {
                Debug.Log("Снаряд игрока проигнорировал своего или игрока");
                return;
            }
        }
        // СНАРЯД БОССА  
        else
        {
            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Нанесен урон игроку: {damage} (супер: {isSuperProjectile})");
                }
                Destroy(gameObject);
            }
            else if (other.CompareTag("Boss") || other.CompareTag("BossProjectile"))
            {
                Debug.Log("Снаряд босса проигнорировал своего или босса");
                return;
            }
        }

        if (other.CompareTag("Wall"))
        {
            Debug.Log("Снаряд уничтожен о стену");
            Destroy(gameObject);
        }
    }
}