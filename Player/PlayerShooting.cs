using System.Collections;
using UnityEngine;

/// <summary>
/// Hệ thống bắn tự động của Player kiểu Contra.
/// - Bắn liên tục (auto fire).
/// - Hướng bắn tức thời theo phím O/I hoặc hướng di chuyển.
/// - Khi bắn chéo: khoá nhảy.
/// - Khi ngồi hoặc nằm: chỉ bắn ngang.
/// - Đạn S bắn ra 3 tia (theo hướng hiện tại).
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    #region 🔫 Cấu hình vũ khí
    [Header("Prefabs đạn")]
    public GameObject normalBullet;
    public GameObject BulletM;
    public GameObject BulletL;
    public GameObject BulletS;

    [HideInInspector] public GameObject bulletPrefab;

    [Header("Fire Points")]
    public Transform firePoint;   // đứng
    public Transform firePoint2;  // ngồi
    public Transform firePoint3;  // nằm
    public Transform firePoint4;  // chéo lên
    public Transform firePoint5;  // chéo xuống

    [Header("Cài đặt tốc độ bắn")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private int maxBulletsPerBurst = 5;
    [SerializeField] private float reloadTime = 1.5f;
    #endregion

    #region 🔧 Biến điều khiển
    private float nextFireTime;
    private int bulletsFired = 0;
    private bool isReloading = false;
    private Vector2 shootDirection = Vector2.right;
    private Transform selectedFirePoint;
    private PlayerMovement playerMovement;
    private HealthSystem healthSystem;

    private bool isShootingDiagonal = false; // ✅ khoá nhảy khi bắn chéo
    #endregion

    #region 💥 Trạng thái vũ khí
    public enum WeaponType { Normal, M, L, S }
    [Header("Trạng thái vũ khí hiện tại")]
    public WeaponType currentWeapon = WeaponType.Normal;
    #endregion

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        healthSystem = GetComponent<HealthSystem>();
        selectedFirePoint = firePoint;
        bulletPrefab = normalBullet;
    }

    private void Update()
    {
        if (healthSystem != null && healthSystem.isDead)
        {
            ResetWeapon();
            return;
        }

        if (isReloading) return;

        UpdateShootingDirection();
        AutoShoot();
    }

    #region 🎯 Cập nhật hướng bắn
    private void UpdateShootingDirection()
    {
        float moveInput = 0f;
        Animator animator = playerMovement.GetAnimator();
        isShootingDiagonal = false;

        if (Input.GetKey(KeyCode.A)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D)) moveInput = 1f;

        animator.SetBool("IdleShoot_Up", false);
        animator.SetBool("Shoot_Up", false);
        animator.SetBool("IdleShoot_Down", false);
        animator.SetBool("Shoot_Down", false);

        // 🔸 Nếu đang ngồi hoặc nằm → chỉ bắn ngang
        if (playerMovement.GetIsCrouching() || playerMovement.GetIsProne())
        {
            selectedFirePoint = playerMovement.GetIsCrouching() ? firePoint2 : firePoint3;
            shootDirection = Vector2.right * playerMovement.GetFacingDirection();
            playerMovement.BlockJumpInput(false);
            return;
        }

        // 🔺 Bắn chéo lên
        if (Input.GetKey(KeyCode.O))
        {
            float facing = playerMovement.GetRealFacingDirection();
            shootDirection = new Vector2(facing, 0.9f).normalized;
            selectedFirePoint = firePoint4;
            animator.SetBool(moveInput != 0 ? "Shoot_Up" : "IdleShoot_Up", true);
            isShootingDiagonal = true;
        }
        // 🔻 Bắn chéo xuống
        else if (Input.GetKey(KeyCode.I))
        {
            float facing = playerMovement.GetRealFacingDirection();
            shootDirection = new Vector2(facing, -0.9f).normalized;
            selectedFirePoint = firePoint5;
            animator.SetBool(moveInput != 0 ? "Shoot_Down" : "IdleShoot_Down", true);
            isShootingDiagonal = true;
        }
        // 🔹 Bắn ngang
        else
        {
            selectedFirePoint = firePoint;
            shootDirection = Vector2.right * playerMovement.GetFacingDirection();
        }

        // ✅ Nếu đang bắn chéo → khoá nhảy
        playerMovement.BlockJumpInput(isShootingDiagonal);
    }
    #endregion

    #region 🔄 Cơ chế bắn tự động
    private void AutoShoot()
    {
        if (Time.time < nextFireTime) return;
        if (isReloading) return;

        if (bulletsFired >= maxBulletsPerBurst)
        {
            StartCoroutine(Reload());
            return;
        }

        // 🔥 Nếu là Spread Gun → bắn 3 tia
        if (currentWeapon == WeaponType.S)
            ShootSpread3(selectedFirePoint, shootDirection);
        else
            ShootSingle(shootDirection, selectedFirePoint);

        bulletsFired++;
        nextFireTime = Time.time + fireRate;

        if (bulletsFired >= maxBulletsPerBurst)
            StartCoroutine(Reload());
    }

    private void ShootSingle(Vector2 direction, Transform spawnPoint)
    {
        if (bulletPrefab == null || spawnPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
        bullet.GetComponent<PlayerBullet>().SetDirection(direction);
    }

    /// <summary>
    /// Đạn S bắn ra 3 tia (trên, giữa, dưới) theo hướng hiện tại.
    /// </summary>
    private void ShootSpread3(Transform spawnPoint, Vector2 baseDir)
    {
        if (BulletS == null || spawnPoint == null) return;

        // Góc lệch nhỏ quanh hướng chính (tỏa nhẹ)
        const float angleOffset = 10f; // độ lệch mỗi tia
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        float[] angles = new float[]
        {
            baseAngle + angleOffset,
            baseAngle,
            baseAngle - angleOffset
        };

        foreach (float angle in angles)
        {
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject bullet = Instantiate(BulletS, spawnPoint.position, Quaternion.identity);
            bullet.GetComponent<PlayerBullet>().SetDirection(dir.normalized);
        }

        Debug.Log("💥 Spread Gun: 3 tia!");
    }
    #endregion

    #region ⚙️ Nâng cấp & Reset vũ khí
    public void UpgradeWeaponPermanent(string weaponTag = "M")
    {
        switch (weaponTag)
        {
            case "M":
                currentWeapon = WeaponType.M;
                bulletPrefab = BulletM;
                fireRate = 0.1f;
                break;

            case "L":
                currentWeapon = WeaponType.L;
                bulletPrefab = BulletL;
                fireRate = 0.25f;
                break;

            case "S":
                currentWeapon = WeaponType.S;
                bulletPrefab = BulletS;
                fireRate = 0.35f; // Spread 3 tia nên hơi chậm lại
                break;

            default:
                currentWeapon = WeaponType.Normal;
                bulletPrefab = normalBullet;
                fireRate = 0.2f;
                break;
        }

        Debug.Log($"🔥 Vũ khí hiện tại: {currentWeapon}, Prefab = {bulletPrefab?.name}");
    }

    public void ResetWeapon()
    {
        if (currentWeapon != WeaponType.Normal)
        {
            currentWeapon = WeaponType.Normal;
            bulletPrefab = normalBullet;
            fireRate = 0.2f;
            Debug.Log("💀 Player chết → trở lại đạn thường!");
        }
    }

    // 🔁 Cơ chế nạp đạn
    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        bulletsFired = 0;
        isReloading = false;
    }
    #endregion
}
