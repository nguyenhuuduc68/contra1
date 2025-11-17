using UnityEngine;

/// <summary>
/// Viên đạn của Player:
/// - Bay theo hướng bắn.
/// - Tự hủy khi vượt quá quãng đường cho phép.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [Header("Cấu hình viên đạn Player")]
    [Tooltip("Tốc độ bay (unit/giây).")]
    public float speed = 12f;

    [Tooltip("Chiều dài đường bay tối đa (unit).")]
    public float maxDistance = 10f;

    [Tooltip("Sát thương mỗi viên đạn.")]
    public int damage = 1;

    private Rigidbody2D rb;
    private Vector3 startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void OnEnable()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Kiểm tra quãng đường đã bay
        float distance = Vector3.Distance(startPos, transform.position);
        if (distance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Đặt hướng bay cho viên đạn (do script PlayerShooting gọi).
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        startPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💥 Gặp Enemy
        if (other.CompareTag("Enemy"))
        {
            RunningEnemy enemy = other.GetComponent<RunningEnemy>();
            if (enemy != null)
            {
                if (enemy.explosionEffect != null)
                    Instantiate(enemy.explosionEffect, enemy.transform.position, Quaternion.identity);
                Destroy(enemy.gameObject);
            }
            Destroy(gameObject);
        }
        // 💥 Gặp Turret
        else if (other.CompareTag("Turret"))
        {
            TurretHealth hp = other.GetComponent<TurretHealth>();
            if (hp != null)
                hp.TakeDamage(damage);
            Destroy(gameObject);
        }
        // Gặp nước
        else if (other.CompareTag("Water"))
        {
            Destroy(gameObject);
        }
    }
}
