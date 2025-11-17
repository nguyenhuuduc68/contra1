using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Hệ thống máu và hồi sinh kiểu Contra:
/// - Khi trúng enemy, đạn, hoặc DeathWall → chết ngay, bật animation Die.
/// - Có thể tắt toàn bộ physics khi chết (không rơi).
/// - Hồi sinh gần chỗ chết, có thời gian bất tử.
/// - Khi chết → reset lại đạn thường.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    #region Inspector
    [Header("Cài đặt mạng sống")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Cài đặt bất tử sau hồi sinh")]
    public float invincibilityDuration = 2f;
    private float invincibilityTimer;
    private bool isInvincible = false;

    [Header("Cài đặt hồi sinh gần chỗ chết")]
    public float enemyRespawnOffsetX = 1.5f;
    public float enemyRespawnOffsetY = 1.0f;
    [Space(5)]
    public float wallRespawnOffsetX = 1.5f;
    public float wallRespawnOffsetY = 3.5f;

    [Header("Tùy chọn vật lý khi chết")]
    [Tooltip("Nếu bật, nhân vật sẽ đứng yên và tắt physics khi chết (không rơi).")]
    public bool disablePhysicsOnDeath = true;

    [Header("Cài đặt animation")]
    [Tooltip("Tên STATE Die trong Animator (không phải trigger).")]
    public string dieStateName = "Die";
    [Tooltip("Thời lượng chờ animation Die trước khi hồi sinh.")]
    public float dieAnimDuration = 1.1f;

    [Header("Sự kiện (có thể gán trong Unity)")]
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    [Header("Tham chiếu UI (tuỳ chọn)")]
    [SerializeField] private PlayerLives playerLivesUI;
    #endregion

    #region Private refs & state
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerShooting playerShooting;
    public bool isDead { get; private set; } = false;
    private float originalGravity = 0f;
    #endregion

    #region Unity lifecycle
    private void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponent<PlayerShooting>();

        if (playerLivesUI == null)
            playerLivesUI = FindFirstObjectByType<PlayerLives>();
        playerLivesUI?.UpdateLivesUI(currentLives);

        if (rb != null) originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        // Hiệu ứng nhấp nháy khi đang bất tử
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            spriteRenderer.enabled = Mathf.Sin(Time.time * 10f) > 0;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                spriteRenderer.enabled = true;
            }
        }
    }
    #endregion

    #region Public APIs
    /// <summary>
    /// Gọi khi bị enemy hoặc đạn bắn trúng.
    /// </summary>
    public void TakeDamage()
    {
        if (isInvincible || isDead) return;

        currentLives--;
        playerLivesUI?.UpdateLivesUI(currentLives);
        onTakeDamage?.Invoke();

        if (currentLives > 0)
            StartCoroutine(DieAndRespawn(enemyRespawnOffsetX, enemyRespawnOffsetY, "Enemy/Bullet"));
        else
            StartCoroutine(DieCompletely());
    }

    /// <summary>
    /// Gọi khi chạm DeathWall.
    /// </summary>
    public void TakeDamageFromWall()
    {
        if (isInvincible || isDead) return;

        currentLives--;
        playerLivesUI?.UpdateLivesUI(currentLives);
        onTakeDamage?.Invoke();

        if (currentLives > 0)
            StartCoroutine(DieAndRespawn(wallRespawnOffsetX, wallRespawnOffsetY, "DeathWall"));
        else
            StartCoroutine(DieCompletely());
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Xử lý chết và hồi sinh gần vị trí cũ.
    /// </summary>
    private IEnumerator DieAndRespawn(float offsetX, float offsetY, string cause)
    {
        if (isDead) yield break;
        isDead = true;

        Vector3 deathPos = transform.position;
        Debug.Log($"💀 Player chết tại {deathPos}, nguyên nhân: {cause}");

        // Tắt toàn bộ điều khiển và bật Die
        DisableAllPlayerActions(playDieAnim: true);

        // 🔥 Reset lại vũ khí về đạn thường
        if (playerShooting != null)
        {
            playerShooting.ResetWeapon();
            Debug.Log("💀 Player chết → Reset lại đạn thường!");
        }

        // ✅ Tùy chọn vật lý khi chết
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            if (disablePhysicsOnDeath)
            {
                rb.simulated = false;
                Debug.Log("🧊 Physics OFF khi chết - nhân vật đứng yên.");
            }
            else
            {
                rb.simulated = true;
                rb.gravityScale = originalGravity;
                Debug.Log("💨 Physics ON khi chết - nhân vật vẫn rơi.");
            }
        }

        // Chờ animation Die hoàn tất
        yield return new WaitForSeconds(Mathf.Max(0.05f, dieAnimDuration));

        // Tính vị trí hồi sinh (lùi sau lưng)
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        Vector3 respawnPos = new Vector3(
            deathPos.x + (offsetX * direction),
            deathPos.y + offsetY,
            deathPos.z
        );

        // Đặt lại vị trí & bật lại physics/collider
        transform.position = respawnPos;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = originalGravity;
        }

        playerMovement?.EnableCollider();

        // Hiệu ứng bật dậy nhẹ
        if (animator != null)
        {
            ClearAllAnimatorStates();
            animator.SetTrigger("Jump");
        }

        // Delay ngắn để ổn định trước khi mở điều khiển
        yield return new WaitForSeconds(1f);

        isDead = false;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        playerMovement?.BlockAllInputs(false);
        if (playerShooting != null) playerShooting.enabled = true;

        Debug.Log($"🔁 Player hồi sinh tại {respawnPos}");
    }

    /// <summary>
    /// Chết hoàn toàn (Game Over).
    /// </summary>
    private IEnumerator DieCompletely()
    {
        if (isDead) yield break;
        isDead = true;

        DisableAllPlayerActions(playDieAnim: true);

        // 🔥 Reset vũ khí khi Game Over
        if (playerShooting != null)
        {
            playerShooting.ResetWeapon();
            Debug.Log("💀 Game Over → Reset lại đạn thường!");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        onDeath?.Invoke();

        yield return new WaitForSeconds(Mathf.Max(0.05f, dieAnimDuration));
        gameObject.SetActive(false);
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Dừng mọi input, animation, và bật animation Die.
    /// </summary>
    private void DisableAllPlayerActions(bool playDieAnim)
    {
        if (playerMovement != null)
        {
            playerMovement.BlockAllInputs(true);
            playerMovement.StopAllMotion();
            if (playerMovement.GetIsFlipping())
                playerMovement.ForceStopFlip(); // ✅ Ngắt lộn ngay
        }

        if (playerShooting != null)
        {
            try { playerShooting.SendMessage("ResetShootingState", SendMessageOptions.DontRequireReceiver); }
            catch { }
            playerShooting.enabled = false;
        }

        if (animator != null)
        {
            ClearAllAnimatorStates();

            if (playDieAnim)
            {
                animator.Update(0f);
                animator.CrossFadeInFixedTime(dieStateName, 0.05f, 0, 0f);
                Debug.Log("☠️ Ép vào animation Die ngay lập tức!");
            }
        }
    }

    /// <summary>
    /// Reset toàn bộ bool/trigger để tránh kẹt animation.
    /// </summary>
    private void ClearAllAnimatorStates()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsProne", false);
        animator.SetBool("Idle", false);

        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Die");
    }

    public int GetCurrentLives() => currentLives;
    #endregion
}
