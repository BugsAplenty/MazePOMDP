using System;
using System.Collections;
using UnityEngine;

public class Pomdp : MonoBehaviour
{
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
        var compositeObservedArea = FogOfWarController.GetCompositeObservedArea();
        var observedAreaWidth = compositeObservedArea.GetLength(1);
        var observedAreaHeight = compositeObservedArea.GetLength(0);
        var playerRelPos = FogOfWarController.Instance.GetPlayerRelativePosition();

        for (var mapY = 0; mapY < WorldGenerator.Instance.height; mapY++)
        {
            for (var mapX = 0; mapX < WorldGenerator.Instance.width; mapX++)
            {
                int startX = mapX - playerRelPos.x;
                int startY = mapY - playerRelPos.y;
                int endX = startX + observedAreaWidth - 1;
                int endY = startY + observedAreaHeight - 1;

                int matchCount = 0;
                int totalRevealedTileCount = 0;

                for (int y = startY, j = 0; y <= endY && j < observedAreaHeight; y++, j++)
                {
                    for (int x = startX, i = 0; x <= endX && i < observedAreaWidth; x++, i++)
                    {
                        if (x < 0 || x >= WorldGenerator.Instance.width || y < 0 || y >= WorldGenerator.Instance.height) continue;

                        var observedTile = compositeObservedArea[j, i];
                        var mapTile = WorldGenerator.Instance.Map[y, x];

                        if (observedTile == FogOfWarController.Instance.darkTile || observedTile == null) continue;
                        totalRevealedTileCount++;

                        if (observedTile == mapTile)
                        {
                            matchCount++;
                        }
                    }
                }

                float matchRatio = totalRevealedTileCount == 0 ? 0 : (float)matchCount / totalRevealedTileCount;

                Color paintColor = Color.Lerp(Color.red, Color.green, matchRatio);

                WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(mapX, mapY, 0), paintColor);
            }
        }

        WorldMapController.PaintWallTilesBlack();
    }
}
