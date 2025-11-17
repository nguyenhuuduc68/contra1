using UnityEngine;
using System.Collections;

/// <summary>
/// Giữ cho nhân vật Player luôn nằm trong vùng hiển thị của Camera chính (Cinemachine hoặc thường).  
/// Script này sẽ chặn không cho Player đi ra ngoài biên trái/phải của Camera.
/// </summary>
public class KeepPlayerInsideCamera : MonoBehaviour
{
    /// <summary>Camera chính trong Scene (nếu để trống sẽ tự động lấy <see cref="Camera.main"/>).</summary>
    public Camera mainCamera;

    /// <summary>Khoảng cách đệm tính từ biên trái/phải của Camera (đơn vị Unity).</summary>
    public float padding = 0.5f;

    /// <summary>Giới hạn bên trái (sau khi trừ padding).</summary>
    private float leftLimit;

    /// <summary>Giới hạn bên phải (sau khi trừ padding).</summary>
    private float rightLimit;

    /// <summary>
    /// Khởi tạo, tự động gán Camera chính nếu chưa được gán trong Inspector.
    /// </summary>
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// Gọi trong LateUpdate để đảm bảo chạy sau mọi cập nhật của nhân vật và Cinemachine.  
    /// Sử dụng Coroutine để khóa vị trí sau khi Camera đã hoàn tất di chuyển.
    /// </summary>
    void LateUpdate()
    {
        StartCoroutine(LockAfterCinemachine());
    }

    /// <summary>
    /// Chờ đến cuối khung hình để Cinemachine cập nhật xong, sau đó giới hạn vị trí Player trong vùng Camera.
    /// </summary>
    private IEnumerator LockAfterCinemachine()
    {
        // ✅ Đợi Cinemachine cập nhật vị trí camera trước khi xử lý
        yield return new WaitForEndOfFrame();

        if (mainCamera == null) yield break;

        // Tính toán vị trí biên trái/phải của camera trên trục thế giới
        Vector3 leftBoundary = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, mainCamera.nearClipPlane));
        Vector3 rightBoundary = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, mainCamera.nearClipPlane));

        // Cộng/trừ khoảng padding để tránh Player dính sát mép màn hình
        leftLimit = leftBoundary.x + padding;
        rightLimit = rightBoundary.x - padding;

        // Giữ Player trong khoảng [leftLimit, rightLimit]
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        transform.position = pos;
    }
}
