using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;

    void Start()
    {
        if (gameOverUI == null) Debug.LogError("GameOverUI not assigned!");
        Debug.Log("GameOverCanvas active: " + gameOverUI.activeSelf);
        gameOverUI.SetActive(false); // Đảm bảo tắt Canvas
        HealthSystem playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();
        if (playerHealth == null) Debug.LogError("Player HealthSystem not found!");
        playerHealth.onDeath.AddListener(OnPlayerDeath);
    }

    void OnPlayerDeath()
    {
        Debug.Log("Game Over triggered!");
        gameOverUI.SetActive(true);
        Animator animator = gameOverUI.GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("FadeIn");
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}