using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class FogOfWarController : MonoBehaviour
{
    public static FogOfWarController Instance { get; private set; }
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public float overlayHeight = 1f; // new variable
    [FormerlySerializedAs("_playerController")] public Component playerController;

    
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
        // find object with tag "player"
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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
        var playerTilePos = overlayTilemap.WorldToCell(worldPosition);

        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var checkPos = new Vector3Int(playerTilePos.x + x, playerTilePos.y + y, playerTilePos.z);
                if (!(Vector3.Distance(worldPosition, overlayTilemap.GetCellCenterWorld(checkPos)) <= radius)) continue;
                if (!IsBlocked(playerTilePos, checkPos))
                {
                    overlayTilemap.SetTile(checkPos, null);
                }
            }
        }
    }

    private static bool IsBlocked(Vector3Int start, Vector3Int end)
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


    private BoundsInt GetObservedBounds()
    {
        var bounds = overlayTilemap.cellBounds;
        var playerTilePos = overlayTilemap.WorldToCell(playerController.transform.position);
        var radius = Mathf.Max(Mathf.Abs(playerTilePos.x - bounds.xMin), Mathf.Abs(playerTilePos.x - bounds.xMax)) + 1;

        var minX = bounds.xMax;
        var minY = bounds.yMax;
        var maxX = bounds.xMin;
        var maxY = bounds.yMin;

        for (var x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var checkPos = new Vector3Int(x, y, playerTilePos.z);

                if (overlayTilemap.GetTile(checkPos) != null) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
    }
    public TileBase[,] GetObservedArea()
    {
        // Get the bounds of the revealed area
        var bounds = GetObservedBounds();

        // Create a 2D array of TileBase
        var tileArray = new TileBase[bounds.size.x, bounds.size.y];

        // Copy the tiles from the revealed area of the overlayTilemap to the tileArray
        for (var x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = overlayTilemap.GetTile(pos);
                tileArray[x - bounds.xMin, y - bounds.yMin] = tile;
            }
        }
        return tileArray;
    }
}

    