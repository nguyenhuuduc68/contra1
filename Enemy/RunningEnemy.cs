using UnityEngine;

/// <summary>
/// Điều khiển Enemy biết chạy và tự nhảy khi hết đất (rơi khỏi mép platform).
/// Enemy sẽ:
/// - Luôn chạy sang trái.
/// - Khi phát hiện không còn mặt đất dưới chân thì nhảy.
/// - Nếu va chạm tường chết (DeathWall) hoặc bị trúng đạn → nổ và bị hủy.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class RunningEnemy : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Cài đặt di chuyển")]
    [Tooltip("Tốc độ di chuyển của Enemy.")]
    public float speed = 3f;

    [Tooltip("Lực nhảy của Enemy.")]
    public float jumpForce = 4f;

    [Header("Kiểm tra mặt đất")]
    [Tooltip("Điểm kiểm tra mặt đất (GroundCheck).")]
    public Transform groundCheck;

    [Tooltip("Lớp layer đại diện cho mặt đất.")]
    public LayerMask groundLayer;

    [Header("Hiệu ứng khi bị trúng đạn hoặc chết")]
    public GameObject explosionEffect;

    private bool isGrounded;
    private bool isJumping;
    private Camera mainCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        CheckGround();

        // Enemy luôn di chuyển sang trái
        rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);

        // Nếu sắp rơi (không còn ground bên dưới) thì nhảy
        if (!isGrounded && !isJumping)
        {
            Jump();
        }

        // Nếu Enemy ra khỏi màn hình bên trái thì tự hủy
        float cameraLeftEdge = mainCamera.ViewportToWorldPoint(Vector3.zero).x;
        if (transform.position.x < cameraLeftEdge - 15f)
            Destroy(gameObject);

        // Cập nhật thông tin cho Animator
        animator.SetFloat("Speed", Mathf.Abs(speed));
        animator.SetBool("IsGrounded", isGrounded);
    }

    /// <summary>
    /// Kiểm tra xem Enemy có đang đứng trên mặt đất hay không.
    /// </summary>
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.25f, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0)
            isJumping = false;
    }

    /// <summary>
    /// Thực hiện hành động nhảy khi không còn mặt đất.
    /// </summary>
    private void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        animator.SetTrigger("Jump");
    }

    /// <summary>
    /// Xử lý va chạm với đạn, Player hoặc tường chết.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ⚔️ Gặp Player → gây sát thương cho Player rồi nổ
        if (collision.CompareTag("Player"))
        {
            HealthSystem health = collision.GetComponent<HealthSystem>();
            if (health != null)
                health.TakeDamage();

            ExplodeAndDestroy();
        }

        // 💣 Bị trúng đạn → nổ và biến mất
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject); // Xóa viên đạn
            ExplodeAndDestroy();
        }

        // ☠️ Gặp tường chết (DeathWall) → nổ chết luôn
        if (collision.CompareTag("DeathWall"))
        {
            ExplodeAndDestroy();
        }
    }

    /// <summary>
    /// Tạo hiệu ứng nổ và hủy đối tượng Enemy.
    /// </summary>
    private void ExplodeAndDestroy()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.25f);
        }
    }
}
