using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [SerializeField] RenderTexture worldMap;
    private RawImage rawImage;

    // Start is called before the first frame update
    void Start()
    {
        SetMinimap();
    }

    private void SetMinimap()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.texture = worldMap;
    }

}
