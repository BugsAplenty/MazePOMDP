using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject player;
    
    private void Start()
    {
        /*
         * This entire section needs to become its own function that will be called on when a button is pressed
         * On top of that, first there needs to be a UI element with text and check said inputs for correctness
         * meaning, the generation of the world must become an event
         * on top of that, an additional function must be built, a world destroyer.
         * and consider when is it triggered. On restart is obvious, anywhere else?
         */
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

    public void Restart()
    {
        // Destroy the player
        Destroy(player);
        // Destroy the world
        WorldGenerator.Instance.DestroyWorld();
        // Generate a new world
        WorldGenerator.Instance.GenerateWorld();
        // Reset the world map
        WorldMapController.Instance.ResetWorldMap();
        // Setup the fog of war overlay
        FogOfWarController.Instance.SetupOverlay(WorldGenerator.Instance.mainMap);
        // Spawn the player
        SpawnPlayer();
        // Randomize the player location
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