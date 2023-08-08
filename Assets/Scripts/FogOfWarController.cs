using UnityEngine;
using UnityEngine.Tilemaps;
using static TileType;

public class FogOfWarController : MonoBehaviour
{
    public static FogOfWarController Instance { get; private set; }
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public float overlayHeight = 1f; // new variable
    private Component _playerController;

    
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
    private void Start()
    {
        // find object with tag "player"
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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


    public void ClearTile(Vector3 worldPosition)
    {
        var tilePosition = overlayTilemap.WorldToCell(worldPosition);
        overlayTilemap.SetTile(tilePosition, null);
    }

    public TileBase[,] GetObservedArea()
    {
        // The observed area radius is the x or y distance from the farthest observable tile to the player. Get radius.
        var bounds = overlayTilemap.cellBounds;
        var radius = Mathf.Max(bounds.size.x, bounds.size.y) / 2;
        
        var playerTilePos = overlayTilemap.WorldToCell(_playerController.transform.position);

        var observedTiles = new TileBase[radius * 2 - 1, radius * 2 - 1];
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var checkPos = new Vector3Int(playerTilePos.x + x, playerTilePos.y + y, playerTilePos.z);
                var distance = Vector3.Distance(_playerController.transform.position, overlayTilemap.GetCellCenterWorld(checkPos));
                if (distance <= radius)
                {
                    var tile = WorldGenerator.Instance.mainMap.GetTile(checkPos);
                    observedTiles[x + radius, y + radius] = tile;
                }
                else
                {
                    observedTiles[x + radius, y + radius] = darkTile;
                }
            }
        }

        return observedTiles;
    }
}