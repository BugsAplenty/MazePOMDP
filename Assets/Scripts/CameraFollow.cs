using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;  // reference to your player object
    private Vector3 _offset;    // offset value for the camera to position relative to the player
    public new Camera camera;
    private bool _isPlayerNotNull;

    private void Start()
    {
        _isPlayerNotNull = player != null;
        _offset = transform.position - player.transform.position;
    }

    private void Update()
    {
        // Change camera position to follow the player with -10 z value
        if (!_isPlayerNotNull) return;
        var position = player.transform.position;
        camera.transform.position = new Vector3(
            position.x + _offset.x,
            position.y + _offset.y, 
            -10
            );
    }
}