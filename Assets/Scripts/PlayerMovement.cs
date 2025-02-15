using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of movement
    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles
    private Rigidbody2D rb;
    private Vector2 moveDirection; // Current movement direction
    private Vector2 targetPosition; // The next grid-aligned position
    private bool isMoving = false; // Whether the player is currently moving

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = rb.position; // Start aligned with the grid
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
        else if (moveDirection != Vector2.zero)
        {
            AttemptMove(moveDirection);
        }
    }

    private void HandleInput()
    {
        // Get input for movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Prevent diagonal movement
        if (horizontalInput != 0)
        {
            verticalInput = 0;
        }

        // Update movement direction based on input
        Vector2 newDirection = new Vector2(horizontalInput, verticalInput);
        if (newDirection != Vector2.zero)
        {
            moveDirection = newDirection; // Update direction
        }
    }

    private void MoveTowardsTarget()
    {
        // Move the player smoothly towards the target position
        rb.position = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

        // Check if the player has reached the target position
        if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
        {
            rb.position = targetPosition; // Snap to target position
            isMoving = false; // Stop movement

            // Immediately attempt to continue in the current direction
            if (moveDirection != Vector2.zero)
            {
                AttemptMove(moveDirection);
            }
        }
    }

    private void AttemptMove(Vector2 direction)
    {
        // Calculate the new target position
        Vector2 newTargetPosition = rb.position + direction;

        // Check for collisions using a raycast
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, 1f, obstacleLayer);
        if (hit.collider == null)
        {
            targetPosition = newTargetPosition; // Update the target position
            isMoving = true; // Start moving
        }
        else
        {
            // Stop movement if an obstacle is in the way
            moveDirection = Vector2.zero;
        }
    }
}
