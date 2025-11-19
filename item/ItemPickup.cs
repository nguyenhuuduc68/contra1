using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Hiệu ứng rơi")]
    public float upwardForce = 6f;    // Lực đẩy item bay lên
    public float floatTime = 0.4f;    // Thời gian bay lên trước khi rơi

    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool hasLanded = false;
    private bool canBeCollected = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // ✅ Tắt collider 2 giây đầu khi item mới spawn
        boxCollider.enabled = false;
    }

    private void Start()
    {
        // Bay nhẹ lên
        rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
        Invoke(nameof(EnableGravity), floatTime);

        // ✅ Bật collider lại sau 1 giây (để tránh va chạm sớm)
        Invoke(nameof(EnableColliderAfterDelay), 1f);
    }

    private void EnableGravity()
    {
        rb.gravityScale = 2f;
    }

    private void EnableColliderAfterDelay()
    {
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
            Debug.Log("✅ BoxCollider bật lại sau 1 giây – có thể va chạm.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 ||
            collision.collider.CompareTag("Ground"))
        {
            if (!hasLanded)
            {
                hasLanded = true;
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;

                // Cho phép trigger với Player để nhặt
                boxCollider.isTrigger = true;
                canBeCollected = true;
                Debug.Log("🎁 Item đã rơi xuống đất – có thể nhặt.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected || !other.CompareTag("Player")) return;

        PlayerShooting shooter = other.GetComponent<PlayerShooting>();
        if (shooter == null) return;

        // 🔥 Lấy tag của item để xác định loại vũ khí
        if (CompareTag("Item_M"))
            shooter.UpgradeWeaponPermanent("M");
        else if (CompareTag("Item_L"))
            shooter.UpgradeWeaponPermanent("L");
        else if (CompareTag("Item_S"))
            shooter.UpgradeWeaponPermanent("S");
        else
            shooter.UpgradeWeaponPermanent("Normal");

        Debug.Log($"🚀 Player nhặt item {gameObject.tag}");
        Destroy(gameObject);
    }
}
