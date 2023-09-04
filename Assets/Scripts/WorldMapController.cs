using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    public int textureWidth;
    public int textureHeight;
    public RawImage rawImage;
    private Texture2D _texture2D;
    private RenderTexture _renderTexture;
    public static WorldMapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        rawImage.texture.height = WorldGenerator.Instance.height;
        rawImage.texture.width = WorldGenerator.Instance.width;


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
            if(pixels[i].r < 0.5f) // dark gray
            {
                pixels[i] = Color.black;
            }
            else // light gray
            {
                pixels[i] = Color.white;
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
        // Create a temporary Texture2D with the same dimensions as the texture in rawImage
        var tempTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

        // Copy the pixel data from the texture in rawImage to the temporary Texture2D
        tempTexture.SetPixels((rawImage.texture as Texture2D).GetPixels());
        tempTexture.Apply();

        // Update the pixel in the temporary Texture2D
        tempTexture.SetPixel(x, y, color);
        tempTexture.Apply();

        // Copy the pixel data from the temporary Texture2D to the texture in rawImage
        var rawImageTexture = rawImage.texture as Texture2D;
        rawImageTexture.SetPixels(tempTexture.GetPixels());
        rawImageTexture.Apply();
    }
}
