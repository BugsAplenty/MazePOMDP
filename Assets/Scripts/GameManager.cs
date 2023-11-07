using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject player;
    
    private void Start()
    {
        if (WorldGenerator.Instance == null)
        {
            Debug.LogError("WorldGenerator instance is not found!");
            return;
        }

        WorldGenerator.Instance.GenerateWorld();

        if (FogOfWarController.Instance == null)
        {
            Debug.LogError("FogOfWarController instance is not found!");
            return;
        }

        FogOfWarController.Instance.SetupOverlay(WorldGenerator.Instance.mainMap);
        SpawnPlayer(); 
        RandomizePlayerLocation();
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