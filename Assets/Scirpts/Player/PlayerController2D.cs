using VInspector;
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;

public enum PlayerState {
    Controllable,
    Frozen,
}

public class PlayerController2D : Entity2D {
    
    public static PlayerController2D Instance { get; private set; }

    [Tab("Player Settings")] // ----------------------------------------------------------------------
    public PlayerState currentPlayerState = PlayerState.Controllable;
    private string logText;
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private bool canTakeFallDamage;
    [ShowIf("canTakeFallDamage")][SerializeField] private int maxFallDamage = 1;[EndIf]
    private int currentHealth;
    private int deaths;
    private bool isInvincible;
    private float invincibilityTime;
    private bool isStunLocked;
    private float stunLockTime;
    [HideInInspector] public bool isFacingRight = true;

    [Header("Movement")]
    [SerializeField] [Min(0.1f)] private float walkSpeed = 4f;
    [SerializeField] [Min(0.1f)] private float airWalkSpeed = 3f;
    [SerializeField] [Min(0.1f)] private float moveAcceleration = 15f; // How fast the player gets to max speed
    [SerializeField] [Min(0.01f)] private float groundFriction = 0.15f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)]private float airFriction = 0.03f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)]private float platformFriction = 0.5f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)]private float movementThreshold = 0.1f;
    private bool isMoving;
    private float moveSpeed;
    

    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float variableJumpMaxHoldDuration = 0.3f; // How long the jump button can be held
    [SerializeField] [Range(0.1f, 1f)] private float variableJumpMultiplier = 0.5f; // Multiplier for jump cut height
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpDownBuffer = 0.2f; // For how long the jump buffer will hold
    [SerializeField] [Range(0, 2f)] private float coyoteJumpBuffer = 0.1f; // For how long the coyote buffer will hold
    private bool isJumping;
    private bool isJumpCut;
    private float holdJumpDownTimer;
    private bool canCoyoteJump;
    private float coyoteJumpTime;
    private float variableJumpHeldDuration;

    [Header("Gravity")] 
    [SerializeField] private bool lerpGravity;
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 4f; // Gravity multiplayer when the payer is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [SerializeField] private float fastFallSpeed = 10f; // The speed at which the player bobs after a fall
    [SerializeField] [Min(0)] private float fastFallBopDiminisher = 4f;
    [HideInInspector] public bool isFastFalling;
    [HideInInspector] public bool atMaxFallSpeed;
    [HideInInspector] public float fallSpeed;
    
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.02f;
    [SerializeField] [Range(0, 3f)] private float ledgeCheckDistance = 0.02f;
    [SerializeField] private float movingRigidBodyVelocityXDecayRate = 4f; // 0 Keep momentum
    private bool isTouchingGround;
    private bool isTouchingPlatform;
    private bool isTouchingWall;
    public bool isGrounded { get; private set; }
    private bool ledgeOnLeft;
    private bool ledgeOnRight;
    private bool isOnPlatform;
    private bool isTouchingWallOnRight;
    private bool isTouchingWallOnLeft;
    private bool onGroundObject;
    private Rigidbody2D movingRigidbody;
    private float movingRigidBodyLastVelocityX;
    private SoftObject2D softObject;

    
    [Header("Climb Steps")]
    [SerializeField] public bool autoClimbSteps;
    [ShowIf("autoClimbSteps")]
    [SerializeField] [Range(0, 1f)] private float stepHeight = 0.12f;
    [SerializeField] [Range(0, 1f)] private float stepWidth = 0.1f;
    [SerializeField] [Range(0, 1f)] private float stepCheckDistance = 0.05f;
    [SerializeField] private LayerMask stepLayer;
    [EndIf]
    
    [Header("Fast Drop")]
    [SerializeField] public bool canFastDrop = true;
    [ShowIf("canFastDrop")]
    [SerializeField] [Range(0, 1f)] private float fastFallAcceleration = 0.2f;
    private bool isFastDropping;
    [EndIf]
    

    [Tab("Player Abilities")] // ----------------------------------------------------------------------
    [Header("Run")]
    [SerializeField] public bool runAbility = true;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float airRunSpeed = 6f;
    [SerializeField] private float runningThreshold = 3; // How fast the player needs to move for running
    private bool wasRunning;
    private bool isRunning;
    
    [Header("Double Jump")]
    [SerializeField] public bool doubleJumpAbility = true;
    [SerializeField] [Range(1, 10f)] public int maxAirJumps = 1;
    private int remainingAirJumps;
    
    [Header("Wall Slide")]
    [SerializeField] public bool wallSlideAbility = true;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float maxWallSlideSpeed = 3f;
    [SerializeField] [Range(0, 1f)] private float wallSlideStickStrength = 0.3f;
    [HideInInspector] public bool isWallSliding;

    [Header("Wall Jump")]
    [SerializeField] public bool wallJumpAbility = true; 
    [SerializeField] private bool wallJumpResetsJumps = false;
    [SerializeField] private float wallJumpVerticalForce = 5f;
    [SerializeField] private float wallJumpHorizontalForce = 4f;

    [Header("Dash")]
    [SerializeField] public bool dashAbility = true;
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashPushForce = 10f;
    [SerializeField] private int maxDashes = 1;
    [SerializeField] private float dashCooldownDuration = 1f;
    [SerializeField] [Range(0.1f, 1f)] private float holdDashRequestTime = 0.1f; // For how long the dash buffer will hold
    private int remainingDashes;
    private bool isDashing;
    private float dashBufferTimer;
    private float dashCooldownTimer;
    private bool isDashCooldownRunning;
    
    [Tab("References")] // ----------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D collBody;
    [SerializeField] public Collider2D collFeet;
    [SerializeField] public Animator animator;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hurtVfx;
    [SerializeField] private ParticleSystem jumpVfx;
    [SerializeField] private ParticleSystem airJumpVfx;
    [SerializeField] private ParticleSystem peakMoveSpeedVfx;
    [SerializeField] private ParticleSystem peakFallSpeedVfx;
    [SerializeField] private ParticleSystem groundRunVfx;
    [SerializeField] private ParticleSystem deathVfx;
    [SerializeField] private ParticleSystem spawnVfx;
    [SerializeField] private ParticleSystem dashVfx;
    [SerializeField] private ParticleSystem bleedVfx;
    [SerializeField] private ParticleSystem wallSlideVfx;
    [SerializeField] private ParticleSystem healVfx;
    
    [Header("Colors")]
    [SerializeField] private Color hurtColor = Color.red;
    [SerializeField] private Color invincibilityColor = new Color(1,1,1,0.5f);
    private  Color defaultColor = Color.white;

    

    [Header("Input")] // ----------------------------------------------------------------------
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    private bool jumpInputDownRequested;
    private bool jumpInputUp;
    private bool jumpInputHeld;
    private bool dashRequested;
    private bool runInput;
    private bool dropDownInput;
    
    
    private void Awake() {

        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        
        CheckpointManager2D.Instance.SetSpawnPoint(transform.position);
        UIManager.Instance.UpdateAbilitiesUI();
        RespawnFromSpawnPoint();
    }
    
    private void Update() {
        if (!CanPlay()) { return; }
        
        CheckForInput();
        HandleDropDown();
        DashTimer();
        JumpChecks();
        CheckFaceDirection();
    }
    
    private void FixedUpdate() {
        if (!CanPlay()) { return; }

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleStepClimbing();
        HandleJump();
        HandleRunning();
        HandleWallSlide();
        HandleWallJump();
        HandleDashing();
        HandleFastDrop();
    }
    
    
    #region Movement functions //------------------------------------
    
    private void HandleMovement() {
        
        // Get the ground object momentum
        float targetMovingRigidBodyVelocity = CalculateMovingRigidBodyMomentum();
        
        // Get move speed and apply fiction
        float baseMoveSpeed = rigidBody.linearVelocity.x - targetMovingRigidBodyVelocity;
        moveSpeed = Mathf.Lerp(baseMoveSpeed, CalculateTargetMoveSpeed(), CalculateFriction());
        
        // Move
        rigidBody.linearVelocityX = moveSpeed + targetMovingRigidBodyVelocity;
        isMoving = moveSpeed  > movementThreshold || moveSpeed < -movementThreshold;
    }
    private float CalculateFriction() {
        
        // If grounded use ground friction else use air friction
        if (isGrounded) {
            return isOnPlatform || isDashing ? platformFriction : groundFriction; // If on a platform multiply the ground friction
        }

        return airFriction;
        
    }
    private float CalculateTargetMoveSpeed() {
        
        float targetSpeed = horizontalInput;
        float acceleration = moveAcceleration;

        // Handle move speed
        if (isWallSliding) { // Wall sliding

            if (horizontalInput < wallSlideStickStrength && horizontalInput > -wallSlideStickStrength) { 

                targetSpeed = 0; 
                acceleration = 0;
            }
        } else {
            if (isGrounded) { // On Ground

                targetSpeed *= runAbility && runInput ? runSpeed : walkSpeed;
                wasRunning = runAbility && runInput;

            } else{ // In air

                if (isTouchingWall) { wasRunning = false;}

                targetSpeed *= runAbility && runInput ? airRunSpeed : airWalkSpeed;
                wasRunning = runAbility && runInput;
            } 
        }
        
        
        // Lerp the player movement
        float targetMoveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, acceleration);
        
        
        return targetMoveSpeed;
    }
    
    private float CalculateMovingRigidBodyMomentum()
    {
        if (!movingRigidbody) { return 0f; } // If there is no ground object return
        if (onGroundObject) { return movingRigidbody.linearVelocityX; } // If on the ground object just get his velocity
        
        
        // If we're very close to 0, just return 0 to prevent tiny floating point values
        if (Mathf.Abs(movingRigidBodyLastVelocityX) < 0.01f) {
            movingRigidBodyLastVelocityX = 0;
            return 0f;
        }
        
        float targetMovingRigidBodyVelocity = Mathf.Lerp(movingRigidBodyLastVelocityX, 0f, movingRigidBodyVelocityXDecayRate * Time.fixedDeltaTime);
        movingRigidBodyLastVelocityX = targetMovingRigidBodyVelocity;
        return targetMovingRigidBodyVelocity ;
    }
    
    
    private void HandleDropDown() { 
        
        if (!softObject) return;

        if (dropDownInput) {
            rigidBody.linearVelocityY = jumpForce/3;
            PlayAnimation("DropDown");
            softObject.StartDropDownCooldown();
            softObject = null;
        }
    }
    
    private void HandleStepClimbing() {
        
        if (!autoClimbSteps) return; // Only check if you can climb steps
        
        if (isGrounded || isDashing) { // Check for steps when grounded or dashing
            
            Vector2 moveDirection = new Vector2(rigidBody.linearVelocity.x, 0).normalized;
            if (moveDirection != Vector2.zero ) { // Moving horizontally

                // Check for step in front of the player
                RaycastHit2D hitLower = Physics2D.Raycast(collFeet.bounds.center, moveDirection, collFeet.bounds.extents.x + stepCheckDistance + 0.05f, stepLayer);
                Debug.DrawRay(collFeet.bounds.center, moveDirection * (collFeet.bounds.extents.x + stepCheckDistance + 0.05f), Color.red);
                Debug.DrawRay(collFeet.bounds.center + new Vector3(0, stepHeight, 0), moveDirection * (collFeet.bounds.extents.x + stepCheckDistance), Color.red);

                if (hitLower.collider != null) {

                    // Check if there's space above the step and move the player
                    RaycastHit2D hitUpper = Physics2D.Raycast(collFeet.bounds.center + new Vector3(0, stepHeight, 0), moveDirection, collFeet.bounds.extents.x + stepCheckDistance, stepLayer);
                    if (!hitUpper) { rigidBody.position += new Vector2(horizontalInput * stepWidth, stepHeight); }
                }
            } 
        }
    }
    
    private void HandleFastDrop() {

        if (!canFastDrop) return;
        if (isGrounded && !atMaxFallSpeed) return;

        isFastDropping = verticalInput < 0;

        if (isFastDropping) {

            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y - fastFallAcceleration * Time.fixedDeltaTime);
        }
    }
    
    #endregion Movement functions
    
    
    #region Jump functions //------------------------------------
    // private void HandleJump() {
    //     
    //     if (jumpInputDownRequested) { // Jump
    //
    //         if (holdJumpDownTimer > holdJumpDownBuffer) { // If past jump buffer than don't jump
    //             jumpInputDownRequested = false;
    //             return;
    //         }
    //         
    //         string jumpDirection;
    //         if (rigidBody.linearVelocity.x < 0 && horizontalInput > 0) {
    //             jumpDirection = "Right";
    //         } else if (rigidBody.linearVelocity.x > 0 && horizontalInput < 0){
    //             jumpDirection = "Left";
    //         } else {
    //             jumpDirection = "None";
    //         }
    //         if (isGrounded || canCoyoteJump) { // Ground / Coyote jump
    //             ExecuteJump(1, "None");
    //         } else if (!(isGrounded && canCoyoteJump) && !isTouchingWall && remainingAirJumps > 1) { // Extra jump after coyote time passed
    //             ExecuteJump(2, jumpDirection);
    //         } else if (!(isGrounded && canCoyoteJump && isJumping) && !isTouchingWall && remainingAirJumps > 0) { // Extra jumps
    //             ExecuteJump(1, jumpDirection);
    //         }
    //     }
    //
    //     // Reset jumps and jump cut state when grounded
    //     if (isGrounded) { 
    //         remainingAirJumps = maxAirJumps;
    //         isJumpCut = false;
    //         // Only reset isJumping if not actively jumping
    //         if (rigidBody.linearVelocity.y <= 0) {
    //             isJumping = false;
    //         }
    //     }
    //
    //     // Reset jump state and cut when falling
    //     if (!isGrounded && rigidBody.linearVelocity.y <= 0) { 
    //         isJumping = false;
    //         isJumpCut = false;
    //     }
    // }
    private void HandleJump() {

        
        
        if (isGrounded) { 
            // Reset jumps and jump cut state
            remainingAirJumps = maxAirJumps;
            isJumpCut = false;
            
            // Reset coyote jump
            canCoyoteJump = true;
            coyoteJumpTime = coyoteJumpBuffer;
            
            // Only reset isJumping if not actively jumping
            if (rigidBody.linearVelocity.y <= 0) {
                isJumping = false;
            }
        }
        
        // Reset jump state and cut when falling
        if (!isGrounded && rigidBody.linearVelocity.y <= 0) { 
            isJumping = false;
            isJumpCut = false;
        }
        
        if (jumpInputDownRequested) { // Jump requsted
            
            if (holdJumpDownTimer > holdJumpDownBuffer)
            {
                // If past jump buffer than don't jump
                jumpInputDownRequested = false;
                return;
            }

            string jumpDirection;
            if (rigidBody.linearVelocity.x < 0 && horizontalInput > 0) {
                jumpDirection = "Right";
            } else if (rigidBody.linearVelocity.x > 0 && horizontalInput < 0) {
                jumpDirection = "Left";
            } else {
                jumpDirection = "None";
            }

            if ((isGrounded || canCoyoteJump) && !isJumping) { // Ground jump
                
                if (canCoyoteJump && !isGrounded) { logText = $"Coyote Jumped: {coyoteJumpTime}";}
                // Jump
                ExecuteJump(0, "None");
                PlayAnimation("Jump");
                SoundManager.Instance?.PlaySoundFX("Player Jump");
                
                // Reset coyote state
                coyoteJumpTime = 0;
                canCoyoteJump = false;
                
            } else if (!isGrounded && !isTouchingWall && !canCoyoteJump && doubleJumpAbility && remainingAirJumps > 0) { // Double Jump
                
                // Jump
                ExecuteJump(1, jumpDirection);
                PlayVfxEffect(airJumpVfx, true); 
                // PlayAnimation("AirJump");
                SoundManager.Instance?.PlaySoundFX("Player Air Jump");
            }
        }
        
    }
    
    private void ExecuteJump(int jumpCost, string side) {
        
        StopVfxEffect(peakFallSpeedVfx, true);
        
        // Jump
        if (side == "Right") {
            rigidBody.linearVelocity = new Vector2(jumpForce, jumpForce);
        } else if (side == "Left") {
            rigidBody.linearVelocity = new Vector2(-jumpForce, jumpForce);
        } else if (side == "None") {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
        }
        jumpInputDownRequested = false;
        remainingAirJumps -= jumpCost;
        isJumping = true;
        jumpInputHeld = true;
        isJumpCut = false;
        variableJumpHeldDuration = 0;
    }
    
    private void JumpChecks() {

        // Jump buffer timer
        if (holdJumpDownTimer <= holdJumpDownBuffer) {

            holdJumpDownTimer += Time.deltaTime;
        }
        
        // Variable jump timer
        if (jumpInputHeld && variableJumpHeldDuration <= variableJumpMaxHoldDuration) {

            variableJumpHeldDuration += Time.deltaTime;
        }
        
        // Only Cut jump height if button is released early in upward motion
        if (jumpInputUp && !isJumpCut) {  
            if (isJumping && rigidBody.linearVelocity.y > 0) {
                rigidBody.linearVelocityY *=  variableJumpMultiplier;
                // SoundManager.Instance?.StopSoundFx("Player Jump");
                isJumpCut = true;
            }
            jumpInputHeld = false;
            variableJumpHeldDuration = 0;
        }
        
        // Update coyote time
        if (canCoyoteJump) {
            if (coyoteJumpTime > 0) {
                coyoteJumpTime -= Time.deltaTime;
            } else {
                canCoyoteJump = false;
            }
        }
    }
    #endregion Jump functions

      
    #region Abilitis functions //------------------------------------

    private void HandleRunning() {
        if (!runAbility) return;
        
        
        bool isRunningOnGround = runInput && isGrounded && !isWallSliding && (moveSpeed > runningThreshold || moveSpeed < -runningThreshold);
        bool isRunningOnAir = wasRunning && !isGrounded && !isWallSliding && (moveSpeed > runningThreshold || moveSpeed < -runningThreshold);
        
        if (isRunningOnGround) { // When on ground
            PlayVfxEffect(peakMoveSpeedVfx, false);
            PlayVfxEffect(groundRunVfx, false);
            // PlayAnimation("Run");
            
        } else {
            StopVfxEffect(groundRunVfx, false);
            StopVfxEffect(peakMoveSpeedVfx, false);
            // PlayAnimation("Idle");
        }
        
        // if (isRunningOnAir) { // When in the air
        //     PlayVfxEffect(peakMoveSpeedVfx, false);
        // }
        //
        // if (!isRunningOnAir && !isRunningOnGround) {
        //     StopVfxEffect(groundRunVfx, false);
        //     StopVfxEffect(peakMoveSpeedVfx, false);
        // }
    
    }

    private void HandleDashing() {


        if (!dashAbility) return; // Return if not allowed to dash

        if (dashRequested && remainingDashes > 0) {

            int dashDirection = isFacingRight ? 1 : -1;

            // Play effects
            PlayVfxEffect(dashVfx, false);
            StopVfxEffect(wallSlideVfx, true);
            StopVfxEffect(peakFallSpeedVfx, true);
            PlayAnimation("Dash");
            SoundManager.Instance?.PlaySoundFX("Player Dash");

            // Dash
            isDashing = true;
            remainingDashes --;
            TurnInvincible();
            TurnStunLocked(0.1f);
            rigidBody.linearVelocityX += dashForce * dashDirection;
            if (isGrounded) { rigidBody.linearVelocityY += 1f; }
            dashRequested = false;
            StartCoroutine(DashCooldown());
        }
    }
    private IEnumerator DashCooldown() {

        if (isDashCooldownRunning) { yield break;} // Exit if already running
        isDashCooldownRunning = true;

        while (remainingDashes < maxDashes) {
            dashCooldownTimer = dashCooldownDuration;

            while (dashCooldownTimer > 0) {
                dashCooldownTimer -= Time.deltaTime;
                yield return null;
            }

            dashCooldownTimer = 0;
            remainingDashes++;
        }

        isDashCooldownRunning = false;
    }
    
    private void DashTimer() {

        // Dash buffer timer
        if (dashBufferTimer <= holdDashRequestTime) {

            dashBufferTimer += Time.deltaTime;
        }
    }
    

    private void HandleWallSlide() {

        // Check if wall sliding
        if (wallSlideAbility && isTouchingWall && !isGrounded && rigidBody.linearVelocity.y < 0) { 
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }

        // Play wall slide effect
        if (isWallSliding) { PlayVfxEffect(wallSlideVfx, false);}
        else { StopVfxEffect(wallSlideVfx, false); }
        
        
        
        if (isWallSliding) { 

            // Make the player face the opposite direction from the wall
            if (isTouchingWallOnLeft && !isFacingRight) { 
                FlipPlayer("Right");
            } else if (isTouchingWallOnRight && isFacingRight) {
                FlipPlayer("Left");
            }

            // Set slide speed
            float slideSpeed = wallSlideSpeed;
            float maxSlideSpeed = maxWallSlideSpeed;

            // Accelerate slide if fast dropping
            if (isFastDropping) {
                slideSpeed *= 1.5f;
                maxSlideSpeed *= 1.5f;
            }


            // Lerp the fall speed
            float newYVelocity = Mathf.Lerp(rigidBody.linearVelocity.y, -maxSlideSpeed, slideSpeed  * Time.fixedDeltaTime);
            rigidBody.linearVelocity = new Vector2 ( rigidBody.linearVelocity.x, newYVelocity);

            if (rigidBody.linearVelocity.y < -maxSlideSpeed) { // Clamp fall speed

                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, -maxSlideSpeed);

            }
            
        }   
    }
    
    private void HandleWallJump () {

        if (!wallJumpAbility) return;

        if (jumpInputDownRequested) {
            if (holdJumpDownTimer > holdJumpDownBuffer) {
                jumpInputDownRequested = false;
                return;
            }
            
            if (!isGrounded && isTouchingWallOnRight) { // Wall jump to the left
                ExecuteWallJump("Left");
                
            } else if (!isGrounded && isTouchingWallOnLeft ) { // Wall jump to the right
                ExecuteWallJump("Right");
            }
        }
        
        if (wallJumpResetsJumps && !isGrounded && isTouchingWall) { // Reset jumps
            remainingAirJumps = maxAirJumps;
            variableJumpHeldDuration = 0;
        }
    }
    private void ExecuteWallJump(string side) {
        
        // Effects
        StopVfxEffect(wallSlideVfx, true);
        SoundManager.Instance?.PlaySoundFX("Player Jump");

        // Jump
        if (side == "Right") {
            rigidBody.linearVelocity = new Vector2(wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Right");
        } else if (side == "Left") {
            rigidBody.linearVelocity = new Vector2(-wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Left");
        }
        logText = "Wall Jumped " + side;
        jumpInputDownRequested = false;
        variableJumpHeldDuration = 0;
        isJumping = true;
        TurnStunLocked();
    }

    #endregion Abilitis functions
    
    
    #region Gravity function //------------------------------------

    private void HandleGravity() {
        
          if (!isTouchingGround && !isWallSliding)
          {
              // In air
              HandleAirGravity();
              CheckFallSpeed();
          }
          else
          {
              // On ground
              HandleGroundedGravity();
              StopVfxEffect(peakFallSpeedVfx, true);
          }
    }
    private void HandleAirGravity() {

        float gravityMultiplier = rigidBody.linearVelocityY > 0 ? 1f : fallMultiplier;
        float appliedGravity = gravityForce * gravityMultiplier;
        float fallSpeed = rigidBody.linearVelocityY - (appliedGravity * Time.fixedDeltaTime); 
        

        if (!lerpGravity) {
            
            rigidBody.linearVelocityY = fallSpeed; // Values 9.8, 2
            
        } else {
            rigidBody.linearVelocityY = Mathf.Lerp(rigidBody.linearVelocity.y, -maxFallSpeed, appliedGravity * Time.fixedDeltaTime); // Values 0.5, 4
        }

    }
    private void CheckFallSpeed() {

        fallSpeed = (rigidBody.linearVelocityY < 0) ? rigidBody.linearVelocityY : 0; // If falling remember fall speed
        
        isFastFalling = rigidBody.linearVelocityY < -fastFallSpeed; // Check if fast falling
        if (atMaxFallSpeed) { PlayVfxEffect(peakFallSpeedVfx, false); }
        
        atMaxFallSpeed = rigidBody.linearVelocityY < -maxFallSpeed;
        if (rigidBody.linearVelocityY < -maxFallSpeed) { // Check if at max fall speed
            rigidBody.linearVelocityY = -maxFallSpeed; // Cap fall speed
        }
        
        
    }
    private void HandleGroundedGravity() {
        
        if (isFastFalling && isGrounded && !isDashing) { // Check if landed
            
            rigidBody.linearVelocityY = -1 * (fallSpeed/fastFallBopDiminisher); // Bop the player
            PlayAnimation("Land");
            
            if (atMaxFallSpeed && canTakeFallDamage) { // Apply fall damage
                DamageHealth(maxFallDamage, false, "Ground");
            }
            
            atMaxFallSpeed = false;
            isFastFalling = false;
        }
    }
    
    #endregion Gravity functions 
    
    
    #region Collision functions //------------------------------------

    private void CollisionChecks() {

        // Check if touching
        LayerMask combinedGroundMask = groundLayer | platformLayer;
        isTouchingGround = collFeet.IsTouchingLayers(combinedGroundMask);
        isTouchingPlatform = collFeet.IsTouchingLayers(platformLayer);
        isTouchingWall = collBody.IsTouchingLayers(combinedGroundMask);
        
        
        Vector2 checkSize = collFeet.bounds.size + new Vector3(-0.04f, 0);
        Vector2 checkCenter = collFeet.bounds.center;
        // Debug visualization
        Vector2 halfSize = checkSize * 0.5f;
        Vector2 topLeft = checkCenter + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = checkCenter + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = checkCenter + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = checkCenter + new Vector2(halfSize.x, -halfSize.y);
        Color color = Color.red;
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
        
        
        // Check if on ground
        if (isTouchingGround) {
            bool isOverlapping = Physics2D.OverlapBox(checkCenter, checkSize, 0f, combinedGroundMask);
            isGrounded = isOverlapping;
        }
        
        // Ledge checks
        if (isGrounded) {
            // Check collision with walls on the right
            RaycastHit2D hitRight = Physics2D.Raycast(new Vector3(collFeet.bounds.center.x + collFeet.bounds.extents.x + ledgeCheckDistance, collFeet.bounds.center.y, collFeet.bounds.center.z), Vector2.down, wallCheckDistance, combinedGroundMask );
            Debug.DrawRay(new Vector3(collFeet.bounds.center.x + collFeet.bounds.extents.x + ledgeCheckDistance, collFeet.bounds.center.y, collFeet.bounds.center.z), Vector2.down * (wallCheckDistance), Color.red);
            ledgeOnRight = !hitRight;
            

            // Check collision with walls on the left
            RaycastHit2D hitLeft = Physics2D.Raycast(new Vector3(collFeet.bounds.center.x - collFeet.bounds.extents.x - ledgeCheckDistance, collFeet.bounds.center.y, collFeet.bounds.center.z), Vector2.down, wallCheckDistance, combinedGroundMask );
            Debug.DrawRay(new Vector3(collFeet.bounds.center.x - collFeet.bounds.extents.x - ledgeCheckDistance, collFeet.bounds.center.y, collFeet.bounds.center.z), Vector2.down * (wallCheckDistance), Color.red);
            ledgeOnLeft = !hitLeft;
        }
        
        // Check if on platform
        if (isTouchingPlatform) {
            bool isOverlapping = Physics2D.OverlapBox(checkCenter, checkSize, 0f, platformLayer);
            isOnPlatform = isOverlapping;
        }
        
        // Check if touching a wall
        if (isTouchingWall) {

            // Check collision with walls on the right
            RaycastHit2D hitRight = Physics2D.Raycast(collBody.bounds.center, Vector2.right, collBody.bounds.extents.x + wallCheckDistance, combinedGroundMask );
            Debug.DrawRay(collBody.bounds.center, Vector2.right * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnRight = hitRight;

            // Check collision with walls on the left
            RaycastHit2D hitLeft = Physics2D.Raycast(collBody.bounds.center, Vector2.left, collBody.bounds.extents.x + wallCheckDistance, combinedGroundMask);
            Debug.DrawRay(collBody.bounds.center, Vector2.left * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnLeft = hitLeft;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        
        if (collision.contactCount == 0) return;
        collision.gameObject.TryGetComponent<SoftObject2D>(out softObject);
        collision.gameObject.TryGetComponent<Rigidbody2D>(out movingRigidbody);
        onGroundObject = movingRigidbody; 
        
        switch (collision.gameObject.tag) {
            case "Platform":

            break;
            case "Enemy":
                
                EnemyController2D enemyCont = collision.gameObject.GetComponent<EnemyController2D>();
                Vector2 enemyNormal = collision.GetContact(0).normal;
                Vector2 enemyPushForce = enemyNormal * 6f;
                Vector2 enemyDashForce = enemyNormal * dashPushForce;
                
                
                // Damage and push the player and enemy
                if (collision.collider == enemyCont.collHead) {

                    Push(enemyPushForce);
                    
                } else {
                    Push(enemyPushForce);
                    DamageHealth(1, true, collision.gameObject.name);
                }

                // Damage and push the enemy if the player is dashing
                if (isDashing) {
                    enemyCont.Push(-enemyDashForce);
                    // enemyCont.DamageHealth(1, true); 
                } 
                
            break;
            case "Spike":

                Vector2 spikeNormal = collision.GetContact(0).normal;
                Vector2 spikePushForce = spikeNormal * 5f;
                Push(spikePushForce);
                DamageHealth(1, true, collision.gameObject.name);

            break;
            
        }
    }

    private void OnCollisionExit2D(Collision2D other) {

        if (other.gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            if (rb == movingRigidbody) {
                onGroundObject = false;
                movingRigidBodyLastVelocityX = rb.linearVelocityX;
            }
        }
        softObject = null;
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        switch (collision.gameObject.tag) {
            case "RespawnTrigger":
                
                SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
                RespawnFromCheckpoint();
            break;
        }
    }
    
    #endregion Collision functions
    
    
    #region Health/Checkpoint functions //------------------------------------
    
    [Button] public void RespawnFromCheckpoint() {

        deaths += 1;
        if (CheckpointManager2D.Instance.activeCheckpoint) {
            Respawn(CheckpointManager2D.Instance.activeCheckpoint.transform.position);
        } else { RespawnFromSpawnPoint();}

        
    }
    [Button] private void RespawnFromSpawnPoint() {

        deaths = 0;
        Respawn(CheckpointManager2D.Instance.playerSpawnPoint);
    }
    private void Respawn(Vector2 position) {
        
        SetPlayerState(PlayerState.Frozen);
        Teleport(position, false);
        PlayAnimation("TeleportOut");
        
    }
    public void Teleport(Vector2 position, bool keepMomentum) {
        
        CameraController2D.Instance.transform.position = new Vector3(position.x, position.y, CameraController2D.Instance.transform.position.z);
        transform.position = position;

        if (!keepMomentum) {
            rigidBody.linearVelocity = new Vector2(0, 0);
            isDashing = false;
            wasRunning = false;
            StopVfxEffect(jumpVfx, true);
            StopVfxEffect(dashVfx, true);
            StopVfxEffect(wallSlideVfx, true);
            StopVfxEffect(bleedVfx, true);
            StopVfxEffect(healVfx, true);
            StopVfxEffect(hurtVfx, true);
            StopVfxEffect(deathVfx, true);
        }
    }

    private void CheckIfDead(string cause = "") {

        if (currentHealth <= 0) { // Dead

            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = Color.clear;
            }
            PlayAnimation("Death");
            SetPlayerState(PlayerState.Frozen);
            StopVfxEffect(bleedVfx,true);
            SpawnVfxEffect(deathVfx);
            SoundManager.Instance?.PlaySoundFX("Player Death");
            logText = "Death by: " + cause;
        }
    }
    
    private void DamageHealth(int damage, bool setInvincible, string cause = "") {
        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) { TurnInvincible();}
            TurnStunLocked();
            currentHealth -= damage;
            SpawnVfxEffect(hurtVfx);
            SoundManager.Instance.PlaySoundFX("Player Hurt");
            if (currentHealth == 1 && currentHealth < maxHealth) { PlayVfxEffect(bleedVfx, false); }
            logText = "Damaged by: " + cause;
            // CameraController2D.Instance?.ShakeCamera(0.4f,1f);
        } 
        CheckIfDead(cause);
    }
    

    public void HealToFullHealth() {
        if (currentHealth == maxHealth) return;
        StopVfxEffect(bleedVfx, true);
        SpawnVfxEffect(healVfx);
        currentHealth = maxHealth;
    }
    
    private void TurnInvincible(float invincibilityDuration = 0.5f) {

        StartCoroutine(Invisible(invincibilityDuration));
    }
    private void TurnVulnerable() {

        isInvincible = false;
        isDashing = false;
        
        foreach (SpriteRenderer sr in spriteRenderers) {
            sr.color = defaultColor;
        }
    }
    private IEnumerator Invisible(float invincibilityDuration) {
        
        isInvincible = true;
        invincibilityTime = invincibilityDuration;
        foreach (SpriteRenderer sr in spriteRenderers) {
            sr.color = invincibilityColor;
        }

        while (isInvincible && invincibilityTime > 0) {
            invincibilityTime -= Time.deltaTime;
            yield return null;
        }

        TurnVulnerable();
    }

    #endregion Health/Checkpoint functions
    

    #region Other functions //------------------------------------
    private void CheckForInput() {

        if (CanMove()) { // Only check for input if the player can move

            // Check for horizontal input
            horizontalInput = InputManager.Movement.x;

            // Check for vertical input
            verticalInput = InputManager.Movement.y;


            // Check for jump inputs
            jumpInputHeld = InputManager.JumpIsHeld;
            jumpInputUp = InputManager.JumpWasReleased;

            if (verticalInput > -1 && InputManager.JumpWasPressed) {
                jumpInputDownRequested = true;
                holdJumpDownTimer = 0f;

                // Only reset jump cut when starting a new jump
                if (isGrounded || canCoyoteJump || remainingAirJumps > 0) {
                    isJumpCut = false;
                }
            }
            
            // Check for run input
            if (runAbility) { runInput = InputManager.RunIsHeld; }

            // Check for dash input
            if (dashAbility && remainingDashes > 0 && InputManager.DashWasPressed) {
                dashRequested = true;
                dashBufferTimer = 0f;
            }
            
            // Check for drop down input
            dropDownInput = verticalInput <= -1 && InputManager.JumpWasPressed;
            
            // Check for restart input
            if (InputManager.RestartWasPressed) { RespawnFromCheckpoint(); }

        } else { // Set inputs to 0 if the player cannot move

            horizontalInput = 0;
            verticalInput = 0;
        }
    }
    
    
    
    private void Push(Vector2 pushForce) {
        
        if (currentHealth > 0 && !isInvincible) {
            
            // Reset current velocity before applying push
            rigidBody.linearVelocity = Vector2.zero;
            // Apply a consistent impulse force
            rigidBody.AddForce(pushForce, ForceMode2D.Impulse);
            // Clamp the resulting velocity to prevent excessive speed
            float maxPushSpeed = 4f;
            // Push the player
            rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity, maxPushSpeed);

        }
    }
    private void TurnStunLocked(float stunLockDuration = 0.1f) {
        
        StartCoroutine(StuckLock(stunLockDuration));
    }
    private void UnStuckLock() {

        isStunLocked = false;
        stunLockTime = 0f;
    }
    private IEnumerator StuckLock(float stunLockDuration) {
        
        isStunLocked = true;
        stunLockTime = stunLockDuration;

        while (isStunLocked && stunLockTime > 0) {
            stunLockTime -= Time.deltaTime;
            yield return null;
        }

        UnStuckLock();
    }
    private void CheckFaceDirection() {

        if (isWallSliding) return; // Only flip the player based on input if he is not wall sliding

        if (!isFacingRight && horizontalInput > 0) {
            FlipPlayer("Right");
        } else if (isFacingRight && horizontalInput < 0) {
            FlipPlayer("Left");
        }
    }
    private void FlipPlayer(string side) {
    
        if (isFacingRight && side == "Right" || !isFacingRight && side == "Left") return; // Only flip the player if he is not already facing the wanted direction

        StopVfxEffect(groundRunVfx, true);
        StopVfxEffect(peakMoveSpeedVfx, true);
        wasRunning = false;
        
        if (side == "Left") {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
            
        } else if (side == "Right") {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private bool CanMove() {
        return !isStunLocked && currentPlayerState == PlayerState.Controllable;
        
    }

    private bool CanPlay() {
        return currentPlayerState == PlayerState.Controllable && GameManager.Instance.currentGameState == GameStates.GamePlay;
    }
    public void SetPlayerState(PlayerState state) {
        currentPlayerState = state;
        
        switch (state) {
            case PlayerState.Controllable:
                rigidBody.linearVelocity = Vector2.zero;
                remainingAirJumps = maxAirJumps;
                remainingDashes = maxDashes;
                movingRigidBodyLastVelocityX = 0;
                isDashCooldownRunning = false;
                fallSpeed = 0;
                wasRunning = false;
                HealToFullHealth();
                SoundManager.Instance?.PlaySoundFX("Player Spawn");
                PlayVfxEffect(spawnVfx, true);
                StopVfxEffect(groundRunVfx, true);
                StopVfxEffect(peakMoveSpeedVfx, true);
                StopVfxEffect(jumpVfx, true);
                StopVfxEffect(dashVfx, true);
                StopVfxEffect(wallSlideVfx, true);
                StopVfxEffect(peakFallSpeedVfx, true);
                
                break;
            case PlayerState.Frozen:
                rigidBody.linearVelocity = Vector2.zero;
                StopVfxEffect(peakFallSpeedVfx, true);
                
                break;
        }
    }
    
    #endregion Other functions
    
    
    #region Vfx/Animations functions //------------------------------------
    
    private void PlayVfxEffect(ParticleSystem  effect, bool forcePlay) {
        
        if (!effect) return;
        if (forcePlay) {effect.Play();} else { if (!effect.isPlaying) { effect.Play(); } }
    }
    private void StopVfxEffect(ParticleSystem effect, bool clear) {
        if (!effect) return;
        
        if (effect.isPlaying) { 
            if (clear) { effect.Clear(); } 
            effect.Stop(); 
        }
    }
    
    private void SpawnVfxEffect(ParticleSystem effect) {
        if (!effect) return;
        Instantiate(effect, transform.position, Quaternion.identity);
    }
    public void PlayAnimation(string animationName) {
        
        if (!animator) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) return;
        animator.SetTrigger(animationName);
    }
    
    
    #endregion Sfx/Vfx functions
    
    
    #region Debugging functions //------------------------------------

    private readonly StringBuilder debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {

                debugStringBuilder.Clear();
                
                debugStringBuilder.AppendFormat("Player:\n");
                debugStringBuilder.AppendFormat("Health: {0} / {1}\n", currentHealth, maxHealth);
                debugStringBuilder.AppendFormat("Deaths: {0}\n\n", deaths);
                debugStringBuilder.AppendFormat("Jumps: {0} / {1}\n", remainingAirJumps, maxAirJumps);
                debugStringBuilder.AppendFormat("Dashes: {0} / {1} ({2:0.0} / {3:0.0})\n", remainingDashes, maxDashes, dashCooldownTimer, dashCooldownDuration);
                debugStringBuilder.AppendFormat("Velocity: ({0:0.0},{1:0.0})\n", rigidBody.linearVelocityX, rigidBody.linearVelocityY);
                debugStringBuilder.AppendFormat("Move Speed: ({0:0.0},{1:0.0})\n", moveSpeed, fallSpeed);

                debugStringBuilder.AppendFormat("\nStates:\n");
                debugStringBuilder.AppendFormat("Facing Right: {0}\n", isFacingRight);
                debugStringBuilder.AppendFormat("Invincible: {0} ({1:0.0})\n", isInvincible, invincibilityTime);
                debugStringBuilder.AppendFormat("Stun Locked: {0} ({1:0.0})\n", isStunLocked, stunLockTime);
                debugStringBuilder.AppendFormat("Running: {0}\n", wasRunning);
                debugStringBuilder.AppendFormat("Dashing: {0}\n", isDashing);
                debugStringBuilder.AppendFormat("Jumping: {0}\n", isJumping);
                debugStringBuilder.AppendFormat("Wall Sliding: {0}\n", isWallSliding);
                debugStringBuilder.AppendFormat("Fast Dropping: {0}\n", isFastDropping);
                debugStringBuilder.AppendFormat("Coyote Jumping: {0} ({1:0.0} / {2:0.0})\n",canCoyoteJump, coyoteJumpTime,coyoteJumpBuffer);
                debugStringBuilder.AppendFormat("Fast Falling: {0}\n", isFastFalling);
                debugStringBuilder.AppendFormat("At Max Fall Speed: {0}\n", atMaxFallSpeed);

                debugStringBuilder.AppendFormat("\nCollisions:\n");
                debugStringBuilder.AppendFormat("Grounded: {0}\n", isGrounded);
                debugStringBuilder.AppendFormat("On Platform: {0}\n", isOnPlatform);
                debugStringBuilder.AppendFormat("Touching Wall on Right: {0}, Left: {1}\n", isTouchingWallOnRight, isTouchingWallOnLeft);
                debugStringBuilder.AppendFormat("Ledge on Right: {0}, Left: {1}\n", ledgeOnRight, ledgeOnLeft);
                debugStringBuilder.AppendFormat("Ground Object: {0} {1:0.0} {2:0.0}\n", movingRigidbody, movingRigidbody?.linearVelocityX, movingRigidBodyLastVelocityX);
                debugStringBuilder.AppendFormat("Soft Object: {0}\n", softObject);

                // debugStringBuilder.AppendFormat("\nInputs:\n");
                // debugStringBuilder.AppendFormat($"H/V: {horizontalInput:F2} / {verticalInput:F2}\n");
                // debugStringBuilder.AppendFormat("Run: {0}\n", runInput);
                // debugStringBuilder.AppendFormat("Jump: {0}  ({1:0.0} / {2:0.0})\n", InputManager.JumpWasPressed, variableJumpHeldDuration, variableJumpMaxHoldDuration);
                // debugStringBuilder.AppendFormat("Dash: {0}\n", InputManager.DashWasPressed);
                // debugStringBuilder.AppendFormat("Drop Down: {0}\n", dropDownInput);
                
                debugStringBuilder.AppendFormat("\n{0}\n", logText);

                textObject.text = debugStringBuilder.ToString();
    }
    
    #endregion Debugging functions
    
}
