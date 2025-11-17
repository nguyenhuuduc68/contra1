using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image[] lifeImages;
    private HealthSystem playerHealth;

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();
        if (playerHealth == null) Debug.LogError("Player HealthSystem not found!");
        if (lifeImages.Length == 0) Debug.LogError("No Life Images assigned!");

        foreach (Image img in lifeImages)
        {
            if (img == null) Debug.LogError("Life Image is null!");
            img.enabled = true;
            img.CrossFadeAlpha(1f, 0f, false);
        }

        playerHealth.onTakeDamage.AddListener(UpdateLivesUI);
        UpdateLivesUI();
    }

    void UpdateLivesUI()
    {
        int currentLives = playerHealth.GetCurrentLives();
        Debug.Log("Updating UI, current lives: " + currentLives);
        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] == null) continue;
            bool shouldBeVisible = i < currentLives;
            lifeImages[i].enabled = shouldBeVisible;
            lifeImages[i].CrossFadeAlpha(shouldBeVisible ? 1f : 0f, 0.5f, false);
            Debug.Log($"LifeImage {i} enabled: {lifeImages[i].enabled}, alpha: {lifeImages[i].color.a}");
        }
    }
}