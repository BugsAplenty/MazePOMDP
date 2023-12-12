using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera playerCamera;


    // Start is called before the first frame update
    void Start()
    {
        //SetFollowCamera();
    }

    public void SetFollowCamera()
    {
        playerCamera = GetComponent<CinemachineVirtualCamera>();
        playerCamera.Follow = GameObject.FindWithTag("Player").transform;
    }
}