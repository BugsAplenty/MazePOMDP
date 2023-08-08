using System.Collections;
using UnityEngine;

public class SetCameraTarget : MonoBehaviour
{
    private CameraFollow _cameraFollow;

    void Start()
    {
        // If CameraFollow script is attached to the same GameObject as SetCameraTarget
        _cameraFollow = GetComponent<CameraFollow>();
        StartCoroutine(WaitForPlayerSpawn());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator WaitForPlayerSpawn()
    {
        GameObject player = null;

        // Keep looking for the player until it's found
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return null; // wait for the next frame before trying again
        }

        // Once the player is found, set the CameraFollow's player reference to it
        _cameraFollow.player = player;
        
        // Set camera's position to the player's position immediately
        // You may want to adjust y and z values accordingly
        var transform1 = _cameraFollow.transform;
        var position = transform1.position;
        position = new Vector3(player.transform.position.x, 
            position.y, 
            position.z);
        transform1.position = position;
    }
}