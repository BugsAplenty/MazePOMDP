using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [SerializeField] RenderTexture worldMap;
    [SerializeField] RenderTexture beliefMap;
    private RawImage rawImage;

    void Start()
    {
        SetMinimap();
    }

    private void SetMinimap()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.texture = worldMap;

        // Add the belief map as an overlay
        GameObject overlay = new GameObject("BeliefOverlay");
        overlay.transform.SetParent(transform, false);  // The second argument 'false' ensures local position/scale is retained.

        RawImage beliefImage = overlay.AddComponent<RawImage>();
        beliefImage.texture = beliefMap;

        RectTransform beliefRectTransform = beliefImage.rectTransform;

        // Set overlay size to match the parent minimap size
        beliefRectTransform.sizeDelta = rawImage.rectTransform.sizeDelta;

        // Ensure overlay covers the entire parent minimap area
        beliefRectTransform.anchorMin = Vector2.zero;
        beliefRectTransform.anchorMax = Vector2.one;
        beliefRectTransform.offsetMin = Vector2.zero;
        beliefRectTransform.offsetMax = Vector2.zero;

        // Optional: If pivots might differ, ensure they match. By default, it should be centered (0.5, 0.5).
        beliefRectTransform.pivot = rawImage.rectTransform.pivot;
    }

}
