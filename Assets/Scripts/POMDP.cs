using UnityEngine;
using UnityEngine.Tilemaps;

public class Pomdp : MonoBehaviour
{
    private const int MaxBelief = 100; 

    public WorldMapController worldMapController;
    private Texture2D _beliefTexture;

    private static Pomdp Instance { get; set; } // Singleton pattern
    private int[,] BeliefMap { get; set; } 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            BeliefMap = new int[WorldGenerator.Instance.width, WorldGenerator.Instance.height];
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        worldMapController = WorldMapController.Instance;
        _beliefTexture = new Texture2D(WorldGenerator.Instance.width, WorldGenerator.Instance.height);
        worldMapController.rawImage.texture = _beliefTexture;
        _beliefTexture.filterMode = FilterMode.Point;
        for (var y = 0; y < _beliefTexture.height; y++)
        {
            for (var x = 0; x < _beliefTexture.width; x++)
            {
                _beliefTexture.SetPixel(x, y, Color.gray);
            }
        }
        _beliefTexture.Apply();
    }

    private void Update()
    {
        UpdateBeliefMap();
    }

    private void UpdateBeliefMap()
    {
        var observedArea = FogOfWarController.Instance.GetObservedArea();
        var observedAreaWidth = observedArea.GetLength(1);
        var observedAreaHeight = observedArea.GetLength(0);

        for (var y = 0; y <= WorldGenerator.Instance.height - observedAreaHeight; y++)
        {
            for (var x = 0; x <= WorldGenerator.Instance.width - observedAreaWidth; x++)
            {
                for (var j = 0; j < observedAreaHeight; j++)        
                {
                    for (var i = 0; i < observedAreaWidth; i++)
                    {       
                        TileBase observedTile = observedArea[j, i];
                        TileBase mapTile = WorldGenerator.Instance.Map[y + j, x + i];

                        var shouldUpdateTexture = false;

                        if (observedTile == mapTile && observedTile != FogOfWarController.Instance.darkTile)
                        {
                            BeliefMap[y, x] += 10;
                            shouldUpdateTexture = true;
                        }
                        else if (observedTile != mapTile && observedTile != FogOfWarController.Instance.darkTile && mapTile != FogOfWarController.Instance.darkTile)
                        {
                            BeliefMap[y, x] -= 10;
                            shouldUpdateTexture = true;
                        }

                        // If any change was made to the belief map, update the belief texture as well
                        if (shouldUpdateTexture)
                        {
                            var worldPos = new Vector3(x + i, y + j, 0);  // Assuming z = 0, adjust as necessary

                            // Update the world map texture based on the belief map
                            var color = BeliefMap[y, x] >= MaxBelief / 2 ? Color.green : Color.red;
                            worldMapController.UpdateWorldMapTexture(worldPos, color);
                        }
                    }
                }
            }
        }
    }
}
