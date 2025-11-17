using UnityEngine;

/// <summary>
/// Hệ thống sinh Enemy tự động khi Player tiến đến gần vị trí spawn.  
/// Mỗi vị trí spawn chỉ kích hoạt một lần.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// Cấu trúc dữ liệu cho mỗi điểm spawn (bao gồm vị trí và prefab Enemy).
    /// </summary>
    [System.Serializable]
    public struct SpawnPoint
    {
        /// <summary>Vị trí spawn của Enemy (tọa độ X, Y).</summary>
        public Vector2 position;

        /// <summary>Prefab Enemy (ví dụ: RunningEnemy).</summary>
        public GameObject enemyPrefab;
    }

    /// <summary>Danh sách các điểm spawn trong Scene.</summary>
    public SpawnPoint[] spawnPoints;

    /// <summary>Bán kính kiểm tra xung quanh Player để kích hoạt spawn.</summary>
    public float spawnRadius = 10f;

    /// <summary>Transform của Player (tìm theo Tag "Player").</summary>
    private Transform player;

    /// <summary>Mảng kiểm tra xem từng điểm spawn đã được kích hoạt hay chưa.</summary>
    private bool[] hasSpawned;

    /// <summary>
    /// Hàm khởi tạo.  
    /// Tìm Player trong Scene, khởi tạo mảng kiểm tra spawn.
    /// </summary>
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("⚠️ Không tìm thấy Player (Tag 'Player')!");

        // Khởi tạo mảng đánh dấu trạng thái spawn
        hasSpawned = new bool[spawnPoints.Length];
        for (int i = 0; i < hasSpawned.Length; i++)
        {
            hasSpawned[i] = false;
        }
    }

    /// <summary>
    /// Kiểm tra khoảng cách giữa Player và từng điểm spawn trong mỗi frame.  
    /// Nếu Player nằm trong bán kính spawn, Enemy sẽ được tạo ra một lần duy nhất.
    /// </summary>
    void Update()
    {
        if (player == null) return;

        float playerX = player.position.x;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Tính khoảng cách từ Player đến vị trí spawn
            float distanceToSpawn = Vector2.Distance(player.position, spawnPoints[i].position);

            //            Debug.Log($"SpawnPoint {i}: Distance to Player = {distanceToSpawn}, Radius = {spawnRadius}");

            // Nếu chưa spawn và Player đủ gần thì sinh Enemy
            if (!hasSpawned[i] && distanceToSpawn <= spawnRadius)
            {
                Instantiate(spawnPoints[i].enemyPrefab, spawnPoints[i].position, Quaternion.identity);
                Debug.Log($"✅ Spawned RunningEnemy tại vị trí: {spawnPoints[i].position}");
                hasSpawned[i] = true;
            }
        }
    }
}
