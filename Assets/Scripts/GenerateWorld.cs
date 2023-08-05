public IEnumerator GenerateWorldCoroutine()
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
            tilemap.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0), _dungeon[x, y] == 1 ? wallTile : floorTile);
            yield return null;
        }
    }
}