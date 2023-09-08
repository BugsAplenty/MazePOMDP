using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    private static GameManager Instance { get; set; }

    public GameObject player;
    public int[,] BeliefMap;
    public event Action PlayerSpawned;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BeliefMap = new int[WorldGenerator.Instance.width, WorldGenerator.Instance.height];
        WorldGenerator.Instance.GenerateWorld();
        FogOfWarController.Instance.SetupOverlay(WorldGenerator.Instance.mainMap);
        SpawnPlayer(); 
        RandomizePlayerLocation();
        PlayerSpawned?.Invoke();
    }

    private void SpawnPlayer()
    {
        var spawnCell = Vector3Int.FloorToInt(WorldGenerator.Instance.GetRandomPosition());
        var spawnPosition = WorldGenerator.Instance.mainMap.GetCellCenterWorld(spawnCell);
        Instantiate(player, spawnPosition, Quaternion.identity);
    }

    private void RandomizePlayerLocation()
    {
        player.transform.position = WorldGenerator.Instance.GetRandomPosition();
    }
}