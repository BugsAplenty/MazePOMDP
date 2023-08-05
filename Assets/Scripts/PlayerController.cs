using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int clearRadius = 3;  // Set this to the radius of fog you want to clear around the player
    public float speed = 50f;
    private Vector3 _lastPosition;
    private Vector3Int _currentCellPosition;

    private void Start()
    {
        var position = transform.position;
        _lastPosition = position;
        _currentCellPosition = WorldGenerator.Instance.tilemap.WorldToCell(position);  // Initialize the current cell position
        FogOfWarController.Instance.ClearArea(position, clearRadius);
    }

    void Update()
    {
        // Determine the direction of movement based on WASD key input
        Vector3Int direction = new Vector3Int(
            (int) Input.GetAxisRaw("Horizontal"),
            (int) Input.GetAxisRaw("Vertical"),
            0
        );

        if (!(direction.magnitude > 0)) return; // if any key was pressed
        var nextCellPosition = _currentCellPosition + direction;
        if (!WorldGenerator.Instance.TileIsWalkable(nextCellPosition)) return; // check if the destination cell is walkable
        _currentCellPosition = nextCellPosition;
        transform.position = WorldGenerator.Instance.tilemap.CellToWorld(_currentCellPosition) + new Vector3(0.5f, 0.5f, 0);  // adjust position to the center of the cell
        FogOfWarController.Instance.ClearArea(transform.position, clearRadius);
    }
}