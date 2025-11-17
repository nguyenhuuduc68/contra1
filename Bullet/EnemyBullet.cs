using UnityEngine;

/// <summary>
/// Viên đạn của Enemy di chuyển (chạy, nhảy):
/// - Bay theo hướng bắn.
/// - Tự hủy sau lifeTime hoặc khi chạm Player.
/// - Có thể bay tầm ngắn hơn turret.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Cấu hình viên đạn Enemy")]
    public float speed = 8f;
    public float lifeTime = 2f;
    public int damage = 1;

    private Rigidbody2D rb;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem hp = other.GetComponent<HealthSystem>();
            if (hp != null)
                hp.TakeDamage();

            Destroy(gameObject);
        }
        else if (other.CompareTag("Water"))
        {
            Destroy(gameObject);
        }
    }
}
