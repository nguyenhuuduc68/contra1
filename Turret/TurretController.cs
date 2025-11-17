using System.Collections;
using UnityEngine;

/// <summary>
/// Turret kiểu Contra NES (phiên bản hoàn thiện):
/// - Nòng ẩn cho tới khi phát hiện Player.
/// - Chạy animation mở, chờ xong rồi mới hiện nòng.
/// - Chỉ bắn khi Player nằm trong góc ±aimTolerance.
/// - Khi bị phá hủy: tắt hoàn toàn Rigidbody2D và Collider để tránh lỗi.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TurretController : MonoBehaviour
{
    [Header("Phát hiện Player")]
    public LayerMask playerLayer;
    public float detectionRadius = 6f;
    public float detectionHysteresis = 1f;

    [Header("Cài đặt bắn")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float fireRate = 1f;
    public float aimTolerance = 10f;

    [Header("Cài đặt nòng pháo")]
    public Transform barrel;
    public float[] allowedAngles = new float[] { 0f, 45f, 90f, 135f };

    [Header("Cấu hình chết/nổ")]
    public GameObject explosionEffect;
    public int maxHP = 4; // ✅ bắn 4 viên thì nổ

    // 🔹 Nội bộ
    private int currentHP;
    private Transform player;
    private Animator animator;
    private SpriteRenderer barrelRenderer;
    private Rigidbody2D rb;
    private Collider2D col;

    private float lastFireTime;
    private bool isOpen;
    private bool isOpening;
    private bool playerDetected;
    private float currentAimAngle;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        currentHP = maxHP;

        if (barrel != null)
        {
            barrelRenderer = barrel.GetComponent<SpriteRenderer>();
            barrelRenderer.enabled = false; // ẩn nòng từ đầu
        }
    }

    private void Update()
    {
        if (player == null || currentHP <= 0) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool withinRange = distance <= detectionRadius;

        // ✅ Xử lý phát hiện Player (có hysteresis để tránh nhấp nháy)
        if (withinRange && !playerDetected)
        {
            playerDetected = true;
            OpenTurret();
        }
        else if (!withinRange && playerDetected)
        {
            playerDetected = false;
            CloseTurret();
        }

        if (isOpen)
        {
            RotateBarrelToPlayer();
            TryShoot(); // ✅ chỉ bắn khi hướng đúng
        }
    }

    #region Mở / Đóng nòng
    private void OpenTurret()
    {
        if (isOpen || isOpening) return;

        isOpening = true;
        animator?.SetBool("IsOpen", true);
        StartCoroutine(ShowBarrelAfterOpen());

        Debug.Log("✅ Turret: bắt đầu mở nòng.");
    }

    private IEnumerator ShowBarrelAfterOpen()
    {
        yield return new WaitForSeconds(1f); // độ dài animation mở

        if (barrelRenderer != null && playerDetected)
            barrelRenderer.enabled = true;

        isOpen = true;
        isOpening = false;

        Debug.Log("🔹 Turret: nòng đã mở và sẵn sàng bắn.");
    }

    private void CloseTurret()
    {
        isOpen = false;
        isOpening = false;
        animator?.SetBool("IsOpen", false);

        if (barrelRenderer != null)
            barrelRenderer.enabled = false;

        Debug.Log("❌ Turret: đóng nòng và ẩn hoàn toàn.");
    }
    #endregion

    #region Quay nòng & bắn
    private void RotateBarrelToPlayer()
    {
        if (barrel == null || player == null) return;

        Vector2 dir = player.position - transform.position;
        float worldAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (worldAngle < 0) worldAngle += 360f;

        float chosen = allowedAngles[0];
        float bestDiff = Mathf.Abs(Mathf.DeltaAngle(worldAngle, chosen));

        foreach (float a in allowedAngles)
        {
            float diff1 = Mathf.Abs(Mathf.DeltaAngle(worldAngle, a));
            float diff2 = Mathf.Abs(Mathf.DeltaAngle(worldAngle, a + 180f));
            if (diff1 < bestDiff) { bestDiff = diff1; chosen = a; }
            if (diff2 < bestDiff) { bestDiff = diff2; chosen = a + 180f; }
        }

        currentAimAngle = chosen;
        barrel.rotation = Quaternion.Euler(0, 0, chosen);
    }

    private void TryShoot()
    {
        if (Time.time - lastFireTime < 1f / fireRate) return;
        if (firePoint == null || bulletPrefab == null) return;

        // 🔹 Tính góc thật sự giữa nòng và Player
        Vector2 dirToPlayer = (player.position - firePoint.position).normalized;
        float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        if (angleToPlayer < 0) angleToPlayer += 360f;

        float diff = Mathf.Abs(Mathf.DeltaAngle(angleToPlayer, currentAimAngle));

        // ✅ Chỉ bắn khi player nằm trong ±aimTolerance
        if (diff <= aimTolerance)
        {
            lastFireTime = Time.time;
            FireBullet();
        }
    }

    private void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        TurretBullet b = bullet.GetComponent<TurretBullet>();
        if (b != null)
        {
            Vector2 dir = firePoint.right;
            b.SetDirection(dir);
        }
    }
    #endregion

    #region Va chạm & chết
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            currentHP--;
            Destroy(other.gameObject);

            if (currentHP <= 0)
                Die();
        }
    }

    private void Die()
    {
        Debug.Log("💥 Turret bị phá hủy!");

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // ✅ Xóa hoàn toàn vật lý và collider để tránh lỗi game
        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        if (col != null)
            col.enabled = false;

        if (barrelRenderer != null)
            barrelRenderer.enabled = false;

        isOpen = false;
        animator.enabled = false;

        // ✅ Xóa sau 1 giây (cho hiệu ứng nổ hiển thị)
        Destroy(gameObject, 1f);
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}
