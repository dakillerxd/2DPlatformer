
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;

    // Movement vars
    private Vector2 moveVelocity;
    private bool isFacingRight;


    // Collision check var
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    [SerializeField] private bool isGrounded;
    private bool bumpedHead;

    // Jump vars
    public float VerticalVelocity {get; private set;}
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    // Apex vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    // Jump buffer vars
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    // Coyote time vars
    private float coyoteTimer;


    void Awake() {

        isFacingRight = true;
        if (!rigidBody) {rigidBody = GetComponent<Rigidbody2D>();}
    }


    void Update()
    {
        CountTimers();
        JumpChecks();
    }

    void FixedUpdate() {

        CollisionChecks();

        if (isGrounded) {

            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManagerUltimate.Movement);
        }
        else {

            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManagerUltimate.Movement);
        }

    }



    #region  Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput) {

        if (moveInput != Vector2.zero) {
            
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManagerUltimate.RunIsHeld) {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else { targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed; }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
            rigidBody.linearVelocity = new Vector2(moveVelocity.x, rigidBody.linearVelocity.y);
        }
        else if (moveInput == Vector2.zero) {

            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.deltaTime);
            rigidBody.linearVelocity = new Vector2(moveVelocity.x, rigidBody.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput) {

        if (isFacingRight && moveInput.x < 0f) { Turn(false); }
        else if (!isFacingRight && moveInput.x > 0f) { Turn(true); }

    }

    private void Turn(bool turnRight) {

        if (turnRight) {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion Movement


    #region Jumping

    private void JumpChecks() {

        // When we press the jump button
        if (InputManagerUltimate.JumpWasPressed) {

            jumpBufferTimer = MoveStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }


        // When we release the jump button
        if (InputManagerUltimate.JumpWasReleased) {

            if (jumpBufferTimer > 0f) {
                jumpReleasedDuringBuffer = true;
            }

            if (isJumping && VerticalVelocity > 0f) {

                if (isPastApexThreshold) {
                    
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
            
        }


        // Initiate jump with jump buffering and coyote time
        if (jumpBufferTimer > 0f && !isJumping &&(isGrounded || coyoteTimer > 0f)) {

            InitiateJump(1);

            if (jumpReleasedDuringBuffer) {

                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        // Double jump
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed) {

            isFastFalling = false;
            InitiateJump(1);
        }
        // Air jump after coyote time lapsed ( So you cannot double air jump after falling)
        else if (jumpBufferTimer  > 0f && isFalling && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1) {

            InitiateJump(2);
            isFastFalling = false;
        }


        // landed
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f) {

            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }

    }

    private void InitiateJump(int numberOfJumpsTooUse) {

        if (!isJumping) { 
            
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += numberOfJumpsTooUse;
        VerticalVelocity = MoveStats.InitialJumpVelocity;

    }

    private void Jump() {

        // Apply gravity while jumping
        if (isJumping) {

            // Check for head bump
            if (bumpedHead) { isFastFalling = true;}

            // Gravity on ascending
            if (VerticalVelocity > 0f) {
                
                apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (apexPoint > MoveStats.ApexThreshold) { // Apex controls

                    if (!isPastApexThreshold) {

                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold) {

                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < MoveStats.ApexHangTime) {

                            VerticalVelocity = 0f;
                        }
                        else {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                else { // Gravity on ascending but not past apex threshold

                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold) {

                        isPastApexThreshold = false;
                    }
                }
            }
            // Gravity on Descending
            else if (!isFastFalling) { 
                
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f) {

                if (!isFalling) {
                    isFalling = true;
                }
            }
        } 

        // Jump cut
        if (isFastFalling) {

            if (fastFallTime >= MoveStats.TimeForUpwardsCancel) {

                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < MoveStats.TimeForUpwardsCancel) {

                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            fastFallTime += Time.fixedDeltaTime;
        }

        // Normal gravity while falling
        if (!isGrounded && !isJumping) {

            if (!isFalling) {

                isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        // Clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, VerticalVelocity);

    }


    #endregion Jumping



    #region Collision Checks

    private void IsGrounded() {

        Vector2 boxCastOrigin = new Vector2(collFeet.bounds.center.x, collFeet.bounds.center.y);
        Vector2 boxCastSize = new Vector2(collFeet.bounds.size.x, collFeet.bounds.size.y);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectRayLength, MoveStats.GroundLayerMask);

        if (groundHit.collider != null) { isGrounded = true; }
        else { isGrounded = false; }

        if (MoveStats.DebugShowIsGroundedBox) {

            Color rayColor;
            if (isGrounded) { rayColor = Color.green; }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
    }

    private void BumpedHead() {

        Vector2 boxCastOrigin = new Vector2(collBody.bounds.center.x, collBody.bounds.max.y);
        Vector2 boxCastSize = new Vector2(collBody.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectRayLength, MoveStats.GroundLayerMask);

        if (headHit.collider != null) { bumpedHead = true; }
        else { bumpedHead = false; }

        if (MoveStats.DebugShowHeadBumpBox) {

            float headWidth = MoveStats.HeadWidth;
            Color rayColor;
            if (bumpedHead) { rayColor = Color.green; }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
    }
    private void CollisionChecks() {

        IsGrounded();
        BumpedHead();
    }


    #endregion Collision Checks


    #region  Timers

    private void CountTimers() {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded) {
            coyoteTimer -= Time.deltaTime;
        }
        else { coyoteTimer = MoveStats.JumpCoyoteTime; }
    }


    #endregion Timers


    
}
