using UnityEngine;

/// <summary>
/// Viên đạn của Tháp Pháo:
/// - Bay theo hướng nòng bắn.
/// - Có giới hạn tầm bay (maxDistance).
/// - Gây damage cho Player.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class TurretBullet : MonoBehaviour
{
    [Header("Cấu hình viên đạn Turret")]
    public float speed = 10f;
    public float maxDistance = 6f;
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
        float dist = Vector3.Distance(startPos, transform.position);
        if (dist >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Đặt hướng bay (gọi từ TurretController khi bắn).
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        startPos = transform.position;
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
