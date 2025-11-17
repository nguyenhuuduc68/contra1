using UnityEngine;

/// <summary>
/// Enemy cố định (stationary):
/// - Phát hiện Player trong phạm vi.
/// - Chỉ bắn nếu Player nằm trong góc cho phép (45°, 90°, 135°, ...).
/// - Bắn ra đạn EnemyBullet.
/// - Bị trúng đạn PlayerBullet thì nổ.
/// </summary>
public class StationaryEnemy : MonoBehaviour
{
    [Header("Thành phần")]
    private Animator animator;
    public Transform firePoint;
    public GameObject explosionEffect;

    [Header("Cài đặt bắn")]
    public GameObject bulletPrefab;
    public float range = 8f;
    public float fireRate = 2f;
    public float[] shootAngles = { 45f, 90f, 135f }; // Góc hợp lệ (tùy hướng enemy)

    private Transform player;
    private float nextFireTime;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Kiểm tra thiếu thành phần
        if (animator == null)
            Debug.LogWarning("⚠️ Missing Animator on " + name);
        if (firePoint == null)
            Debug.LogWarning("⚠️ FirePoint chưa gán trên " + name);
        if (bulletPrefab == null)
            Debug.LogWarning("⚠️ BulletPrefab chưa được gán trên " + name);
        if (player == null)
            Debug.LogWarning("⚠️ Không tìm thấy Player (Tag 'Player')");
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > range) return;

        // Nếu đủ thời gian để bắn tiếp
        if (Time.time >= nextFireTime)
        {
            // Tính hướng đến Player
            Vector2 dir = (player.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // Kiểm tra nếu Player nằm trong 1 trong các góc bắn hợp lệ
            bool canShoot = false;
            foreach (float allowed in shootAngles)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(angle, allowed)) <= 10f)
                {
                    canShoot = true;
                    break;
                }
            }

            // Nếu góc hợp lệ thì bắn
            if (canShoot)
            {
                Shoot(dir);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    /// <summary>
    /// Thực hiện bắn đạn theo hướng đã xác định.
    /// </summary>
    private void Shoot(Vector2 dir)
    {
        if (animator != null)
            animator.SetTrigger("Shoot");

        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(dir);
        }
        else
        {
            // Fallback nếu không có script
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * 8f;
        }

        Debug.Log($"💥 Enemy bắn đạn hướng {dir}");
    }

    /// <summary>
    /// Khi bị đạn Player bắn trúng → nổ và biến mất.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            Debug.Log("💥 StationaryEnemy bị trúng đạn Player!");

            if (explosionEffect != null)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

            Destroy(collision.gameObject); // hủy đạn
            Destroy(gameObject); // hủy enemy
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}
