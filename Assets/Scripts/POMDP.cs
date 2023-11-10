using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pomdp : Singleton<Pomdp>
{
    private Texture2D _beliefTexture;

    private void Start()
    {
        _beliefTexture = new Texture2D(WorldGenerator.Instance.width, WorldGenerator.Instance.height);
        WorldMapController.Instance.rawImage.texture = _beliefTexture;
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
        StartCoroutine(UpdateBeliefMap());
    }


    private static IEnumerator UpdateBeliefMap()
    {

        for (var mapX = 0; mapX < WorldGenerator.Instance.width; mapX++)
        {
            for (var mapY = 0; mapY < WorldGenerator.Instance.height; mapY++)
            {
                // Get cell type from the world map
                var cellType = WorldGenerator.Instance.Map[mapY, mapX];
                // If the cell is a wall, skip it
                if (cellType == WorldGenerator.Instance.wallTile) {
                    WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(mapX, mapY, 0), Color.black);
                    continue;
                }
                if (cellType != WorldGenerator.Instance.floorTile) continue;
                // Convert the array indices to world coordinates
                var worldPos = new Vector3Int(mapX - WorldGenerator.Instance.width / 2, mapY - WorldGenerator.Instance.height / 2, 0);
                // Get a TileBase Array of the observed area if the player were in this cell
                var observedAreaHypothesis = FogOfWarController.Instance.GetObservedAreaAround(worldPos);
                // Get actual observed area
                var actualObservedArea = FogOfWarController.Instance.GetPlayerObservedArea();
                // Get the observed area around the actual player
                // Compare the two arrays and get a score
                var score = MatchScore(observedAreaHypothesis, actualObservedArea);
                // Create a color based on a lerp between red and green based on the score
                var color = Color.Lerp(Color.red, Color.green, score / (observedAreaHypothesis.GetLength(0) * observedAreaHypothesis.GetLength(1)));
                // Update the belief map texture
                WorldMapController.Instance.UpdateWorldMapTexture(new Vector3Int(mapX, mapY, 0), color);
            }
            yield return null;
        }
    }

    private static float MatchScore(TileBase[,] observedAreaHypothesis, TileBase[,] actualObservedArea)
    {
        // Return a score based on the number of non-dark tiles that match between the two arrays
        var score = 0;
        for (var x = 0; x < observedAreaHypothesis.GetLength(0); x++)
        {
            for (var y = 0; y < observedAreaHypothesis.GetLength(1); y++)
            {
                // If a hypothesis tile is null, the match score is automatically 0
                if (observedAreaHypothesis[x, y] == null)
                {
                    return 0;
                }
                if (observedAreaHypothesis[x, y] != actualObservedArea[x, y])
                {
                    if (observedAreaHypothesis[x, y] == FogOfWarController.Instance.darkTile || actualObservedArea[x, y] == FogOfWarController.Instance.darkTile)
                    {
                        // If either tile is dark, the match score is unaffected
                        continue;
                    }
                    // If the tiles don't match and neither is dark, the match score is 0
                    return 0;
                }
                score++;
            }
        }
        return score;
    }
}
