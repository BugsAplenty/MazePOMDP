using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    Camera cam;
    public LayerMask ignoreMask; // Assign this in the inspector to include both Player and Fog of War layers

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // This will ignore the layers set in the ignoreMask (e.g., player and fog of war)
        cam.cullingMask &= ~ignoreMask.value; 
    }

    void Update()
    {
        // Set the aspect ratio to 1:1 (square)
        cam.aspect = 1.0f;
    }
}
