using System.Collections;
using UnityEngine;

public class TurretHealth : MonoBehaviour
{
    public int maxHP = 4;
    private int currentHP;

    public GameObject explosionEffect;
    public TurretController turretController;

    private Animator animator;
    private SpriteRenderer bodyRenderer;
    private bool isDestroyed = false;
    private bool invincible = false; // ✅ tránh bị trừ máu 2 lần trong 1 frame

    private void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
        bodyRenderer = GetComponent<SpriteRenderer>();

        if (turretController == null)
            turretController = GetComponent<TurretController>();
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDestroyed || invincible) return; // ✅ bỏ qua nếu đang bị tính damage

        StartCoroutine(DamageCooldown());
        currentHP -= damage;
        Debug.Log($"💥 Turret bị bắn! HP còn: {currentHP}");

        if (currentHP <= 0)
            Die();
        else
            StartCoroutine(FlashRed());
    }

    private IEnumerator DamageCooldown()
    {
        invincible = true;
        yield return new WaitForSeconds(0.05f); // khoảng cách 1 frame (50ms)
        invincible = false;
    }

    private IEnumerator FlashRed()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            bodyRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        animator.enabled = false;
        if (turretController != null)
            turretController.enabled = false;
        if (turretController != null && turretController.barrel != null)
            turretController.barrel.gameObject.SetActive(false);
        if (bodyRenderer != null)
            bodyRenderer.enabled = false;

        Debug.Log("💣 Turret bị phá hủy hoàn toàn!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
