using UnityEngine;

public class PlatformMoving2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveAcceleration = 2f; // How fast the platform gets to max speed
    [SerializeField] private float friction = 1f; // The higher the friction there is less resistance
    [SerializeField] private bool startMovingRight = true; // Direction to start moving in
    [SerializeField] private bool allowMovement = true; // Whether the platform can move
    [SerializeField] private bool movePlayer = true; // Whether to move the player with the platform
    
    [Header("Wall Detection")]
    [SerializeField] private float wallPauseTime = 1f; // How long to pause at walls
    private float wallPauseTimer = 0f;
    private bool isPaused = false;
    private bool isMovingRight;
    private Vector3 lastPosition;
    
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.03f;
    private bool isTouchingWallOnRight;
    private bool isTouchingWallOnLeft;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D collider;
    
    private void Start() 
    {
        isMovingRight = startMovingRight;
        lastPosition = transform.position;
    }
    
    private void Update() 
    {
        if (!allowMovement) return;
        HandleWallPause();
        if (!isPaused) CheckMoveDirection();
    }

    private void LateUpdate()
    {
        if (!movePlayer) return;
        
        // Calculate movement delta
        Vector3 movement = transform.position - lastPosition;
        
        // Move any players standing on the platform
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
            {
                child.position += movement;
            }
        }
        
        lastPosition = transform.position;
    }

    private void FixedUpdate() 
    {
        if (!allowMovement) 
        {
            // Stop platform movement when allowMovement is false
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }
        
        CollisionChecks();
        if (!isPaused) HandleMovement();
        else rigidBody.linearVelocity = Vector2.zero; // Ensure platform stops during pause
    }
    
    private void HandleWallPause()
    {
        if ((isTouchingWallOnRight && isMovingRight) || (isTouchingWallOnLeft && !isMovingRight))
        {
            if (!isPaused)
            {
                isPaused = true;
                wallPauseTimer = wallPauseTime;
            }
        }

        if (isPaused)
        {
            wallPauseTimer -= Time.deltaTime;
            if (wallPauseTimer <= 0)
            {
                isPaused = false;
                // Change direction after pause
                isMovingRight = !isMovingRight;
            }
        }
    }
    
    private void CheckMoveDirection()
    {
        // Check if there is a wall and flip
        if (!isMovingRight && isTouchingWallOnLeft) {
            isPaused = true;
            wallPauseTimer = wallPauseTime;
        } else if (isMovingRight && isTouchingWallOnRight) {
            isPaused = true;
            wallPauseTimer = wallPauseTime;
        }
    }
    
    private void HandleMovement() 
    {
        float speed = isMovingRight ? 1f : -1f;
        float acceleration = moveAcceleration;
        
        speed *= moveSpeed;
        acceleration *= friction;
        
        // Lerp the platforms movement
        float newXVelocity = Mathf.Lerp(rigidBody.linearVelocity.x, speed, acceleration * Time.fixedDeltaTime);
        rigidBody.linearVelocity = new Vector2(newXVelocity, rigidBody.linearVelocity.y);
    }
    
    private void CollisionChecks() 
    {
        // Check collision with walls on the right
        RaycastHit2D hitRight = Physics2D.Raycast(collider.bounds.center, Vector2.right, collider.bounds.extents.x + wallCheckDistance, groundLayer);
        Debug.DrawRay(collider.bounds.center, Vector2.right * (collider.bounds.extents.x + wallCheckDistance), Color.red);
        isTouchingWallOnRight = hitRight;

        // Check collision with walls on the left
        RaycastHit2D hitLeft = Physics2D.Raycast(collider.bounds.center, Vector2.left, collider.bounds.extents.x + wallCheckDistance, groundLayer);
        Debug.DrawRay(collider.bounds.center, Vector2.left * (collider.bounds.extents.x + wallCheckDistance), Color.red);
        isTouchingWallOnLeft = hitLeft;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!movePlayer || !collision.gameObject.CompareTag("Player")) return;
        
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f) // Player is above platform
            {
                if (collision.transform.parent != transform)
                {
                    collision.transform.SetParent(transform);
                }
                break;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (movePlayer && collision.gameObject.CompareTag("Player") && collision.transform.parent == transform)
        {
            collision.transform.SetParent(null);
        }
    }
}