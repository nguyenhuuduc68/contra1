using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator))]
public class ItemBox : MonoBehaviour
{
    [Header("Cài đặt HP và thời gian mở/đóng")]
    public int health = 3;
    public float openDuration = 3f;     // thời gian mở
    public float closeDuration = 3f;    // thời gian đóng

    [Header("Hiệu ứng & Prefab")]
    public GameObject explosionPrefab;
    public GameObject itemPrefab;

    private bool isOpen = false;
    private bool isDestroyed = false;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private Coroutine openCloseLoop;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }

    private void Start()
    {
        // 🔁 Bắt đầu vòng lặp mở/đóng liên tục
        openCloseLoop = StartCoroutine(OpenCloseLoop());
    }

    private IEnumerator OpenCloseLoop()
    {
        yield return new WaitForSeconds(0.5f); // đợi camera đến
        while (!isDestroyed)
        {
            OpenBox();
            yield return new WaitForSeconds(openDuration);

            CloseBox();
            yield return new WaitForSeconds(closeDuration);
        }
    }

    private void OpenBox()
    {
        if (isDestroyed) return;
        isOpen = true;

        // ✅ Bật collider nhưng KHÔNG đụng tới isTrigger
        boxCollider.enabled = true;

        animator?.SetTrigger("Open");
        Debug.Log("📦 ItemBox MỞ (đang nhận đạn)");
    }

    private void CloseBox()
    {
        if (isDestroyed) return;
        isOpen = false;

        // ✅ Tắt collider, không thay đổi isTrigger
        boxCollider.enabled = false;

        animator?.SetTrigger("Close");
        Debug.Log("📦 ItemBox ĐÓNG (ngừng nhận đạn)");
    }

    public void TakeDamage(int damage)
    {
        if (!isOpen || isDestroyed) return;

        health -= damage;
        Debug.Log($"💥 ItemBox trúng đạn! HP còn lại: {health}");

        if (health <= 0)
            DestroyBox();
    }

    private void DestroyBox()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 🛑 Dừng vòng lặp
        if (openCloseLoop != null)
            StopCoroutine(openCloseLoop);

        boxCollider.enabled = false;
        animator?.ResetTrigger("Open");
        animator?.SetTrigger("Close");

        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        if (itemPrefab)
        {
            GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0;
            }
        }

        Destroy(gameObject);
        Debug.Log("💥 ItemBox bị phá hủy!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpen || isDestroyed) return;

        if (collision.CompareTag("PlayerBullet"))
        {
            PlayerBullet bullet = collision.GetComponent<PlayerBullet>();
            if (bullet != null)
                TakeDamage(bullet.damage);

            Destroy(collision.gameObject);
        }
    }
}
