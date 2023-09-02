using System;
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
        PlayerController.Instance.PlayerMoved += PlayerController_PlayerMoved;
    }

    private void PlayerController_PlayerMoved(object sender, EventArgs e)
    {
        UpdateBeliefMap();
    }

    private void UpdateBeliefMap()
    {
        var observedArea = FogOfWarController.Instance.GetObservedArea();
        var observedAreaWidth = observedArea.GetLength(1);
        var observedAreaHeight = observedArea.GetLength(0);

        // Assuming the observed area is always centered around the player
        var centerX = WorldGenerator.Instance.width / 2;
        var centerY = WorldGenerator.Instance.height / 2;

        var startX = centerX - observedAreaWidth / 2;
        var startY = centerY - observedAreaHeight / 2;

        for (var j = 0; j < observedAreaHeight; j++)
        {
            for (var i = 0; i < observedAreaWidth; i++)
            {
                var x = startX + i;
                var y = startY + j;

                var observedTile = observedArea[j, i];
                var mapTile = WorldGenerator.Instance.Map[y, x];

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

                if (!shouldUpdateTexture) continue;
                var worldPos = new Vector3(x, y, 0);
                var color = BeliefMap[y, x] >= MaxBelief / 2 ? Color.green : Color.red;
                worldMapController.UpdateWorldMapTexture(worldPos, color);
            }
        }
    }

}
