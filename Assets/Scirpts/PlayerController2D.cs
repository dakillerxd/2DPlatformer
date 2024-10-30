using VInspector;
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController2D : MonoBehaviour
{
    private static PlayerController2D Instance { get; set; }


    [Tab("Player Settings")]
    [Header("Health")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private bool canTakeFallDamage;
        [ShowIf("canTakeFallDamage")]
        [SerializeField] private int maxFallDamage = 1;
        [EndIf]

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
    [SerializeField] [Min(0.01f)]private float movementThreshold = 0.1f;
    private bool isMoving;
    private float moveSpeed;
    private bool onGroundObject;
    private Rigidbody2D groundObjectRigidbody;
    private float groundObjectLastVelocityX;
    private float groundObjectMomentum;
    [SerializeField] [Range(0f, 1f)] private float objectVelocityDecayRate = 0.5f; // 0 Keep momentum, 1 leave momentum
    

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private float variableJumpMaxHoldDuration = 0.3f; // How long the jump button can be held
    [SerializeField] [Range(0.1f, 1f)] private float variableJumpMultiplier = 0.5f; // Multiplier for jump cut height
    [SerializeField] [Range(1, 5f)] private int maxJumps = 2;
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpDownBuffer = 0.2f; // For how long the jump buffer will hold
    [SerializeField] [Range(0, 2f)] private float coyoteJumpBuffer = 0.1f; // For how long the coyote buffer will hold
    private bool isJumping;
    private bool isJumpCut;
    private int remainingJumps;
    private float holdJumpDownTimer;
    private bool canCoyoteJump;
    private float coyoteJumpTime;
    private float variableJumpHeldDuration;
    
    [Header("Gravity")]
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 4f; // Gravity multiplayer when the payer is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [HideInInspector] public bool isFastFalling;
    [HideInInspector] public bool atMaxFallSpeed;
    
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isOnPlatform;
    private bool isTouchingWall;
    private bool isTouchingWallOnRight;
    private bool isTouchingWallOnLeft;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.02f;


    [Header("Debug")]
    [SerializeField] private bool showDebugText;
    [SerializeField] private bool showFpsText;
    private TextMeshProUGUI debugText;
    private TextMeshProUGUI fpsText;
    [EndTab]

    // ----------------------------------------------------------------------

    [Tab("Player Abilities")]
    [Header("Running")]
    [SerializeField] private bool runAbility = true;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float airRunSpeed = 6f;
    [SerializeField] private float runningThreshold = 3; // How fast the player needs to move for running
    private bool wasRunning;
    private bool isRunning;

    [Header("Climb Steps")]
    [SerializeField] private bool autoClimbStepsAbility = true;
    [SerializeField] [Range(0, 1f)] private float stepHeight = 0.12f;
    [SerializeField] [Range(0, 1f)] private float stepWidth = 0.1f;
    [SerializeField] [Range(0, 1f)] private float stepCheckDistance = 0.05f;
    [SerializeField] private LayerMask stepLayer;
    
    [Header("Wall Slide")]
    [SerializeField] private bool wallSlideAbility = true;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float maxWallSlideSpeed = 3f;
    [SerializeField] [Range(0, 1f)] private float wallSlideStickStrength = 0.3f;
    [HideInInspector] public bool isWallSliding;

    [Header("Wall Jump")]
    [SerializeField] private bool wallJumpAbility = true;
    [SerializeField] private bool wallJumpResetsJumps = false;
    [SerializeField] private float wallJumpVerticalForce = 5f;
    [SerializeField] private float wallJumpHorizontalForce = 4f;

    [Header("Dash")]
    [SerializeField] private bool dashAbility = true;
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

    [Header("Fast Drop")]
    [SerializeField] private bool fastDropAbility = true;
    [SerializeField] [Range(0, 1f)] private float fastFallAcceleration = 0.2f;
    private bool isFastDropping;
    [EndTab]
    
    // ----------------------------------------------------------------------

    [Tab("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D collBody;
    [SerializeField] public Collider2D collFeet;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hurtVfx;
    [SerializeField] private ParticleSystem jumpVfx;
    [SerializeField] private ParticleSystem airJumpVfx;
    [SerializeField] private ParticleSystem peakMoveSpeedVfx;
    [SerializeField] private ParticleSystem groundRunVfx;
    [SerializeField] private ParticleSystem deathVfx;
    [SerializeField] private ParticleSystem spawnVfx;
    [SerializeField] private ParticleSystem dashVfx;
    [SerializeField] private ParticleSystem bleedVfx;
    [SerializeField] private ParticleSystem wallSlideVfx;
    [SerializeField] private ParticleSystem healVfx;
    
    [EndTab]

    // ----------------------------------------------------------------------

    [Header("Input")]
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    private bool jumpInputDownRequested;
    private bool jumpInputUp;
    private bool jumpInputHeld;
    private bool dashRequested;
    private bool runInput;
    private bool dropDownInput;




    
    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

       } else {

            Instance = this;
       }

       QualitySettings.vSyncCount = 0;
       Application.targetFrameRate = 120;
    }
 

    private void Start() {

        fpsText = GameObject.Find("FpsText").GetComponent<TextMeshProUGUI>();
        debugText = GameObject.Find("PlayerDebugText").GetComponent<TextMeshProUGUI>();
        
        currentHealth = maxHealth;
        remainingDashes = maxDashes;
        isDashCooldownRunning = false;
        deaths = 0;
        groundObjectLastVelocityX = 0;
        PlayVfxEffect(spawnVfx, true);
        CheckpointManager2D.Instance.SetSpawnPoint(transform.position);
    }


    private void Update() {

        if (Input.GetKeyDown(KeyCode.R)) { RespawnFromCheckpoint();}
        if (Input.GetKeyDown(KeyCode.T)) { SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
        UpdateDebugText(); 
        UpdateFpsText();
        
        CheckForInput();
        HandleDropDown();
        CountTimers();
        CoyoteTimeCheck();
        CheckFaceDirection();
    }


    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
        
        HandleJump();
        HandleRunning();
        HandleWallSlide();
        HandleWallJump();
        HandleDashing();
        HandleStepClimbing();
        HandleFastDrop();
    }


    //------------------------------------
    
    #region Movement function
    private void HandleMovement() {
        
        // Get the ground object momentum
        groundObjectMomentum = CalculateGroundObjectMomentum();
        
        // Get move speed and apply fiction
        float baseMoveSpeed = rigidBody.linearVelocity.x - groundObjectMomentum;
        moveSpeed = Mathf.Lerp(baseMoveSpeed, CalculateTargetMoveSpeed(), CalculateFriction());
        
        // Move
        rigidBody.linearVelocityX = moveSpeed + groundObjectMomentum;
        isMoving = moveSpeed  > movementThreshold || moveSpeed < -movementThreshold;
    }
    private float CalculateFriction() {
        
        // If grounded use ground friction else use air friction
        if (isGrounded)
        {
            return isOnPlatform ? groundFriction*3 : groundFriction; // If on a platform multiply the ground friction
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
    private float CalculateGroundObjectMomentum()
    {
        if (!groundObjectRigidbody) return 0;
    
        float currentVelocity = groundObjectRigidbody.linearVelocityX;
    
        if (onGroundObject) {
            groundObjectLastVelocityX = currentVelocity;
            return currentVelocity;
        }

        // Lerp from the last velocity to 0
        groundObjectLastVelocityX = Mathf.Lerp(groundObjectLastVelocityX, 0, objectVelocityDecayRate);
        return groundObjectLastVelocityX;
    }

    private void HandleDropDown()
    { 
        if (!isOnPlatform) return;

        if (dropDownInput) {
            Collider2D platformCollider = groundObjectRigidbody.gameObject.GetComponent<Collider2D>();
            platformCollider.enabled = false;
            StartCoroutine(DropDownCooldown(platformCollider));
        }
    }
    
    private IEnumerator DropDownCooldown(Collider2D  platformCollider) {

        float time = 0.3f;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        platformCollider.enabled = true;
    }
    
    #endregion Movement function
    
    //------------------------------------
    
    #region Abilitis functions

    private void HandleRunning() {
        if (!runAbility) return;
        
        
        bool isRunningOnGround = runInput && isGrounded && !isWallSliding && (moveSpeed > runningThreshold || moveSpeed < -runningThreshold);
        bool isRunningOnAir = wasRunning && !isGrounded && !isWallSliding && (moveSpeed > runningThreshold || moveSpeed < -runningThreshold);
        
        if (isRunningOnGround) { // When on ground
            PlayVfxEffect(peakMoveSpeedVfx, false);
            PlayVfxEffect(groundRunVfx, false);
            
        } else {
            StopVfxEffect(groundRunVfx, false);
        }
        
        if (isRunningOnAir) { // When in the air
            PlayVfxEffect(peakMoveSpeedVfx, false);
        }

        if (!isRunningOnAir && !isRunningOnGround) {
            StopVfxEffect(groundRunVfx, false);
            StopVfxEffect(peakMoveSpeedVfx, false);
        }
    
    }
    private void HandleFastDrop() {

        if (!fastDropAbility) return;
        if (isGrounded && !atMaxFallSpeed) return;

        isFastDropping = verticalInput < 0;

        if (isFastDropping) {

            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y - fastFallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleDashing() {


        if (!dashAbility) return; // Return if not allowed to dash

        if (dashRequested && remainingDashes > 0) {

            int dashDirection = isFacingRight ? 1 : -1;

            // Play effects
            PlayVfxEffect(dashVfx, false);
            StopVfxEffect(wallSlideVfx, true);
            SoundManager.Instance?.PlaySoundFX("Player Dash");

            // Dash
            isDashing = true;
            remainingDashes --;
            TurnInvincible();
            TurnStunLocked(0.1f);
            rigidBody.linearVelocityX += dashForce * dashDirection;
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

    private void HandleStepClimbing() {
        
        if (!autoClimbStepsAbility) return; // Only check if you can climb steps
        
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

    
    private void HandleWallSlide() {

        // Check if wall sliding
        if (wallSlideAbility && isTouchingWall && !isGrounded && rigidBody.linearVelocity.y < 0) { 
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }

        // Play wall slide effect
        if (isWallSliding) { PlayVfxEffect(wallSlideVfx, false); }
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

    #endregion Abilitis functions
    
    //------------------------------------
    
    #region Jump functions
    private void HandleJump() {
        
        if (jumpInputUp && !isJumpCut) { 
            
            // Only Cut jump height if button is released early AND we're still in upward motion
            if (isJumping && rigidBody.linearVelocity.y > 0) {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y * variableJumpMultiplier);
                isJumpCut = true;
            }
            jumpInputHeld = false;
            variableJumpHeldDuration = 0;
        }

        if (jumpInputDownRequested) { // Jump

            if (holdJumpDownTimer > holdJumpDownBuffer) { // If past jump buffer than don't jump
                jumpInputDownRequested = false;
                return;
            }
            
            string jumpDirection;
            if (rigidBody.linearVelocity.x < 0 && horizontalInput > 0) {
                jumpDirection = "Right";
            } else if (rigidBody.linearVelocity.x > 0 && horizontalInput < 0){
                jumpDirection = "Left";
            } else {
                jumpDirection = "None";
            }
            if (isGrounded || canCoyoteJump) { // Ground / Coyote jump
                ExecuteJump(1, "None");
            } else if (!(isGrounded && canCoyoteJump) && !isTouchingWall && remainingJumps > 1) { // Extra jump after coyote time passed
                ExecuteJump(2, jumpDirection);
            } else if (!(isGrounded && canCoyoteJump && isJumping) && !isTouchingWall && remainingJumps > 0) { // Extra jumps
                ExecuteJump(1, jumpDirection);
            }
        }

        // Reset jumps and jump cut state when grounded
        if (isGrounded) { 
            remainingJumps = maxJumps;
            isJumpCut = false;
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
    }


    private void ExecuteJump(int jumpCost, string side) {

        // Play effects
        if (!isGrounded) {PlayVfxEffect(airJumpVfx, true); SoundManager.Instance?.PlaySoundFX("Player Air Jump"); };  // CameraController2D.Instance?.ShakeCamera(0.2f,0.1f);
        if (isGrounded) { SoundManager.Instance?.PlaySoundFX("Player Jump");}

        // Jump
        if (side == "Right") {
            rigidBody.linearVelocity = new Vector2(jumpForce, jumpForce);
        } else if (side == "Left") {
            rigidBody.linearVelocity = new Vector2(-jumpForce, jumpForce);
        } else if (side == "None") {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
        }
        jumpInputDownRequested = false;
        remainingJumps -= jumpCost;
        isJumping = true;
        jumpInputHeld = true;
        isJumpCut = false;
        variableJumpHeldDuration = 0;

        // Reset coyote state
        coyoteJumpTime = 0;
        canCoyoteJump = false;
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
            remainingJumps = maxJumps;
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
        jumpInputDownRequested = false;
        variableJumpHeldDuration = 0;
        isJumping = true;
        TurnStunLocked();
    }

    private void CoyoteTimeCheck() {

        // Reset coyote jump
        if (isGrounded) {
            canCoyoteJump = true;
            coyoteJumpTime = coyoteJumpBuffer;
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

    //------------------------------------
    
    #region Gravity function
    private void HandleGravity()
    {
        if (!isGrounded && !isWallSliding) // Apply gravity when not grounded
        {
            // Apply gravity and apply the fall multiplier if the player is falling
            float gravityMultiplier = rigidBody.linearVelocity.y > 0 ? 1f : fallMultiplier; 
            rigidBody.linearVelocity = new Vector2 ( rigidBody.linearVelocity.x, Mathf.Lerp(rigidBody.linearVelocity.y, -maxFallSpeed, gravityForce * gravityMultiplier * Time.fixedDeltaTime));


            // Cap fall speed
            if (!isWallSliding)
            {

                isFastFalling = rigidBody.linearVelocity.y < -(maxFallSpeed/2);


                if (rigidBody.linearVelocity.y < -maxFallSpeed) {

                    atMaxFallSpeed = true;
                    rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, -maxFallSpeed);
                    if (CameraController2D.Instance?.isShaking == false) { CameraController2D.Instance.ShakeCamera( 1f, 4f,2,2); } // shake camera when at max fall speed

                } else { atMaxFallSpeed = false; }
            }

        } else { // Apply gravity when grounded

            // const float groundGravityForce = 0.1f;
            // rigidBody.linearVelocity += groundGravityForce * Time.fixedDeltaTime * Vector2.down;

            // Check for landing at fast falling
            if (isFastFalling) {

                if (CameraController2D.Instance?.isShaking == true) { // Stop camera shake
                    CameraController2D.Instance.StopCameraShake();
                }

                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce/2); // Make the player bop
                isFastFalling = false;
            }

            // Check for landing at max speed
            if (atMaxFallSpeed) {

                if (canTakeFallDamage) { DamageHealth(maxFallDamage, false, "Ground");} // Take damage 
                

                if (CameraController2D.Instance?.isShaking == true) { // Stop camera shake
                    CameraController2D.Instance.StopCameraShake();
                }

                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce); // Make the player bop
                atMaxFallSpeed = false;
                isFastFalling = false;
            }
        }
    }


    #endregion Gravity functions
    
    //------------------------------------
    
    #region Collision functions

    private void CollisionChecks() {

        
        LayerMask combinedMask = groundLayer | platformLayer;
        
        // Check if on grounded
        isGrounded = collFeet.IsTouchingLayers(combinedMask);

        // Check if on platform
        isOnPlatform = collFeet.IsTouchingLayers(platformLayer);
        
        
        // Check if touching a wall
        isTouchingWall = collBody.IsTouchingLayers(combinedMask);
        if (isTouchingWall) {

            // Check collision with walls on the right
            RaycastHit2D hitRight = Physics2D.Raycast(collBody.bounds.center, Vector2.right, collBody.bounds.extents.x + wallCheckDistance, combinedMask );
            Debug.DrawRay(collBody.bounds.center, Vector2.right * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnRight = hitRight;

            // Check collision with walls on the left
            RaycastHit2D hitLeft = Physics2D.Raycast(collBody.bounds.center, Vector2.left, collBody.bounds.extents.x + wallCheckDistance, combinedMask);
            Debug.DrawRay(collBody.bounds.center, Vector2.left * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnLeft = hitLeft;
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        
        if (collision.contactCount == 0) return;
        
        switch (collision.gameObject.tag) {
            case "Platform":
                    collision.gameObject.TryGetComponent<Rigidbody2D>(out groundObjectRigidbody);
                    onGroundObject = true; 
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
            if (rb == groundObjectRigidbody) {
                onGroundObject = false;
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        switch (collision.gameObject.tag) {
            case "RespawnTrigger":
                
                SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
                RespawnFromCheckpoint();

            break;
            case "Checkpoint":

                HealToFullHealth();
                CheckpointManager2D.Instance.ActivateCheckpoint(collision.gameObject);

            break;
            case "Teleporter":

                SceneTeleporter2D teleporter = collision.gameObject.GetComponent<SceneTeleporter2D>();
                teleporter.GoToSelectedLevel();
            break;
            case "Portal":

                Portal2D portal = collision.gameObject.GetComponent<Portal2D>();
                if (portal.CanTeleport()) {
                    portal.StartCooldown();
                    portal.StartCooldownForConnectedPortal();
                    Teleport(portal.GetConnectedPortalLocation());
                }
                
                break;
        }
    }
    
    #endregion Collision functions
    
    //------------------------------------
    
    #region Health/Checkpoint functions
    

    [Button] private void RespawnFromCheckpoint() {

            if (CheckpointManager2D.Instance.activeCheckpoint) {
                Respawn(CheckpointManager2D.Instance.activeCheckpoint.transform.position);
            } else { RespawnFromSpawnPoint();}

        
    }

    [Button] private void RespawnFromSpawnPoint() {

        Respawn(CheckpointManager2D.Instance.playerSpawnPoint);
    }
    
    
    private void Respawn(Vector2 position) {

        // Reset stats/states
        deaths += 1;
        transform.position = position;
        rigidBody.linearVelocity = new Vector2(0, 0);
        remainingDashes = maxDashes;
        remainingJumps = maxJumps;
        wasRunning = false;
        isDashCooldownRunning = false;
        TurnInvincible(2f);
        TurnStunLocked(0.5f);
        HealToFullHealth();

        // Play effects
        PlayVfxEffect(spawnVfx, false);
        SoundManager.Instance?.PlaySoundFX("Player Spawn");
        if (CameraController2D.Instance?.isShaking == true) { // Stop camera shake
            CameraController2D.Instance.StopCameraShake();
        }
        

        // Debug.Log("Respawned");
    }


    private void Teleport(Vector2 position) {
        

        CameraController2D.Instance.transform.position = new Vector3(position.x, position.y, CameraController2D.Instance.transform.position.z);
        transform.position = position;
        rigidBody.linearVelocity = new Vector2(0, 0);
        isDashing = false;
        wasRunning = false;
        StopVfxEffect(jumpVfx, true);
        StopVfxEffect(dashVfx, true);
        StopVfxEffect(wallSlideVfx, true);
        Push(new Vector2(jumpForce, jumpForce));
        TurnStunLocked(0.2f);
        
        PlayVfxEffect(spawnVfx, false);
        SoundManager.Instance?.PlaySoundFX("Player Teleport");
    }

    private void DamageHealth(int damage, bool setInvincible, string cause = "") {
        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) { TurnInvincible();}
            TurnStunLocked();
            currentHealth -= damage;
            SpawnVfxEffect(hurtVfx);
            SoundManager.Instance.PlaySoundFX("Player Hurt");
            if (currentHealth == 1 && currentHealth < maxHealth) { PlayVfxEffect(bleedVfx, false); }
            // CameraController2D.Instance?.ShakeCamera(0.4f,1f);
            // Debug.Log("Damaged by: " + cause);
        } 
        CheckIfDead(cause);
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

            SoundManager.Instance?.PlaySoundFX("Player Pushed");

        }
    }

    private void CheckIfDead(string cause = "") {

        if (currentHealth <= 0) {

            SpawnVfxEffect(deathVfx);
            SoundManager.Instance?.PlaySoundFX("Player Death");

            // Debug.Log("Death by: " + cause);

            RespawnFromCheckpoint();
        }
    }

    private void HealToFullHealth() {
        if (currentHealth == maxHealth) return;
        currentHealth = maxHealth;
        SpawnVfxEffect(healVfx);
        StopVfxEffect(bleedVfx, true);
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



    private void TurnInvincible(float invincibilityDuration = 0.5f) {

        StartCoroutine(Invisible(invincibilityDuration));
    }
    
    private void TurnVulnerable() {

        isInvincible = false;
        isDashing = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private IEnumerator Invisible(float invincibilityDuration) {
        
        isInvincible = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        invincibilityTime = invincibilityDuration;

        while (isInvincible && invincibilityTime > 0) {
            invincibilityTime -= Time.deltaTime;
            yield return null;
        }

        TurnVulnerable();
    }

    #endregion Health/Checkpoint functions

    //------------------------------------

    #region Other functions

    private void CheckForInput() {

        if (CanMove()) { // Only check for input if the player can move

            // Check for horizontal input
            horizontalInput = Input.GetAxis("Horizontal");

            // Check for vertical input
            verticalInput = Input.GetAxis("Vertical");


            // Check for jump inputs
            jumpInputHeld = Input.GetButton("Jump");
            jumpInputUp = Input.GetButtonUp("Jump");

            if (Input.GetButtonDown("Jump")) {
                jumpInputDownRequested = true;
                holdJumpDownTimer = 0f;

                // Only reset jump cut when starting a new jump
                if (isGrounded || canCoyoteJump || remainingJumps > 0) {
                    isJumpCut = false;
                }
            }
            
            // Check for run input
            if (runAbility) { runInput = Input.GetButton("Run"); }

            // Check for dash input
            if (dashAbility && remainingDashes > 0 && Input.GetButtonDown("Dash")) {
                dashRequested = true;
                dashBufferTimer = 0f;
            }
            
            // Check for drop down input
            dropDownInput = verticalInput < -0.2 && Input.GetButtonDown("Crouch");

        } else { // Set inputs to 0 if the player cannot move

            horizontalInput = 0;
            verticalInput = 0;
        }
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


    private void CountTimers() {

        // Jump buffer timer
        if (holdJumpDownTimer <= holdJumpDownBuffer) {

            holdJumpDownTimer += Time.deltaTime;
        }

        // Variable jump timer
        if (jumpInputHeld && variableJumpHeldDuration <= variableJumpMaxHoldDuration) {

            variableJumpHeldDuration += Time.deltaTime;
        }

        // Dash buffer timer
        if (dashBufferTimer <= holdDashRequestTime) {

            dashBufferTimer += Time.deltaTime;
        }
    }
    
    private bool CanMove() {
        return !isStunLocked;
    }
    
    #endregion Other functions
    
    //------------------------------------
    
    #region Vfx functions
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

    private bool IsVfxPlaying(ParticleSystem effect) {
        if (!effect) return false;
        return  effect.isPlaying;
    }
    
    #endregion Sfx/Vfx functions
    
    //------------------------------------
    
    #region Debugging functions
    #if UNITY_EDITOR || DEVELOPMENT_BUILD

    private readonly StringBuilder debugStringBuilder = new StringBuilder(256);
    private void UpdateDebugText() {

        if (debugText) {
            debugText.enabled = showDebugText;
            if (showDebugText) {  

                debugStringBuilder.Clear();
                
                debugStringBuilder.AppendFormat("Player:\n");
                debugStringBuilder.AppendFormat("Health: {0} / {1}\n", currentHealth, maxHealth);
                debugStringBuilder.AppendFormat("Deaths: {0}\n\n", deaths);
                debugStringBuilder.AppendFormat("Jumps: {0} / {1}\n", remainingJumps, maxJumps);
                debugStringBuilder.AppendFormat("Dashes: {0} / {1} ({2:0.0} / {3:0.0})\n", remainingDashes, maxDashes, dashCooldownTimer, dashCooldownDuration);
                debugStringBuilder.AppendFormat("Velocity: {0:0.0}\n", rigidBody.linearVelocity);
                debugStringBuilder.AppendFormat("Move Speed: ({0:0.0},{1:0.0})\n", moveSpeed, groundObjectMomentum);

                debugStringBuilder.AppendFormat("\nStates:\n");
                debugStringBuilder.AppendFormat("Facing Right: {0}\n", isFacingRight);
                debugStringBuilder.AppendFormat("Invincible: {0} ({1:0.0})\n", isInvincible, invincibilityTime);
                debugStringBuilder.AppendFormat("Stun Locked: {0} ({1:0.0})\n", isStunLocked, stunLockTime);
                debugStringBuilder.AppendFormat("Running: {0}\n", wasRunning);
                debugStringBuilder.AppendFormat("Dashing: {0}\n", isDashing);
                debugStringBuilder.AppendFormat("Wall Sliding: {0}\n", isWallSliding);
                debugStringBuilder.AppendFormat("Fast Dropping: {0}\n", isFastDropping);
                debugStringBuilder.AppendFormat("Coyote Jumping: {0} ({1:0.0} / {2:0.0})\n",canCoyoteJump, coyoteJumpTime,coyoteJumpBuffer);
                debugStringBuilder.AppendFormat("Jumping: {0}\n", isJumping);
                debugStringBuilder.AppendFormat("Fast Falling: {0}\n", isFastFalling);
                debugStringBuilder.AppendFormat("At Max Fall Speed: {0}\n", atMaxFallSpeed);

                debugStringBuilder.AppendFormat("\nCollisions:\n");
                debugStringBuilder.AppendFormat("Grounded: {0}\n", isGrounded);
                debugStringBuilder.AppendFormat("On Platform: {0}\n", isOnPlatform);
                debugStringBuilder.AppendFormat("Touching Wall: {0}\n", isTouchingWall);
                debugStringBuilder.AppendFormat("Wall on Right: {0}\n", isTouchingWallOnRight);
                debugStringBuilder.AppendFormat("Wall on Left: {0}\n", isTouchingWallOnLeft);
                if (groundObjectRigidbody) {
                    debugStringBuilder.AppendFormat("Ground Object: {0} {1:0.0}\n", groundObjectRigidbody.gameObject.name, groundObjectRigidbody?.linearVelocityX);
                }

                debugStringBuilder.AppendFormat("\nInputs:\n");
                debugStringBuilder.AppendFormat($"H/V: {horizontalInput:F2} / {verticalInput:F2}\n");
                debugStringBuilder.AppendFormat("Run: {0}\n", runInput);
                debugStringBuilder.AppendFormat("Jump: {0}  ({1:0.0} / {2:0.0})\n", Input.GetButtonDown("Jump"), variableJumpHeldDuration, variableJumpMaxHoldDuration);
                debugStringBuilder.AppendFormat("Dash: {0}\n", Input.GetButtonDown("Dash"));
                debugStringBuilder.AppendFormat("Drop Down: {0}\n", dropDownInput);

                debugText.text = debugStringBuilder.ToString();
            }
        }
    }

    private readonly StringBuilder fpsStringBuilder = new StringBuilder(256);
    private void UpdateFpsText() {

        if (fpsText) {
            fpsText.enabled = showFpsText;
            if (showFpsText) {  

                fpsStringBuilder.Clear();

                float deltaTime = 0.0f;
                deltaTime += Time.unscaledDeltaTime - deltaTime;
                float fps = 1.0f / deltaTime;

                fpsStringBuilder.AppendFormat("FPS: {0}\n", (int)fps);
                fpsStringBuilder.AppendFormat("VSync: {0}\n", QualitySettings.vSyncCount);

                fpsText.text = fpsStringBuilder.ToString();
            }
        }
    }

    #endif
    #endregion Debugging functions
    
}
