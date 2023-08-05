using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public WorldGenerator worldGenerator;
    public FogOfWarController fogOfWarController;

    void Start()
    {
        // Call for World Generation
        worldGenerator.GenerateWorld();

        // Call for FogOfWar Generation
        fogOfWarController.SetupOverlay(worldGenerator.mainMap);

        // Call for Spawning the Player
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPosition = worldGenerator.GetRandomPosition();
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}