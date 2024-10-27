using UnityEngine;
using CustomAttribute;
public class PlayerController2DBasic : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Jump Settings")]
    [SerializeField] private bool useCoyoteTime = true;
    [SerializeField] private float coyoteTime = 0.08f;
    [SerializeField] private bool useJumpBuffer = true;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private bool useVariableJumpHeight = true;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.8f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Debug")]
    [ReadOnly] [SerializeField] private bool isGrounded;
    [ReadOnly] [SerializeField] private float moveHorizontal;
    [ReadOnly] [SerializeField] private float coyoteTimeCounter;
    [ReadOnly] [SerializeField] private float jumpBufferCounter;
    [ReadOnly] [SerializeField] private float verticalVelocity;
    [ReadOnly] [SerializeField] private bool jumpInputDown;
    [ReadOnly] [SerializeField] private bool jumpInputUp;



    private void Start()
    {
        if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
        if (!spriteRenderer) {spriteRenderer = GetComponent<SpriteRenderer>();}
    }

    private void Update()
    {
        CheckInputs();
        if (useCoyoteTime) CoyoteTimeCheck();
        if (useJumpBuffer) JumpBufferCheck();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyGravity();
        HandleMovement();
        HandleJumping();
        HandleSprite();
    }

     private void CheckInputs()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        jumpInputDown = Input.GetButtonDown("Jump");
        jumpInputUp = Input.GetButtonUp("Jump");

        if (jumpInputDown && useJumpBuffer)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private void CoyoteTimeCheck()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void JumpBufferCheck()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded
        }
    }

    private void HandleMovement()
    {
        float horizontalVelocity = moveHorizontal * moveSpeed;
        rigidBody.linearVelocity = new Vector2(horizontalVelocity, verticalVelocity);
    }



    private void HandleJumping()
    {
        if (useCoyoteTime && useJumpBuffer)
        {
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
            }
        }
        else if (useCoyoteTime && coyoteTimeCounter > 0f && jumpInputDown)
        {
            ExecuteJump();
        }
        else if (useJumpBuffer && isGrounded && jumpBufferCounter > 0f)
        {
            ExecuteJump();
            jumpBufferCounter = 0f;
        }
        else if (isGrounded && jumpInputDown)
        {
            ExecuteJump();
        }

        if (useVariableJumpHeight && jumpInputUp && verticalVelocity > 0f)
        {
            verticalVelocity *= jumpCutMultiplier;
        }
    }

    private void ExecuteJump()
    {
        verticalVelocity = jumpForce;
        isGrounded = false;
        coyoteTimeCounter = 0f;
    }

    private void ApplyGravity()
    {
        float gravity = gravityForce;
        
        if (verticalVelocity < 0)
        {
            gravity *= fallMultiplier;
        }

        verticalVelocity -= gravity * Time.fixedDeltaTime;
    }

    private void HandleSprite()
    {
        if (moveHorizontal != 0)
        {
            spriteRenderer.flipX = moveHorizontal < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}