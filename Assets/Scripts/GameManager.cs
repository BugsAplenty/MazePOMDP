using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // Call for World Generation
        WorldGenerator.Instance.GenerateWorld();

        // Call for FogOfWar Generation
        FogOfWarController.Instance.SetupOverlay(WorldGenerator.Instance.mainMap);

        // Call for Spawning the Player
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        var spawnPosition = WorldGenerator.Instance.GetRandomPosition();
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}