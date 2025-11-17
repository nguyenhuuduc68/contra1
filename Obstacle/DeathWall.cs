using UnityEngine;

public class DeathWall : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

            // ✅ Chỉ gọi hệ thống máu của Player thôi
            HealthSystem health = collision.GetComponent<HealthSystem>();
            if (health != null)
                health.TakeDamageFromWall();
        }
    }
}
