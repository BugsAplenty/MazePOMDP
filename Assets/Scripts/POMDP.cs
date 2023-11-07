using System;
using System.Collections;
using UnityEngine;

public class Pomdp : Singleton<Pomdp>
{
    public WorldMapController worldMapController;
    private Texture2D _beliefTexture;

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
        var playerRelPos = WorldMapController.GetPlayerMapPosition();

        for (var mapY = 0; mapY < WorldGenerator.Instance.height; mapY++)
        {
            for (var mapX = 0; mapX < WorldGenerator.Instance.width; mapX++)
            {
                var startX = mapX - playerRelPos.x;
                var startY = mapY - playerRelPos.y;
                var endX = startX + observedAreaWidth - 1;
                var endY = startY + observedAreaHeight - 1;

                var matchCount = 0;
                var totalRevealedTileCount = 0;

                for (int y = startY, j = 0; y <= endY && j < observedAreaHeight; y++, j++)
                {
                    for (int x = startX, i = 0; x <= endX && i < observedAreaWidth; x++, i++)
                    {
                        // Check if the current tile is within the bounds of the world.
                        if (x < 0 || x >= WorldGenerator.Instance.width || y < 0 || y >= WorldGenerator.Instance.height) continue;

                        var observedTile = compositeObservedArea[j, i];
                        var mapTile = WorldGenerator.Instance.Map[y, x];

                        // Skip tiles that are unobserved or null.
                        if (observedTile == FogOfWarController.Instance.darkTile || observedTile == null) continue;

                        // Count this tile as revealed.
                        totalRevealedTileCount++;

                        // If the observed tile matches the map tile, increase the match count.
                        if (observedTile == mapTile)
                        {
                            matchCount++;
                        }
                    }
                }

                // Calculate the match ratio.
                var matchRatio = totalRevealedTileCount == 0 ? 0 : (float)matchCount / totalRevealedTileCount;

                // Interpolate the color based on the match ratio.
                var paintColor = Color.Lerp(Color.red, Color.green, matchRatio);

                // Update the belief texture with the interpolated color.
                WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(mapX, mapY, 0), paintColor);
            }
        }

        // After updating all tiles, ensure the belief texture reflects the changes.
        _beliefTexture.Apply();

        // Paint any permanent, non-changeable features such as walls.
        WorldMapController.PaintWallTilesBlack();
    }
}
