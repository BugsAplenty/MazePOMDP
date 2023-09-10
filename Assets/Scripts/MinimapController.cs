using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class MinimapController : MonoBehaviour
{

    [SerializeField] Tilemap WorldTileMap;
    //[SerializeField] Camera MinimapCamera;
    [SerializeField] Transform playerPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int playerPositionVector = new Vector3Int((int)playerPosition.position.x, (int)playerPosition.position.y, (int)playerPosition.position.z);
        SurroundCheck(playerPositionVector);
    }

    private void SurroundCheck(Vector3Int playerPositionVector)
    {
        Vector3Int eastCheck = new Vector3Int(playerPositionVector.x + 2, playerPositionVector.y, playerPositionVector.z);
        Vector3Int westCheck = new Vector3Int(playerPositionVector.x - 2, playerPositionVector.y, playerPositionVector.z);
        Vector3Int northCheck = new Vector3Int(playerPositionVector.x, playerPositionVector.y + 2, playerPositionVector.z);
        Vector3Int southCheck = new Vector3Int(playerPositionVector.x, playerPositionVector.y - 2, playerPositionVector.z);

        WorldTileMap.GetTileFlags(eastCheck);
        WorldTileMap.GetTile(westCheck);
        WorldTileMap.GetTile(northCheck);
        WorldTileMap.GetTile(southCheck);

        //WorldTileMap.GetTileFlags

        Debug.Log("North: " + northCheck + " West: " + westCheck + " East: " + eastCheck + " South: " + southCheck);
    }
}
