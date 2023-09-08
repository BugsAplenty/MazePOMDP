using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarController : MonoBehaviour
{
    public static FogOfWarController Instance { get; private set; }
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public Tile lightTile;
    public float overlayHeight = 1f; // new variable
    public Component playerController;

    
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
                if (!IsBlocked(playerTilePos, checkPos) && !IsOutsideBounds(checkPos))
                {
                    overlayTilemap.SetTile(checkPos, lightTile);
                }
            }
        }
    }

    private static bool IsOutsideBounds(Vector3Int checkPos)
    {
        //Create bounds based on the world map
        var bounds = WorldGenerator.Instance.tilemap.cellBounds;
        //Check if the checkPos is within the bounds
        return !bounds.Contains(checkPos);
    }

    private static bool IsBlocked(Vector3Int start, Vector3Int end)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = (dx > dy ? dx : -dy) / 2;
        int e2;

        bool wallEncountered = false;  // flag to check if a wall tile has been encountered

        while (true)
        {
            Vector3Int checkPos = new Vector3Int(x0, y0, 0);
        
            if (WorldGenerator.Instance.TileIsWall(checkPos))
            {
                if(wallEncountered)  // if we've already encountered a wall, then exit after illuminating this tile
                    return true;
                wallEncountered = true;
            }
            else
            {
                wallEncountered = false;  // reset if it's not a wall
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            e2 = err;

            if (e2 > -dx)
            {
                err -= dy;
                x0 += sx;
            }
        
            if (e2 < dy)
            {
                err += dx;
                y0 += sy;
            }
        }

        return false;
    }


    private void DrawDebugRectangle(BoundsInt bounds, Color color)
    {
        Vector3 topLeft = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMax, 0));
        Vector3 topRight = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        Vector3 bottomLeft = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 bottomRight = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMin, 0));

        Debug.DrawLine(topLeft, topRight, color, 5f, false);
        Debug.DrawLine(topRight, bottomRight, color, 5f, false);
        Debug.DrawLine(bottomRight, bottomLeft, color, 5f, false);
        Debug.DrawLine(bottomLeft, topLeft, color, 5f, false);
    }

    private BoundsInt GetObservedBounds()
    {
        var bounds = overlayTilemap.cellBounds;
        var playerTilePos = overlayTilemap.WorldToCell(playerController.transform.position);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        int lightTileCount = 0;

        for (var x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var checkPos = new Vector3Int(x, y, playerTilePos.z);
                var tileBase = overlayTilemap.GetTile(checkPos);

                if (tileBase == lightTile)
                {
                    lightTileCount++;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
                else if (tileBase == darkTile) continue;
            }
        }
        // Force the bounds to be no more than the size of the map
        minX = Math.Max(minX, bounds.xMin);
        minY = Math.Max(minY, bounds.yMin);
        maxX = Math.Min(maxX, bounds.xMax);
        maxY = Math.Min(maxY, bounds.yMax);
        // Display bounds of observed area
        // Debug.Log("Observed Area Bounds: " + minX + ", " + minY + ", " + maxX + ", " + maxY);
        return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
    }

    public TileBase[,] GetObservedArea()
    {
        var bounds = GetObservedBounds();
        DrawDebugRectangle(bounds, Color.yellow);

        // Create a 2D array of TileBase
        var tileArray = new TileBase[bounds.size.x + 1, bounds.size.y + 1];

        // Copy the tiles from the revealed area of the overlayTilemap to the tileArray
        for (var x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = overlayTilemap.GetTile(pos);
                // Index tileArray at relative position to the bounds
                tileArray[x - bounds.xMin, y - bounds.yMin] = tile;
            }
        }

        return tileArray;
    }

}

    