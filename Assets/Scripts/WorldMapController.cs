using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 256;
    public RawImage rawImage;
    private Texture2D _texture2D;

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
        StartCoroutine(SetWorldMap());
        _texture2D = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
    }

    private IEnumerator SetWorldMap()
    {
        rawImage = GetComponent<RawImage>();
        var renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        worldCamera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        // Wait until the RenderTexture is ready
        yield return new WaitUntil(() => renderTexture.IsCreated());

        // Modify the colors in the RenderTexture
        var modifiedTexture = ModifyRenderTexture(renderTexture);

        // Use the modified RenderTexture
        rawImage.texture = modifiedTexture;
    }

    private static RenderTexture ModifyRenderTexture(RenderTexture source)
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

        // Step 4: Create a new RenderTexture and copy the modified pixel data
        var result = new RenderTexture(width, height, 24);
        Graphics.Blit(tempTexture, result);

        return result;
    }

    public void UpdateWorldMapTexture(Vector3 worldPos, Color color)
    {
        var x = Mathf.FloorToInt(worldPos.x);
        var y = Mathf.FloorToInt(worldPos.y);

        _texture2D.SetPixel(x, y, color);
        _texture2D.Apply();
    }
}
