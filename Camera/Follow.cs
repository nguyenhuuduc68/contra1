using UnityEngine;



public class Follow : MonoBehaviour
{
    public Transform Player;
    void Update()
    {
        transform.position = new Vector3(Player.position.x + 5f, 0, -10); // Follow ngang, offset để player ở bên trái
    }
}
