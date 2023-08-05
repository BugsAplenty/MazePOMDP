using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarController : MonoBehaviour
{
    public static FogOfWarController Instance { get; private set; }
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public float overlayHeight = 1f; // new variable


    private void Awake()
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

        var bounds = mainMap.cellBounds;
        var allTiles = mainMap.GetTilesBlock(bounds);

        for (var x = 0; x < bounds.size.x; x++)
        {
            for (var y = 0; y < bounds.size.y; y++)
            {
                var pos = new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0);
                overlayTilemap.SetTile(pos, darkTile);  // set dark tile regardless of the main map's tile at this position
            }
        }
    }

    public void ClearArea(Vector3 worldPosition, int radius)
    {
        Vector3Int playerTilePos = overlayTilemap.WorldToCell(worldPosition);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int checkPos = new Vector3Int(playerTilePos.x + x, playerTilePos.y + y, playerTilePos.z);
                if (Vector3.Distance(worldPosition, overlayTilemap.GetCellCenterWorld(checkPos)) <= radius)
                {
                    if (!IsBlocked(playerTilePos, checkPos))
                    {
                        overlayTilemap.SetTile(checkPos, null);
                    }
                }
            }
        }
    }

    private bool IsBlocked(Vector3Int start, Vector3Int end)
    {
        var rayDirection = Vector3.Normalize(end - start);
        var distance = Vector3Int.Distance(start, end);
        var currentPos = start + new Vector3Int((int)rayDirection.x, (int)rayDirection.y, (int)rayDirection.z);

        for (var i = 1; i <= distance; i++)
        {
            if (WorldGenerator.Instance.TileIsWall(currentPos))
            {
                return true;
            }
            currentPos = start + new Vector3Int((int)(i * rayDirection.x), (int)(i * rayDirection.y), (int)(i * rayDirection.z));
        }

        return false;
    }


    public void ClearTile(Vector3 worldPosition)
    {
        Vector3Int tilePosition = overlayTilemap.WorldToCell(worldPosition);
        overlayTilemap.SetTile(tilePosition, null);
    }
}