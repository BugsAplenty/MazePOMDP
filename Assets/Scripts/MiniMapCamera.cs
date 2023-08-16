using UnityEngine;

public class SquareCamera : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Set the aspect ratio to 1:1 (square)
        cam.aspect = 1.0f;
    }
}