using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform; // Assign your player's transform here.
    private bool _isplayerTransformNotNull;

    // Update is called once per frame
    private void Start()
    {
        _isplayerTransformNotNull = playerTransform != null;
    }

    void LateUpdate()
    {
        // Check if there is a player
        if (_isplayerTransformNotNull)
        {
            var transform1 = transform;
            var position = playerTransform.position;
            transform1.position = new Vector3(position.x, position.y, transform1.position.z);
        }
    }
}