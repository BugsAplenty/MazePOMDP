using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapController : Singleton<WorldMapController>
{
    [SerializeField] private Camera worldCamera;
    public int textureWidth;
    public int textureHeight;
    public RawImage rawImage;
    private Texture2D _texture2D;
    private RenderTexture _renderTexture;

    
    private void Start()
    {
        textureWidth = WorldGenerator.Instance.width;
        textureHeight = WorldGenerator.Instance.height;
        StartCoroutine(SetWorldMap());
        _texture2D = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point
        };
    }

    private IEnumerator SetWorldMap()
    {
        rawImage = GetComponent<RawImage>();
        _renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        worldCamera.targetTexture = _renderTexture;
        rawImage.texture = _renderTexture;


        // Wait until the RenderTexture is ready
        yield return new WaitUntil(() => _renderTexture.IsCreated());

        // Modify the colors in the RenderTexture
        var modifiedTexture = ModifyRenderTexture(_renderTexture);

        // Use the modified RenderTexture
        rawImage.texture = modifiedTexture;
        
        // Set raw image filter mode to point
        rawImage.texture.filterMode = FilterMode.Point;
        // Release the RenderTexture as it is no longer needed
        RenderTexture.ReleaseTemporary(_renderTexture);
    }

    private static Texture2D ModifyRenderTexture(RenderTexture source)
    {
        var width = source.width;
        var height = source.height;

        // Step 1: Create a temporary Texture2D
        var tempTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Step 2: Copy the pixel data
        RenderTexture.active = source;
        tempTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;

        // Step 3: Modify the pixel data
        var pixels = tempTexture.GetPixels();
        for(var i = 0; i < pixels.Length; i++)
        {
            if(pixels[i].r < 0.5f) // Assuming dark gray is the wall
            {
                pixels[i] = Color.black;  // Wall
            }
            else 
            {
                pixels[i] = Color.white;  // Path
            }
        }
        tempTexture.SetPixels(pixels);
        tempTexture.Apply();

        return tempTexture;
    }

    public void UpdateWorldMapTexture(Vector3 worldPos, Color color)
    {
        var x = Mathf.FloorToInt(worldPos.x);
        var y = Mathf.FloorToInt(worldPos.y);

        // Directly update the _texture2D pixel
        _texture2D.SetPixel(x, y, color);
        _texture2D.Apply();

        // Set the updated texture to rawImage
        rawImage.texture = _texture2D;
    }

    public static void PaintWallTilesBlack()
    {
        for (var y = 0; y < WorldGenerator.Instance.height; y++)
        {
            for (var x = 0; x < WorldGenerator.Instance.width; x++)
            {
                if (WorldGenerator.Instance.Map[y, x] == WorldGenerator.Instance.wallTile)
                {
                    Instance.UpdateWorldMapTexture(new Vector3(x, y, 0), Color.black);
                }
            }
        }
    }

    public static Vector3Int WorldToMapPosition(Vector3Int worldPos)
    {
        var mapPos = new Vector3Int(worldPos.x + WorldGenerator.Instance.width / 2, worldPos.y + WorldGenerator.Instance.height / 2, 0);
        return mapPos;
    }
    public static Vector3Int GetPlayerMapPosition()
    // Converts the player's world position to the map position
    {
        return WorldToMapPosition(PlayerController.Instance.currentCellPosition);
    }
    // A function that returns the player's position relative to the world map
    public static void VisualizePlayerPosition()
    {
        var playerPos = GetPlayerMapPosition();
        Instance.UpdateWorldMapTexture(playerPos, Color.blue);
    }
}
