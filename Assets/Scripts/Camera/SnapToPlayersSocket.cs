using UnityEngine;


[ExecuteAlways]
public class SnapToPlayersSocket : MonoBehaviour
{
    public Transform socket;
    void LateUpdate()
    {
        if (socket == null) return;

        transform.position = socket.position;
    }
}