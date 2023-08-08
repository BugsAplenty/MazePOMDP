using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public enum TileType 
{
    Path,
    Wall,
    Darkness
}

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    // Rest of your code

    
    public int width;
    public int height;
    public Tilemap tilemap;
    public Tile wallTile;
    public Tile floorTile;
    private int[,] _dungeon;
    private List<Rect> _rooms;
    [Range(0, 100)]
    public int fillPercent; // Percentage of the room initially filled with walls
    public int smoothingIterations; // Number of times to apply the smoothing function
    public Tilemap mainMap; // assign in inspector
    public float mainMapHeight; // new variable
    public Tilemap overlayTilemap; // assign in inspector
    public TileType[,] Map; // This is the 2D array that represents your game map

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Map = new TileType[height, width];
        for(var y = 0; y < height; y++)
        {
            for(var x = 0; x < width; x++)
            {
                // Just setting random tiles for testing
                Map[y, x] = (TileType)Random.Range(0, 3);
            }
        }
        GenerateWorld();
    }
    public bool TileIsWall(Vector3Int tilePos)
    {
        // Convert from tile position to _dungeon array indices
        var x = tilePos.x + width / 2;
        var y = tilePos.y + height / 2;

        // Check if the coordinates are within the bounds of the array
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return _dungeon[x, y] == 1;
        }

        // If the coordinates are outside the array, treat them as a wall
        return true;
    }
    public bool TileIsWalkable(Vector3Int position)
    {
        return !TileIsWall(position);
    }

    public void GenerateWorld()
    {
        _dungeon = new int[width, height];
        _rooms = new List<Rect>();

        // Initialize dungeon with walls
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _dungeon[x, y] = 1;
            }
        }

        // Start the recursive Binary Space Partitioning
        Split(new Rect(0, 0, width, height));

        // Carve out corridors between rooms
        for (var i = 0; i < _rooms.Count - 1; i++)
        {
            var roomA = _rooms[i];
            var roomB = _rooms[i + 1];

            var roomACenter = new Vector2(roomA.x + roomA.width / 2, roomA.y + roomA.height / 2);
            var roomBCenter = new Vector2(roomB.x + roomB.width / 2, roomB.y + roomB.height / 2);

            // Draw a line from the center of room A to the center of room B
            while ((int)roomACenter.x != (int)roomBCenter.x)
            {
                roomACenter.x = roomACenter.x < roomBCenter.x ? roomACenter.x + 1 : roomACenter.x - 1;
                _dungeon[(int)roomACenter.x, (int)roomACenter.y] = 0;
            }

            while ((int)roomACenter.y != (int)roomBCenter.y)
            {
                roomACenter.y = roomACenter.y < roomBCenter.y ? roomACenter.y + 1 : roomACenter.y - 1;
                _dungeon[(int)roomACenter.x, (int)roomACenter.y] = 0;
            }
        }

        // Render the dungeon on the Tilemap
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                tilemap.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0),
                    _dungeon[x, y] == 1 ? wallTile : floorTile);
            }
        }
    }

    private void Split(Rect rect)
    {
        while (true)
        {
            // Choose a vertical or horizontal split depending on the proportions
            bool splitH;
            if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if (Mathf.Min(rect.height, rect.width) < 20)
            {
                // Minimum room size reached, stop splitting and create a room
                _rooms.Add(rect);
                CreateRoom(rect);
                return;
            }

            if (splitH)
            {
                // Split the rectangle horizontally
                var split = Random.Range((int)rect.y + 3, (int)(rect.y + rect.height - 3));

                Split(new Rect(rect.x, rect.y, rect.width, split - rect.y));
                rect = new Rect(rect.x, split, rect.width, rect.height - (split - rect.y));
            }
            else
            {
                // Split the rectangle vertically
                var split = Random.Range((int)rect.x + 3, (int)(rect.x + rect.width - 3));

                Split(new Rect(rect.x, rect.y, split - rect.x, rect.height));
                rect = new Rect(split, rect.y, rect.width - (split - rect.x), rect.height);
            }
        }
    }

    private void CreateRoom(Rect rect)
    {
        var room = new int[(int)rect.width, (int)rect.height];
        
        // Initial random fill
        for (var x = 0; x < rect.width; x++)
        {
            for (var y = 0; y < rect.height; y++)
            {
                if (x == 0 || x == rect.width - 1 || y == 0 || y == rect.height - 1)
                {
                    room[x, y] = 1;
                }
                else
                {
                    room[x, y] = (Random.Range(0, 100) < fillPercent) ? 1 : 0;
                }
            }
        }
        
        // Perform smoothing operations
        for (var i = 0; i < smoothingIterations; i++)
        {
            room = SmoothRoom(room);
        }
        
        // Copy the room into the dungeon
        for (var x = 0; x < rect.width; x++)
        {
            for (var y = 0; y < rect.height; y++)
            {
                _dungeon[(int)rect.x + x, (int)rect.y + y] = room[x, y];
            }
        }
    }

    private static int GetSurroundingWallCount(int[,] room, int gridX, int gridY)
    {
        var wallCount = 0;
        for (var neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (var neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < room.GetLength(0) && neighbourY >= 0 && neighbourY < room.GetLength(1))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += room[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private static int[,] SmoothRoom(int[,] room)
    {
        var newRoom = new int[room.GetLength(0), room.GetLength(1)];
        for (var x = 0; x < room.GetLength(0); x++)
        {
            for (var y = 0; y < room.GetLength(1); y++)
            {
                var neighbouringWalls = GetSurroundingWallCount(room, x, y);

                newRoom[x, y] = neighbouringWalls switch
                {
                    > 4 => 1,
                    < 4 => 0,
                    _ => newRoom[x, y]
                };
            }
        }
        return newRoom;
    }

    public Vector3 GetRandomPosition()
    {
        // Get a random non-wall tile position
        var x = Random.Range(0, width);
        var y = Random.Range(0, height);
        while (_dungeon[x, y] == 1)
        {
            x = Random.Range(0, width);
            y = Random.Range(0, height);
        }
        return new Vector3(x - width / 2, y - height / 2, 0);
    }

    public TileType GetTileType(TileBase tile)
    {
        if (tile == wallTile) return TileType.Wall;
        if (tile == floorTile) return TileType.Path;
        return TileType.Darkness;  // or other default
    }

}
