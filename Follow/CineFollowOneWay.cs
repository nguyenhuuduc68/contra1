using UnityEngine;
using Unity.Cinemachine;

public class CineFollowOneWay : MonoBehaviour
{
    private CinemachineCamera vcam;
    private Transform player;
    private float maxX;

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogError("❌ Không tìm thấy CinemachineCamera trên " + gameObject.name);
            return;
        }

        player = vcam.Follow;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            vcam.Follow = player;
        }

        if (player != null)
            maxX = player.position.x;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Khi player đi tới, cập nhật camera
        if (player.position.x > maxX)
        {
            maxX = player.position.x;
        }

        // Giữ camera chỉ tiến tới, không lùi
        Vector3 camPos = transform.position;
        camPos.x = maxX;
        transform.position = camPos;
    }
}
