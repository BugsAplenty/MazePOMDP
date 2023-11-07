using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : Singleton<PlayerController>
{

    public int clearRadius = 3;
    public float moveSpeed = 5f;
    public delegate void PlayerMovedEventHandler(object sender, EventArgs e);
    public event PlayerMovedEventHandler PlayerMoved;

    private Rigidbody2D _rb;
    public Vector3Int currentCellPosition;
    private bool _isMoving;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        var position = transform.position;
        currentCellPosition = WorldGenerator.Instance.tilemap.WorldToCell(position);
        FogOfWarController.Instance.ClearArea(position, clearRadius);
    }

    private void Update()
    {
        // If the player is currently moving, we don't want to start another move
        if (_isMoving)
            return;
        
        // Get user input
        var horizontal = (int) Input.GetAxisRaw("Horizontal");
        var vertical = (int) Input.GetAxisRaw("Vertical");

        // Check if we have some input
        if (horizontal == 0 && vertical == 0) return;
        // Make sure the player only moves in one direction
        if (horizontal != 0)
        {
            vertical = 0;
        }

        var direction = new Vector3Int(horizontal, vertical);

        // Calculate the new position
        var newPosition = currentCellPosition + direction;

        // Check if the new position is walkable
        if (WorldGenerator.Instance.TileIsWalkable(newPosition))
        {
            // Start the movement coroutine
            StartCoroutine(Move(newPosition));
        }
    }

    private IEnumerator Move(Vector3Int newPosition)
    {
        _isMoving = true;

        // Calculate the world position of the new cell
        var targetPosition = WorldGenerator.Instance.tilemap.GetCellCenterWorld(newPosition);

        // Move towards the target position until we reach it
        while ((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // We have reached the target position, update the cell position
        currentCellPosition = newPosition;

        // Clear the fog around the player
        FogOfWarController.Instance.ClearArea(transform.position, clearRadius);

        _isMoving = false;

        // Raise the PlayerMoved event
        OnPlayerMoved(EventArgs.Empty);
    }
    protected virtual void OnPlayerMoved(EventArgs e)
    {
        PlayerMoved?.Invoke(this, e);
    }

}
