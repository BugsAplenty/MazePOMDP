using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        WorldGenerator.Instance.GenerateWorld();
        FogOfWarController.Instance.SetupOverlay(WorldGenerator.Instance.mainMap);
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        var spawnCell = Vector3Int.FloorToInt(WorldGenerator.Instance.GetRandomPosition());
        var spawnPosition = WorldGenerator.Instance.mainMap.GetCellCenterWorld(spawnCell);
        Debug.Log("Spawning player at " + spawnPosition);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

}