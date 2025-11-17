using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý di chuyển, nhảy/lộn và tư thế (đứng, ngồi, nằm) của Player.
/// - Ở trên không: bật animation Jump & thu nhỏ BoxCollider.
/// - Chạm đất: khôi phục collider & tắt Jump.
/// - Lộn nhào: tắt BoxCollider trong flipColliderDisableTime, sau đó bật lại ở dạng thu nhỏ cho tới khi chạm đất.
/// - Khi chết: khoá toàn bộ input, tắt chuyển động & collider (được gọi từ HealthSystem).
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    #region Refs
    private Rigidbody2D rb;
    private Animator animator;
    private BoxCollider2D boxCollider2D;
    private HealthSystem healthSystem;
    #endregion

    #region Inspector
    [Header("Cài đặt di chuyển")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Kiểm tra mặt đất")]
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Cài đặt collider khi nhảy/lộn")]
    [Tooltip("Tỉ lệ thu nhỏ collider khi ở trên không.")]
    public Vector2 jumpColliderScale = new Vector2(1.2f, 0.6f);

    [Tooltip("Độ lệch offset khi collider thu nhỏ trong lúc ở trên không.")]
    public Vector2 jumpColliderOffset = new Vector2(0f, -0.1f);

    [Tooltip("Thời gian tắt BoxCollider khi lộn (giây).")]
    public float flipColliderDisableTime = 0.5f;

    [Header("Thời gian chờ giữa các lần nhảy (giây)")]
    public float jumpCooldown = 1f;   // ✅ thêm: thời gian giữa 2 lần nhảy
    #endregion

    #region State
    private bool isGrounded;
    private bool wasGrounded = true;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;

    private bool isFlipping = false;
    private bool blockJump = false;
    private bool blockAllInputs = false;
    private Coroutine flipRoutine;

    private float lastJumpTime = -10f; // ✅ lưu thời điểm nhảy gần nhất
    #endregion

    #region Unity lifecycle
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        healthSystem = GetComponent<HealthSystem>();

        standingColliderSize = boxCollider2D.size;
        standingColliderOffset = boxCollider2D.offset;
    }

    private void Update()
    {
        if (blockAllInputs || (healthSystem != null && healthSystem.isDead))
            return;

        CheckGround();
        HandleMovement();
        HandleJumpInput();
        HandlePosture();
    }
    #endregion

    #region Ground check
    private void CheckGround()
    {
        bool currentGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
        animator.SetBool("Grounded", currentGrounded);

        if (!currentGrounded && wasGrounded)
        {
            animator.SetTrigger("Jump");
        }

        if (currentGrounded && !wasGrounded)
        {
            RestoreCollider();
            animator.ResetTrigger("Jump");
        }

        wasGrounded = currentGrounded;
        isGrounded = currentGrounded;
    }
    #endregion

    #region Movement / Input
    private void HandleMovement()
    {
        float moveInput = 0f;

        if (!animator.GetBool("IsProne") && !animator.GetBool("IsCrouching"))
        {
            if (Input.GetKey(KeyCode.A)) moveInput = -1f;
            else if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput != 0 && isGrounded)
        {
            transform.localScale = new Vector3(moveInput > 0 ? 1 : -1, 1, 1);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Idle", false);
        }
        else
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("Idle", isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.001f);
        }
    }

    /// <summary>Nhấn W để nhảy (Contra: nhảy = flip) chỉ khi chạm đất & hết cooldown.</summary>
    private void HandleJumpInput()
    {
        if (blockJump) return;

        // ✅ chỉ cho phép nhảy nếu đã qua cooldown
        bool canJumpNow = (Time.time - lastJumpTime) >= jumpCooldown;

        if (Input.GetKeyDown(KeyCode.W) && isGrounded
            && canJumpNow
            && !animator.GetBool("IsCrouching")
            && !animator.GetBool("IsProne"))
        {
            lastJumpTime = Time.time;  // ✅ ghi lại thời điểm nhảy
            StartFlip();
        }
    }

    private void HandlePosture()
    {
        if (animator.GetBool("IsCrouching") && Input.GetKey(KeyCode.S)) return;
        if (animator.GetBool("IsProne") && Input.GetKey(KeyCode.J)) return;

        if (Input.GetKey(KeyCode.S) && isGrounded && !animator.GetBool("IsCrouching"))
        {
            animator.SetBool("IsProne", true);
            animator.SetBool("IsCrouching", false);
            boxCollider2D.offset = new Vector2(standingColliderOffset.x, -standingColliderSize.y * 0.29f);
            boxCollider2D.size = new Vector2(standingColliderSize.x * 3.3f, standingColliderSize.y * 0.4f);
        }
        else if (Input.GetKey(KeyCode.J) && isGrounded && !animator.GetBool("IsProne"))
        {
            animator.SetBool("IsCrouching", true);
            animator.SetBool("IsProne", false);
            boxCollider2D.offset = new Vector2(standingColliderOffset.x, -standingColliderSize.y * 0.125f);
            boxCollider2D.size = new Vector2(standingColliderSize.x, standingColliderSize.y * 0.75f);
        }
        else if (isGrounded && !Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.S))
        {
            animator.SetBool("IsCrouching", false);
            animator.SetBool("IsProne", false);
            RestoreCollider();
        }
    }
    #endregion

    #region Flip / Jump
    public void StartFlip()
    {
        if (isFlipping) return;

        isFlipping = true;
        animator.SetTrigger("Jump");
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        flipRoutine = StartCoroutine(HandleFlipCollider());
    }

    private IEnumerator HandleFlipCollider()
    {
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = false;
        }

        yield return new WaitForSeconds(flipColliderDisableTime);

        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
            ShrinkColliderForJump();
        }

        yield return new WaitUntil(() => isGrounded);

        RestoreCollider();

        isFlipping = false;
        flipRoutine = null;
    }

    public void ForceStopFlip()
    {
        if (!isFlipping) return;

        isFlipping = false;

        if (flipRoutine != null)
        {
            StopCoroutine(flipRoutine);
            flipRoutine = null;
        }

        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
            RestoreCollider();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
        }
    }
    #endregion

    #region Collider helpers
    private void ShrinkColliderForJump()
    {
        if (boxCollider2D == null) return;

        boxCollider2D.size = new Vector2(
            standingColliderSize.x * jumpColliderScale.x,
            standingColliderSize.y * jumpColliderScale.y
        );
        boxCollider2D.offset = new Vector2(
            standingColliderOffset.x + jumpColliderOffset.x,
            standingColliderOffset.y + jumpColliderOffset.y
        );
    }

    private void RestoreCollider()
    {
        if (boxCollider2D == null) return;

        boxCollider2D.size = standingColliderSize;
        boxCollider2D.offset = standingColliderOffset;
    }
    #endregion

    #region Death control APIs
    public void StopAllMotion()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("Idle", false);
            animator.SetBool("IsCrouching", false);
            animator.SetBool("IsProne", false);
            animator.ResetTrigger("Jump");
        }

        if (boxCollider2D != null)
            boxCollider2D.enabled = false;
    }

    public void EnableCollider()
    {
        if (boxCollider2D != null)
        {
            boxCollider2D.enabled = true;
            RestoreCollider();
        }
    }

    public int GetRealFacingDirection()
    {
        if (Input.GetKey(KeyCode.A)) return -1;
        if (Input.GetKey(KeyCode.D)) return 1;
        return transform.localScale.x > 0 ? 1 : -1;
    }

    public void BlockAllInputs(bool block)
    {
        blockAllInputs = block;
    }
    #endregion

    #region Getters / Debug
    public bool GetIsFlipping() => isFlipping;
    public bool GetIsGrounded() => isGrounded;
    public Animator GetAnimator() => animator;
    public bool GetIsCrouching() => animator.GetBool("IsCrouching");
    public bool GetIsProne() => animator.GetBool("IsProne");
    public int GetFacingDirection() => transform.localScale.x > 0 ? 1 : -1;
    public void BlockJumpInput(bool block) => blockJump = block;

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.25f);
        }
    }
    #endregion
}
