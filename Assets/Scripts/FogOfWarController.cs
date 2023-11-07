using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWarController : Singleton<FogOfWarController>
{
    public Tilemap overlayTilemap;
    public Tile darkTile;
    public Tile lightTile;
    public float overlayHeight = 1f; // new variable

    
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
        var x0 = start.x;
        var y0 = start.y;
        var x1 = end.x;
        var y1 = end.y;

        var dx = Mathf.Abs(x1 - x0);
        var dy = Mathf.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = (dx > dy ? dx : -dy) / 2;

        var wallEncountered = false;  // flag to check if a wall tile has been encountered

        while (true)
        {
            var checkPos = new Vector3Int(x0, y0, 0);
        
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

            var e2 = err;

            if (e2 > -dx)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 >= dy) continue;
            err += dx;
            y0 += sy;
        }

        return false;
    }


    private void DrawDebugRectangle(BoundsInt bounds, Color color)
    {
        var topLeft = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMax, 0));
        var topRight = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));
        var bottomLeft = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        var bottomRight = overlayTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMin, 0));

        Debug.DrawLine(topLeft, topRight, color, 5f, false);
        Debug.DrawLine(topRight, bottomRight, color, 5f, false);
        Debug.DrawLine(bottomRight, bottomLeft, color, 5f, false);
        Debug.DrawLine(bottomLeft, topLeft, color, 5f, false);
    }

    private BoundsInt GetObservedBounds()
    {
        var bounds = overlayTilemap.cellBounds;
        var playerTilePos = overlayTilemap.WorldToCell(PlayerController.Instance.currentCellPosition);

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        for (var x = bounds.xMin; x <= bounds.xMax; x++)
        {
            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                var checkPos = new Vector3Int(x, y, playerTilePos.z);
                var tileBase = overlayTilemap.GetTile(checkPos);

                if (tileBase != lightTile) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
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
    public Vector2Int GetPlayerRelativePosition()
    {
        var playerTilePos = overlayTilemap.WorldToCell(PlayerController.Instance.currentCellPosition);
        var bounds = GetObservedBounds();
        var playerRelX = (playerTilePos.x - bounds.xMin) / (float)bounds.size.x;
        var playerRelY = (playerTilePos.y - bounds.yMin) / (float)bounds.size.y;

        // Convert the relative float values into int by scaling.
        var scaledX = Mathf.RoundToInt(playerRelX * WorldGenerator.Instance.width);
        var scaledY = Mathf.RoundToInt(playerRelY * WorldGenerator.Instance.height);

        return new Vector2Int(scaledX, scaledY);
    }

    
    /// <summary>
    /// Returns a 2D array of TileBase that represents the composite of the fog of war and the main map.
    /// </summary>
    /// <returns></returns>
    public  TileBase[,] GetPlayerCompositeObservedArea()
    {
        var playerTilePos = overlayTilemap.WorldToCell(PlayerController.Instance.currentCellPosition);
        return GetCompositeObservedAreaAround(playerTilePos);
    }

    public TileBase[,] GetCompositeObservedAreaAround(Vector3Int tilePos)
    {
        var fogObservedArea = Instance.GetObservedAreaAround(tilePos);
        var width = fogObservedArea.GetLength(1);
        var height = fogObservedArea.GetLength(0);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (fogObservedArea[y, x] != Instance.darkTile)
                {
                    fogObservedArea[y, x] = WorldGenerator.Instance.Map[y, x];
                }
            }
        }
        return fogObservedArea;
    }

    public TileBase[,] GetObservedAreaAround(Vector3Int tilePos)
    {
        var bounds = GetObservedBounds();
        var boundsNew = new BoundsInt();
        // Offset the bounds by the difference between the player's position and the tilePos
        boundsNew.xMin = bounds.xMin + tilePos.x - PlayerController.Instance.currentCellPosition.x;
        boundsNew.xMax = bounds.xMax + tilePos.x - PlayerController.Instance.currentCellPosition.x;
        boundsNew.yMin = bounds.yMin + tilePos.y - PlayerController.Instance.currentCellPosition.y;
        boundsNew.yMax = bounds.yMax + tilePos.y - PlayerController.Instance.currentCellPosition.y;
        

        // Create a 2D array of TileBase
        var tileArray = new TileBase[boundsNew.size.y, boundsNew.size.x];

        // Copy the tiles from the revealed area of the overlayTilemap to the tileArray
        for (var x = boundsNew.xMin; x < boundsNew.xMax; x++)
        {
            for (var y = boundsNew.yMin; y < boundsNew.yMax; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = overlayTilemap.GetTile(pos);
                // Index tileArray at relative position to the bounds
                var tilePosX = x - boundsNew.xMin;
                var tilePosY = y - boundsNew.yMin;
                tileArray[tilePosY, tilePosX] = tile;
            }
        }
        DrawDebugRectangle(bounds, Color.yellow);
        return tileArray;
    }
}

    