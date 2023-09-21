using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    Camera cam;
    public LayerMask ignoreMask; // Assign this in the inspector to include both Player and Fog of War layers

    private void Start()
    {
        cam = GetComponent<Camera>();
        
        // This will ignore the layers set in the ignoreMask (e.g., player and fog of war)
        cam.cullingMask &= ~ignoreMask.value;

        AdjustCameraToMap();
    }

    private void AdjustCameraToMap()
    {
        // Calculate the bottom-left and top-right corners based on the new center information
        var bottomLeftCorner = new Vector2(-WorldGenerator.Instance.width / 2, -WorldGenerator.Instance.height / 2);
        var topRightCorner = new Vector2(WorldGenerator.Instance.width / 2, WorldGenerator.Instance.height / 2);

        var mapSize = new Vector2(
            topRightCorner.x - bottomLeftCorner.x,
            topRightCorner.y - bottomLeftCorner.y
        );

        // Center the camera to the middle of the map
        var mapCenter = new Vector2(
            (topRightCorner.x + bottomLeftCorner.x) / 2,
            (topRightCorner.y + bottomLeftCorner.y) / 2
        );

        cam.transform.position = new Vector3(mapCenter.x, mapCenter.y, cam.transform.position.z);

        // Adjust orthographic size based on the map's height. Consider the aspect ratio if width is greater.
        cam.orthographicSize = mapSize.y / 2;
        if (cam.aspect < 1.0f && mapSize.x / mapSize.y > cam.aspect)
        {
            cam.orthographicSize = mapSize.x / 2 / cam.aspect;
        }
    }
}