using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện hiển thị mạng sống (UI tim) của người chơi.
/// Script này KHÔNG điều khiển vị trí hoặc hồi sinh nhân vật.
/// </summary>
public class PlayerLives : MonoBehaviour
{
    [Header("Cài đặt UI mạng sống")]
    [Tooltip("Tổng số mạng ban đầu (phải trùng với HealthSystem.maxLives).")]
    public int maxLives = 3;

    [Tooltip("Danh sách hình trái tim (UI Image) hiển thị mạng.")]
    public Image[] lifeImages;

    private int currentLives;

    /// <summary>
    /// Khởi tạo UI mạng sống.
    /// </summary>
    void Start()
    {
        currentLives = maxLives;

        if (lifeImages == null || lifeImages.Length == 0)
        {
            Debug.LogError("⚠️ lifeImages chưa được gán trên " + gameObject.name);
            return;
        }

        // Bật tất cả trái tim lúc đầu
        foreach (Image heart in lifeImages)
            heart.enabled = true;
    }

    /// <summary>
    /// Cập nhật số lượng mạng còn lại (được gọi từ HealthSystem).
    /// </summary>
    public void UpdateLivesUI(int livesRemaining)
    {
        currentLives = Mathf.Clamp(livesRemaining, 0, maxLives);

        // Ẩn hoặc hiện tim theo số mạng còn lại
        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].enabled = i < currentLives;
        }

        Debug.Log($"❤️ Cập nhật UI: còn {currentLives}/{maxLives} mạng.");
    }

    /// <summary>
    /// Reset toàn bộ UI khi hồi sinh lại (ví dụ chơi lại từ đầu).
    /// </summary>
    public void ResetLivesUI()
    {
        currentLives = maxLives;
        foreach (Image heart in lifeImages)
            heart.enabled = true;
    }
}
