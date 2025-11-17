using UnityEngine;

/// <summary>
/// Điều khiển hành vi của viên đạn (cả từ Player và Enemy).  
/// Xử lý hướng bay, thời gian tồn tại, và va chạm với đối tượng khác.
/// </summary>
public class Bullets : MonoBehaviour
{
    [Header("Cấu hình viên đạn")]
    /// <summary>Tốc độ bay của viên đạn.</summary>
    public float speed = 10f;

    /// <summary>Thời gian sống tối đa của viên đạn (tính bằng giây).</summary>
    public float lifeTime = 3f;

    /// <summary>Xác định nguồn gốc viên đạn.  
    /// <para>True = đạn của Enemy.</para>  
    /// <para>False = đạn của Player.</para>
    /// </summary>
    public bool isEnemyBullet = false;

    /// <summary>Tham chiếu đến Rigidbody2D của viên đạn.</summary>
    private Rigidbody2D rb;

    /// <summary>Hướng di chuyển của viên đạn (đã được chuẩn hóa).</summary>
    private Vector2 direction;

    /// <summary>Bộ đếm thời gian để tự hủy sau khi hết thời gian sống.</summary>
    private float timer;

    /// <summary>
    /// Hàm Awake() khởi tạo và kiểm tra thành phần Rigidbody2D.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("Thiếu Rigidbody2D trên viên đạn!");
    }

    /// <summary>
    /// Hàm gọi khi viên đạn được bật (khi spawn hoặc kích hoạt lại).  
    /// Thiết lập vận tốc ban đầu dựa trên hướng đã định.
    /// </summary>
    private void OnEnable()
    {
        timer = 0f;
        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    /// <summary>
    /// Kiểm tra vòng đời của viên đạn, tự hủy nếu vượt quá thời gian tồn tại.
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    /// <summary>
    /// Đặt hướng bay và loại đạn (Enemy hoặc Player).
    /// </summary>
    /// <param name="dir">Hướng di chuyển (Vector2).</param>
    /// <param name="enemyBullet">True nếu là đạn Enemy, False nếu là đạn Player.</param>
    public void SetDirection(Vector2 dir, bool enemyBullet = false)
    {
        direction = dir.normalized;
        isEnemyBullet = enemyBullet;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        Debug.Log("Bullet direction set: " + direction + ", isEnemyBullet: " + isEnemyBullet);
    }

    /// <summary>
    /// Xử lý va chạm giữa viên đạn và các đối tượng khác.
    /// </summary>
    /// <param name="other">Collider của đối tượng va chạm.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔻 Nếu là đạn của Enemy → gây sát thương cho Player
        if (isEnemyBullet && other.CompareTag("Player"))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                Debug.Log("💥 Enemy bullet hit Player, calling TakeDamage()");
                health.TakeDamage(); // Gọi hệ thống máu xử lý trừ mạng + hồi sinh
            }

            Destroy(gameObject);
        }

        // 🔺 Nếu là đạn của Player → hủy Enemy
        else if (!isEnemyBullet && other.CompareTag("Enemy"))
        {
            RunningEnemy enemy = other.GetComponent<RunningEnemy>();
            if (enemy != null)
            {
                // Gọi hiệu ứng nổ của Enemy (nếu có)
                if (enemy.explosionEffect != null)
                    Instantiate(enemy.explosionEffect, other.transform.position, Quaternion.identity);

                Destroy(other.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
