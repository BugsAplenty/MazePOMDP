using System;
using System.Collections;
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
        StartCoroutine(UpdateBeliefMap());
    }
    
    
    private static IEnumerator UpdateBeliefMap()
{
    var compositeObservedArea = FogOfWarController.GetCompositeObservedArea();
    var observedAreaWidth = compositeObservedArea.GetLength(1);
    var observedAreaHeight = compositeObservedArea.GetLength(0);
    WorldMapController.Instance.PaintWallTilesBlack();

    for (var startX = 0; startX <= WorldGenerator.Instance.width - observedAreaWidth; startX++)
    {
        for (var startY = 0; startY <= WorldGenerator.Instance.height - observedAreaHeight; startY++)
        {
            int matchCount = 0;
            int totalRevealedTileCount = 0;

            for (var j = 0; j < observedAreaHeight; j++)
            {
                for (var i = 0; i < observedAreaWidth; i++)
                {
                    var x = startX + i;
                    var y = startY + j;

                    var observedTile = compositeObservedArea[j, i];
                    var mapTile = WorldGenerator.Instance.Map[y, x];

                    // We're only interested in revealed tiles
                    if (observedTile != FogOfWarController.Instance.darkTile)
                    {
                        totalRevealedTileCount++;
                        if (observedTile == mapTile)
                        {
                            matchCount++;
                        }
                    }
                }
            }

            // Determine color to paint based on percentage of matches
            float matchPercentage = (float)matchCount / totalRevealedTileCount;
            Color paintColor = new Color(1.0f - matchPercentage, matchPercentage, 0); // Red value decreases and green value increases with match percentage

            // Set the color for the center of the subsection
            var centerX = startX + observedAreaWidth / 2;
            var centerY = startY + observedAreaHeight / 2;
            WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(centerX, centerY, 0), paintColor);

            yield return new WaitForSeconds(0.05f);  // Delay for visualization. Adjust time as needed.
        }
    }
}




}
