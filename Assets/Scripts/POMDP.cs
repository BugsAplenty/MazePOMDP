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

        for (int startX = 0; startX <= WorldGenerator.Instance.width - observedAreaWidth; startX++)
        {
            for (int startY = 0; startY <= WorldGenerator.Instance.height - observedAreaHeight; startY++)
            {
                bool isMismatched = false;

                // First, check for mismatches in the subsection
                for (var j = 0; j < observedAreaHeight && !isMismatched; j++)
                {
                    for (var i = 0; i < observedAreaWidth && !isMismatched; i++)
                    {
                        var x = startX + i;
                        var y = startY + j;

                        var observedTile = observedArea[j, i];
                        var mapTile = WorldGenerator.Instance.Map[y, x];

                        if (mapTile == WorldGenerator.Instance.wallTile) continue;

                        if (observedTile != mapTile && observedTile != FogOfWarController.Instance.darkTile)
                        {
                            isMismatched = true;
                        }
                    }
                }

                // If a mismatch was found, then update the belief and paint the subsection red
                if (isMismatched)
                {
                    for (var j = 0; j < observedAreaHeight; j++)
                    {
                        for (var i = 0; i < observedAreaWidth; i++)
                        {
                            var x = startX + i;
                            var y = startY + j;

                            var mapTile = WorldGenerator.Instance.Map[y, x];

                            if (mapTile != FogOfWarController.Instance.darkTile && mapTile != WorldGenerator.Instance.wallTile)
                            {
                                BeliefMap[y, x] -= 10;
                                WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(x, y, 0), Color.red);
                            }
                            else if (mapTile == WorldGenerator.Instance.wallTile)
                            {
                                WorldMapController.Instance.UpdateWorldMapTexture(new Vector3(x, y, 0), Color.black);
                            }
                        }
                    }
                }
            }
        }
    }
}
