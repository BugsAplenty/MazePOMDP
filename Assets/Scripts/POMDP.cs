using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Pomdp : MonoBehaviour
{
    private const int MaxBelief = 100; // The maximum value for a belief score
    public GameManager gameManager; // Reference to the GameManager
    public RawImage beliefImage; // Reference to a UI RawImage to display the belief map
    private Texture2D _beliefTexture; // Texture to represent the belief map

    private void Start()
    {
        // Create a new Texture2D with the same dimensions as the map
        _beliefTexture = new Texture2D(WorldGenerator.Instance.width, WorldGenerator.Instance.height);
        beliefImage.texture = _beliefTexture;
    }

    private void Update()
    {
        UpdateBeliefMap();
        UpdateBeliefTexture();
    }

    private void UpdateBeliefMap()
    {
        // Get radius of the player's observed area
        var observedArea = FogOfWarController.Instance.GetObservedArea();
        // Dimensions of the player's observed area
        var observedAreaWidth = observedArea.GetLength(1);
        var observedAreaHeight = observedArea.GetLength(0);

        // Loop through all possible patches in the entire map
        for (var y = 0; y <= WorldGenerator.Instance.height - observedAreaHeight; y++)
        {
            for (var x = 0; x <= WorldGenerator.Instance.width - observedAreaWidth; x++)
            {
                // Compare the player's observed area with the current patch in the map
                for (var j = 0; j < observedAreaHeight; j++)
                {
                    for (var i = 0; i < observedAreaWidth; i++)
                    {
                        // Convert TileBase to TileType for comparison
                        var observedTileType = WorldGenerator.Instance.GetTileType(observedArea[j, i]);
                        var mapTileType = WorldGenerator.Instance.Map[y + j, x + i];

                        // Increase the belief score if the tiles match and are not darkness
                        if (observedTileType == mapTileType && observedTileType != TileType.Darkness)
                        {
                            gameManager.beliefMap[y, x] += 1;
                        }
                        // If the tiles do not match and neither of them is darkness, decrease the belief score
                        else if (observedTileType != mapTileType && observedTileType != TileType.Darkness && mapTileType != TileType.Darkness)
                        {
                            gameManager.beliefMap[y, x] -= 1;
                        }
                    }
                }
            }
        }
    }

    void UpdateBeliefTexture()
    {
        for (var y = 0; y < WorldGenerator.Instance.height; y++)
        {
            for (var x = 0; x < WorldGenerator.Instance.width; x++)
            {
                // Clamp the belief score between 0 and MaxBelief
                int clampedScore = Mathf.Clamp(gameManager.beliefMap[y, x], 0, MaxBelief);
                // Normalize the clamped score to be between 0 and 1, and assign it to the red channel of the color
                Color color = new Color(clampedScore / (float)MaxBelief, 0, 0);
                _beliefTexture.SetPixel(x, y, color);
            }
        }
        _beliefTexture.Apply();
    }

}
