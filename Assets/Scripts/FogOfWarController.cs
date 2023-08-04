using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarController : MonoBehaviour
{
    public static FogOfWarController Instance { get; private set; }
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public float overlayHeight = 1f; // new variable


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetupOverlay(Tilemap mainMap)
    {
        // Ensure overlay is at origin
        overlayTilemap.transform.localPosition = new Vector3(0, 0, overlayHeight);

        BoundsInt bounds = mainMap.cellBounds;
        TileBase[] allTiles = mainMap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int pos = new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0);
                if (allTiles[x + y * bounds.size.x] != null)
                {
                    overlayTilemap.SetTile(pos, null);
                }
                else
                {
                    overlayTilemap.SetTile(pos, darkTile);
                }
            }
        }
    }


    public void ClearTile(Vector3 worldPosition)
    {
        Vector3Int tilePosition = overlayTilemap.WorldToCell(worldPosition);
        overlayTilemap.SetTile(tilePosition, null);
    }
}