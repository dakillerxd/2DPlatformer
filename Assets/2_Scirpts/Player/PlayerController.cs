using VInspector;
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class PlayerController : MonoBehaviour {
    
    public static PlayerController Instance { get; private set; }

    [Tab("Player Settings")] // ----------------------------------------------------------------------
    public PlayerState currentPlayerState = PlayerState.Controllable;
    
    [Header("Health")]
    [SerializeField] public int maxHealth = 2;
    [SerializeField] private bool canTakeFallDamage;
    [ShowIf("canTakeFallDamage")][SerializeField] private int maxFallDamage = 1;[EndIf]
    [SerializeField] [Range(0f, 5f)] private float deathTime;
    private int _currentHealth;
    private int _currentDeaths;
    private float _currentTime;
    private float _invincibilityTime;
    private float _stunLockTime;

    [Header("Movement")] 
    [SerializeField] private bool lookRightOnStart = true;
    [SerializeField] [Min(0.01f)] private float maxMoveSpeed = 6f;
    [SerializeField] [Min(0.01f)] private float maxAirMoveSpeed = 5f;
    [SerializeField] [Min(0.01f)] private float airRunSpeed = 6f;
    [SerializeField] [Min(0.01f)] private float moveAcceleration = 2f; // How fast the player gets to acceleration threshold
    [SerializeField] private float runAcceleration = 3f;     // Slower final acceleration
    [SerializeField] private float accelerationThreshold = 5f;   // Speed at which we switch to slower acceleration
    [SerializeField] [Min(0.01f)] private float groundFriction = 4.5f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)] private float airFriction = 0.2f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)] private float platformFriction = 25f; // The higher the friction there is less resistance
    [SerializeField] [Min(0.01f)] private float movementThreshold = 0.1f;
    [SerializeField] private float runningThreshold = 5.9f; // How fast the player needs to move for running
    [SerializeField] private float movingRigidbodyVelocityDecayRate = 4f; // 0 Keep momentum
    private float _moveSpeed;
    private bool _wasLastRunningState;
    private Vector2 _cachedVelocity;
    private Vector2 _cachedPosition;
    private Vector3 _cachedVector3;

    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float variableJumpMaxHoldDuration = 0.3f; // How long the jump button can be held
    [SerializeField] [Range(0.1f, 1f)] private float variableJumpMultiplier = 0.4f; // Multiplier for jump cut height
    [SerializeField] [Range(0.1f, 1f)] private float holdJumpDownBuffer = 0.2f; // For how long the jump buffer will hold
    [SerializeField] [Range(0, 2f)] private float coyoteJumpBuffer = 0.1f; // For how long the coyote buffer will hold
    private bool _isJumpCut;
    private float _holdJumpDownTimer;
    private bool _canCoyoteJump;
    private float _coyoteJumpTime;
    private float _variableJumpHeldDuration;
    
    [Header("Gravity")] 
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 3f; // Gravity multiplayer when the payer is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [SerializeField] private float fastFallSpeed = 12f; // The speed at which the player bobs after a fall
    [SerializeField] [Min(0)] private float fastFallBopDiminisher = 6f;
    [HideInInspector] public float fallSpeed;
    
    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.02f;
    [SerializeField] [Range(0, 3f)] private float ledgeCheckHorizontalDistance = 0.5f;
    [SerializeField] [Range(0, 3f)] private float ledgeCheckVerticalDistance = 2f;
    [SerializeField] [Range(0, 3f)] private float groundCheckVerticalDistance = 2f;
    private LayerMask _combinedGroundMask;
    private bool _isTouchingWallOnRight;
    private bool _isTouchingWallOnLeft;
    private bool _onGroundObject;
    private bool _isGroundBelow;
    private SoftObject _softObject;
    private Rigidbody2D _movingRigidbody;
    private float _movingRigidbodyLastVelocityX;
    private Vector2 _tempCheckSize;

    
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
    [EndIf] 
    
    

    [Tab("Player Abilities")] // ----------------------------------------------------------------------

    [Header("Double Jump")]
    [SerializeField] public bool doubleJumpAbility = true;
    [SerializeField] [Range(1, 10f)] public int maxAirJumps = 1;
    private int _remainingAirJumps;
    
    [Header("Wall Slide")]
    [SerializeField] public bool wallSlideAbility = true;
    [SerializeField] private float wallSlideAcceleration = 3f;
    [SerializeField] private float maxWallSlideSpeed = 4f;
    [SerializeField] [Range(0, 1f)] private float wallSlideStickTime = 0.2f;
    private float _wallSlideStickTimer;

    [Header("Wall Jump")]
    [SerializeField] public bool wallJumpAbility = true;
    [SerializeField] private bool wallJumpResetsJumps;
    [SerializeField] private float wallJumpVerticalForce = 5f;
    [SerializeField] private float wallJumpHorizontalForce = 4f;
    private int _wallJumpCombo;

    [Header("Dash")]
    [SerializeField] public bool dashAbility = true;
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashPushForce = 10f;
    [SerializeField] private int maxDashes = 1;
    [SerializeField] private float dashCooldownDuration = 1f;
    [SerializeField] [Range(0.1f, 1f)] private float holdDashRequestTime = 0.1f; // For how long the dash buffer will hold
    private int _remainingDashes;
    private float _dashBufferTimer;
    private float _dashCooldownTimer;
    private bool _isDashCooldownRunning;
    
    
    [Tab("References")] // ----------------------------------------------------------------------
    [Header("Components")]
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] private Collider2D collBody;
    [SerializeField] private Collider2D collFeet;
    [SerializeField] public Animator animator;
    [SerializeField] private ShadowCaster2D shadowCaster2D;
    [SerializeField] private GameObject propellerHat;
    [SerializeField] private GameObject normalEye;
    [SerializeField] private GameObject googlyEye;
    [SerializeField] private GameObject curlyMustache;
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
    [SerializeField] private ParticleSystem landVfx;
    [SerializeField] private ParticleSystem landMaxSpeedVfx;
    
    [Header("Colors")]
    [SerializeField] private Color invincibilityColor = new Color(1,1,1,0.5f);
    [SerializeField] private Color deadColor = Color.clear;
    private readonly Color _defaultColor = Color.white;


    [Header("States")] 
    public bool isMoving;
    public bool isJumping { get; private set; }
    public bool isFacingRight { get; private set; }
    public bool isStunLocked { get; private set; }
    public bool isInvincible { get; private set; }
    public bool wasRunning { get; private set; }
    public bool isRunning { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isOnPlatform { get; private set; }
    public bool ledgeOnLeft { get; private set; }
    public bool ledgeOnRight { get; private set; }
    public bool isDashing { get; private set; }
    public bool isFastDropping { get; private set; }
    public bool isFalling { get; private set; }
    public bool isFastFalling { get; private set; }
    public bool atMaxFallSpeed { get; private set; } 
    public bool isWallSliding{ get; private set; } 
    public bool isTeleporting { get; private set; } 
    public static event UnityAction OnPlayerDeath;
    
    [Header("Input")] 
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInputDownRequested;
    private bool _jumpInputUp;
    private bool _jumpInputHeld;
    private bool _dashRequested;
    private bool _runInput;
    private bool _dropDownInput;
    [EndTab]
    
    [Tab("Camera")] // ----------------------------------------------------------------------
    [Header("Offset")]
    [SerializeField] private float baseHorizontalOffset = 1f;
    [SerializeField] private float baseVerticalOffset = 1.5f;
    [SerializeField] private float horizontalMoveDiminisher = 1f;
    [SerializeField] private float verticalMoveDiminisher = 1f;
    
    [Header("Zoom")]
    [SerializeField] private float runningZoomOffset = 2f;
    [SerializeField] private float frozenZoomOffset = -2f;

    [Header("Shake")] 
    [SerializeField] private float fallShakeMagnitude = 0.2f;
    [EndTab]
    
    
    
    [Header("Debug")]  // ---------------------------------------------------------------------
    private string _logText;
    private StringBuilder _debugText = new StringBuilder(256);
    private float _debugUpdateTimer;
    private const float DEBUG_UPDATE_INTERVAL = 0.1f;
    
    
    
    private void Awake() {

        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        _combinedGroundMask = groundLayer | platformLayer;
    }

    private void Start()
    {
        CheckpointManager.Instance?.SetSpawnPoint(transform.position);
        isFacingRight = true;
        _currentHealth = maxHealth;
        FlipPlayer(lookRightOnStart ? "Right" : "Left");
        ToggleAllCosmetics();
        Restart();
    }
    
    private void Update() {
        
        if (!CanPlay()) { return; }
        
        _currentTime += Time.deltaTime;
        
        CheckForInput();
        CheckFaceDirection();
        HandleDropDown();
        JumpChecks();
        DashTimer();
        WallSlideTimer();
    }
    
    
    private void FixedUpdate() {
        
        if (!CanPlay()) { return; }
        
        CollisionChecks();
        HandleGravity();
        HandleMovement();
        HandleStepClimbing();
        HandleFastDrop();
        HandleJump();
        HandleWallSlide();
        HandleWallJump();
        HandleDashing();
    }
    
    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    
    
    #region Movement functions //------------------------------------
    
    
    
    private void HandleMovement() {

        float targetMovingRigidBodyVelocity = CalculateMovingRigidBodyMomentum();
        float baseMoveSpeed = rigidBody.linearVelocity.x - targetMovingRigidBodyVelocity;
    

        _moveSpeed = Mathf.Lerp(baseMoveSpeed, CalculateTargetMoveSpeed(), CalculateAcceleration() * Time.fixedDeltaTime);
        if (_horizontalInput == 0) { 
            _moveSpeed = Mathf.Lerp(_moveSpeed, 0, CalculateFriction() * Time.fixedDeltaTime); 
        }
    
        // Apply movement using cached vector
        _cachedVelocity.Set(_moveSpeed + targetMovingRigidBodyVelocity, rigidBody.linearVelocity.y);
        rigidBody.linearVelocity = _cachedVelocity;
    
        // Update states
        isMoving = Mathf.Abs(_moveSpeed) > movementThreshold;
        isRunning = isGrounded && Mathf.Abs(_moveSpeed) > runningThreshold;
    
        if (isRunning) { 
            wasRunning = true;
        }
        if (Mathf.Abs(_moveSpeed) < runningThreshold) { 
            wasRunning = false; 
        }
    
        // Handle running state changes
        if (isRunning != _wasLastRunningState) {
            if (isRunning) { 
                VFXManager.Instance?.ToggleMotionBlur(true);
                PlayAnimationTrigger("Run");
                VFXManager.Instance?.PlayVfxEffect(peakMoveSpeedVfx, false);
                VFXManager.Instance?.PlayVfxEffect(groundRunVfx, false);
            } else if (wasRunning) {
                VFXManager.Instance?.ToggleMotionBlur(true);
                VFXManager.Instance?.StopVfxEffect(groundRunVfx, false);
                VFXManager.Instance?.StopVfxEffect(peakMoveSpeedVfx, false);
            } else {
                VFXManager.Instance?.ToggleMotionBlur(false);
                VFXManager.Instance?.StopVfxEffect(groundRunVfx, false);
                VFXManager.Instance?.StopVfxEffect(peakMoveSpeedVfx, false);
                PlayAnimationTrigger("StopRunning");
            }
        
            _wasLastRunningState = isRunning;
        }
    }

    private float CalculateTargetMoveSpeed()
    {

        float targetSpeed = _horizontalInput;


        if (isGrounded) {
            targetSpeed *= maxMoveSpeed;

        } else {
            targetSpeed *= wasRunning ? airRunSpeed : maxAirMoveSpeed;
        }
        
        
        if (isWallSliding && _wallSlideStickTimer < wallSlideStickTime) {
            targetSpeed = 0;
        }
        
        return targetSpeed;
    }

    private float CalculateAcceleration()
    {
        // use moveAcceleration until getting to accelerationThreshold then use runAcceleration
        float acceleration = Mathf.Abs(_moveSpeed) > accelerationThreshold ? runAcceleration : moveAcceleration;

        return acceleration;
    }

    
    private float CalculateFriction() {
        
        // If grounded use ground or platform friction else use air friction
        if (isGrounded) {
            return isOnPlatform || isDashing ? platformFriction : groundFriction;
        }

        return airFriction;
        
    }
    
    private float CalculateMovingRigidBodyMomentum() {
        if (!_movingRigidbody) return 0f;
        if (_onGroundObject) return _movingRigidbody.linearVelocityX;
    
        if (Mathf.Abs(_movingRigidbodyLastVelocityX) < 0.01f) {
            _movingRigidbodyLastVelocityX = 0;
            return 0f;
        }
    
        float targetVelocity = Mathf.Lerp(_movingRigidbodyLastVelocityX, 0f, movingRigidbodyVelocityDecayRate * Time.fixedDeltaTime);
        _movingRigidbodyLastVelocityX = targetVelocity;
        return targetVelocity;
    }
    
    
    private void HandleDropDown() { 
        
        if (!isGrounded) return;
        if (!_softObject || !_softObject.enabled) return;

        if (!_dropDownInput) return;
        rigidBody.linearVelocityY = jumpForce/3;
        PlayAnimationTrigger("DropDown");
        _softObject.StartDropDownCooldown();
        _softObject = null;
        
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
                    if (!hitUpper) { rigidBody.position += new Vector2(_horizontalInput * stepWidth, stepHeight); }
                }
            } 
        }
    }
    
    private void HandleFastDrop() {

        if (!canFastDrop || atMaxFallSpeed) return;

        isFastDropping = _verticalInput < 0;

        if (isFastDropping) {

            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y - fastFallAcceleration * Time.fixedDeltaTime);
        }
    }
    
    #endregion Movement functions
    
    #region Jump functions //------------------------------------

    private void HandleJump() {
        
        if (isGrounded) { 
            // Reset jumps and jump cut state
            _remainingAirJumps = maxAirJumps;
            _isJumpCut = false;
            
            // Reset coyote jump
            _canCoyoteJump = true;
            _coyoteJumpTime = coyoteJumpBuffer;
            
            // Only reset isJumping if not actively jumping
            if (rigidBody.linearVelocity.y <= 0) {
                isJumping = false;
            }
        }
        
        // Reset jump state and cut when falling
        if (!isGrounded && rigidBody.linearVelocity.y <= 0) { 
            isJumping = false;
            _isJumpCut = false;
        }
        
        if (_jumpInputDownRequested) { // Jump requested
            
            if (_holdJumpDownTimer > holdJumpDownBuffer)
            {
                // If past jump buffer than don't jump
                _jumpInputDownRequested = false;
                return;
            }

            string jumpDirection;
            if (rigidBody.linearVelocity.x < 0 && _horizontalInput > 0) {
                jumpDirection = "Right";
            } else if (rigidBody.linearVelocity.x > 0 && _horizontalInput < 0) {
                jumpDirection = "Left";
            } else {
                jumpDirection = "None";
            }
            

            if ((isGrounded || _canCoyoteJump) && !isJumping) { // Ground jump
                
                if (_canCoyoteJump && !isGrounded) { _logText = $"Coyote Jumped: {_coyoteJumpTime}";}
                ExecuteJump(0, "None");
                
                // Reset coyote state
                _coyoteJumpTime = 0;
                _canCoyoteJump = false;
                
            } else if (!isGrounded && !wallJumpAbility && !_canCoyoteJump && doubleJumpAbility && _remainingAirJumps > 0) { // Double Jump
                
                ExecuteJump(1, jumpDirection);
                VFXManager.Instance?.PlayVfxEffect(airJumpVfx, true); 
                
            } else if (!isGrounded && wallJumpAbility && !(_isTouchingWallOnLeft || _isTouchingWallOnRight) && !_canCoyoteJump && doubleJumpAbility && _remainingAirJumps > 0) { // Double Jump when wall jump ability
                
                ExecuteJump(1, jumpDirection);
                VFXManager.Instance?.PlayVfxEffect(airJumpVfx, true); 
                
            }
        }
    }
    
    
    
    private void ExecuteJump(int jumpCost, string side) {
        
        VFXManager.Instance?.StopVfxEffect(peakFallSpeedVfx, true);
        PlayAnimationTrigger("Jump");
        
        float randPeach = Random.Range(1f, 1.05f);
        SoundManager.Instance?.PlaySoundFX("Player Jump",randPeach);
        
        // Jump
        if (side == "Right") {
            rigidBody.linearVelocity = new Vector2(jumpForce, jumpForce);
        } else if (side == "Left") {
            rigidBody.linearVelocity = new Vector2(-jumpForce, jumpForce);
        } else if (side == "None") {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
        }
        _jumpInputDownRequested = false;
        _remainingAirJumps -= jumpCost;
        isJumping = true;
        _jumpInputHeld = true;
        _isJumpCut = false;
        _variableJumpHeldDuration = 0;
    }
    
    private void JumpChecks() {

        // Jump buffer timer
        if (_holdJumpDownTimer <= holdJumpDownBuffer) {

            _holdJumpDownTimer += Time.deltaTime;
        }
        
        // Variable jump timer
        if (_jumpInputHeld && _variableJumpHeldDuration <= variableJumpMaxHoldDuration) {

            _variableJumpHeldDuration += Time.deltaTime;
        }
        
        
        if (_jumpInputUp && !_isJumpCut) {   // Only Cut jump height if button is released early in upward motion
            if (isJumping && rigidBody.linearVelocity.y > 0) {
                rigidBody.linearVelocityY *=  variableJumpMultiplier;
                PlayAnimationTrigger("Idle");
                _isJumpCut = true;
                SoundManager.Instance?.FadeSoundOut("Player Jump",fadeTime: 0.2f);
            }
            _jumpInputHeld = false;
            _variableJumpHeldDuration = 0;
        }
        
        // Update coyote time
        if (_canCoyoteJump) {
            if (_coyoteJumpTime > 0) {
                _coyoteJumpTime -= Time.deltaTime;
            } else {
                _canCoyoteJump = false;
            }
        }
    }
    #endregion Jump functions
      
    #region Abilitis functions //------------------------------------
    
    private void HandleDashing() {


        if (!dashAbility && _remainingDashes <= 0) return; // Return if not allowed to dash

        if (_dashRequested) {

            int dashDirection = isFacingRight ? 1 : -1;

            // Play effects
            VFXManager.Instance?.PlayVfxEffect(dashVfx, false);
            VFXManager.Instance?.StopVfxEffect(wallSlideVfx, true);
            VFXManager.Instance?.StopVfxEffect(peakFallSpeedVfx, true);
            PlayAnimationTrigger("Dash");
            SoundManager.Instance?.PlaySoundFX("Player Dash");

            // Dash
            isDashing = true;
            _remainingDashes --;
            TurnInvincible(0.5f);
            TurnStunLocked();
            rigidBody.linearVelocityX += dashForce * dashDirection;
            if (isGrounded) { rigidBody.linearVelocityY += 1f; }
            _dashRequested = false;
            StartCoroutine(DashCooldown());
        }
    }
    private IEnumerator DashCooldown() {

        if (_isDashCooldownRunning) { yield break;} // Exit if already running
        _isDashCooldownRunning = true;

        while (_remainingDashes < maxDashes) {
            _dashCooldownTimer = dashCooldownDuration;

            while (_dashCooldownTimer > 0) {
                _dashCooldownTimer -= Time.deltaTime;
                yield return null;
            }

            _dashCooldownTimer = 0;
            _remainingDashes++;
        }

        _isDashCooldownRunning = false;
    }
    
    private void DashTimer() {

        // Dash buffer timer
        if (_dashBufferTimer <= holdDashRequestTime) {

            _dashBufferTimer += Time.deltaTime;
        }
    }
    

    private void HandleWallSlide() {

        if (!wallSlideAbility) return;
        
        // Check if wall sliding
        isWallSliding = !isGrounded && (_isTouchingWallOnLeft || _isTouchingWallOnRight) && rigidBody.linearVelocity.y < 0;

        // Play wall slide effect
        if (isWallSliding) { VFXManager.Instance?.PlayVfxEffect(wallSlideVfx, false);}
        else { VFXManager.Instance?.StopVfxEffect(wallSlideVfx, false); }
        
        
        
        if (isWallSliding) { 

            // Make the player face the opposite direction from the wall
            if (_isTouchingWallOnLeft && !isFacingRight) { 
                FlipPlayer("Right");
            } else if (_isTouchingWallOnRight && isFacingRight) {
                FlipPlayer("Left");
            }

            // Set slide speed
            float slideSpeed = wallSlideAcceleration;
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

    private void WallSlideTimer()
    {
        if (isWallSliding && _horizontalInput != 0) {
            _wallSlideStickTimer += Time.deltaTime;
        } else {
            _wallSlideStickTimer = 0;
        }
    }
    
    private void HandleWallJump () {

        if (isGrounded) {
            _wallJumpCombo = 0;
        }
        
        if (!wallJumpAbility || isGrounded) return;

        if (_jumpInputDownRequested) {
            if (_holdJumpDownTimer > holdJumpDownBuffer) {
                _jumpInputDownRequested = false;
                return;
            }
            
            if (_isTouchingWallOnRight) { // Wall jump to the left
                ExecuteWallJump("Left");
                
            } else if (_isTouchingWallOnLeft ) { // Wall jump to the right
                ExecuteWallJump("Right");
            }
        }
        
        if (!wallJumpResetsJumps ) return;
        if  (_isTouchingWallOnLeft || _isTouchingWallOnRight) { // Reset jumps
            _remainingAirJumps = maxAirJumps;
            _variableJumpHeldDuration = 0;
        }
    }
    private void ExecuteWallJump(string side) {
        
        // Effects
        VFXManager.Instance?.StopVfxEffect(wallSlideVfx, true);
        // PlayAnimationTrigger("Jump");
        float randPeach = Random.Range(1f, 1.05f);
        SoundManager.Instance?.PlaySoundFX("Player Jump",randPeach);

        // Jump
        if (side == "Right") {
            rigidBody.linearVelocity = new Vector2(wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Right");
        } else if (side == "Left") {
            rigidBody.linearVelocity = new Vector2(-wallJumpHorizontalForce, wallJumpVerticalForce);
            FlipPlayer("Left");
        }
        _logText = "Wall Jumped " + side;
        _jumpInputDownRequested = false;
        _variableJumpHeldDuration = 0;
        isJumping = true;
        
        
        if (_wallJumpCombo > 0) TurnStunLocked(0.1f * _wallJumpCombo); // Stop the player from wall climbing
        _wallJumpCombo += 1;
    }

    #endregion Abilitis functions
    
    #region Gravity function //------------------------------------

    private void HandleGravity() {
        
          if (!isGrounded && !isWallSliding)
          {
              // In air
              HandleAirGravity();
              CheckFallSpeed();
          }
          else
          {
              // On ground
              HandleGroundedGravity();
              VFXManager.Instance?.StopVfxEffect(peakFallSpeedVfx, true);
          }
    }
    private void HandleAirGravity() {
        
        float gravityMultiplier = rigidBody.linearVelocityY > 0 ? 1f : fallMultiplier;
        float appliedGravity = gravityForce * gravityMultiplier * Time.fixedDeltaTime;
        
        _cachedVelocity.Set(rigidBody.linearVelocity.x, 
            Mathf.Max(rigidBody.linearVelocityY - appliedGravity, -maxFallSpeed));
        rigidBody.linearVelocity = _cachedVelocity;
    }
    
    private void CheckFallSpeed() {

        fallSpeed = (rigidBody.linearVelocityY < 0) ? rigidBody.linearVelocityY : 0; // remember fall speed
        isFalling = rigidBody.linearVelocityY < -0.1; // Check if falling
        isFastFalling = rigidBody.linearVelocityY < -fastFallSpeed; // Check if fast falling

        
        atMaxFallSpeed = rigidBody.linearVelocityY <= -maxFallSpeed; // If at max fall speed play effects
        if (atMaxFallSpeed)
        {
            VFXManager.Instance?.PlayVfxEffect(peakFallSpeedVfx, false);
            CameraController.Instance?.ShakeCamera(0.1f, fallShakeMagnitude, 1f, 1f);
        }
        
        if (rigidBody.linearVelocityY <= -maxFallSpeed) { // Cap fall speed
            rigidBody.linearVelocityY = -maxFallSpeed; 
        }
    }
    
    
    private void HandleGroundedGravity()
    {
        
        if (!isFalling || !isGrounded || isDashing) return; // Check if landed
        
        
        if (fallSpeed < -maxFallSpeed)
        {
            //     rigidBody.linearVelocityY = -1 * (fallSpeed/fastFallBopDiminisher); // Bop the player
            PlayAnimationTrigger("LandMaxFall");
            if (canTakeFallDamage) DamageHealth(maxFallDamage, false, "Ground");
            CameraController.Instance?.ShakeCamera(0.2f, 1f * (fallSpeed/fastFallBopDiminisher), 1.8f, 1.8f);
            VFXManager.Instance?.SpawnParticleEffect(landMaxSpeedVfx, transform.position + new Vector3(0.16f, -0.16f, 0), Quaternion.identity);
            VFXManager.Instance?.SpawnParticleEffect(landMaxSpeedVfx, transform.position + new Vector3(-0.16f, -0.16f, 0), Quaternion.AngleAxis(180, Vector3.up));
            SoundManager.Instance?.PlaySoundFX("Player Land",0.8f);
        }
        else if (fallSpeed < -fastFallSpeed)
        {
            PlayAnimationTrigger("LandHighFall");
            CameraController.Instance?.ShakeCamera(0.2f, 1f * (fallSpeed/fastFallBopDiminisher), 1.5f, 1.5f);
            VFXManager.Instance?.SpawnParticleEffect(landVfx, transform.position + new Vector3(0.16f, -0.16f, 0), Quaternion.identity);
            VFXManager.Instance?.SpawnParticleEffect(landVfx, transform.position + new Vector3(-0.16f, -0.16f, 0), Quaternion.AngleAxis(180, Vector3.up));
            SoundManager.Instance?.PlaySoundFX("Player Land",0.85f );
        }
        else if (fallSpeed < -9)
        {
            PlayAnimationTrigger("Land");
            VFXManager.Instance?.SpawnParticleEffect(landVfx, transform.position + new Vector3(0.16f, -0.16f, 0), Quaternion.identity);
            VFXManager.Instance?.SpawnParticleEffect(landVfx, transform.position + new Vector3(-0.16f, -0.16f, 0), Quaternion.AngleAxis(180, Vector3.up));
            SoundManager.Instance?.PlaySoundFX("Player Land",0.9f);
        }
        else if (fallSpeed < -6)
        {
            PlayAnimationTrigger("Land");
            SoundManager.Instance?.PlaySoundFX("Player Land");
            
        }
        else
        {
            SoundManager.Instance?.PlaySoundFX("Player Land");
        }
        
            
        atMaxFallSpeed = false;
        isFastFalling = false;
        isFalling = false;


    }
    
    #endregion Gravity functions 
    
    #region Collision functions //------------------------------------

    private void CollisionChecks() {
        // Cache common values
        Vector2 checkCenter = collFeet.bounds.center;
        Vector2 bottomPoint = checkCenter - new Vector2(0, collFeet.bounds.size.y / 2);
        
        // Calculate check box size
        _tempCheckSize.Set(collFeet.bounds.size.x - 0.04f, collFeet.bounds.size.y - 0.3f);
        
        // Ground checks
        isGrounded = Physics2D.OverlapBox(bottomPoint, _tempCheckSize, 0f, _combinedGroundMask);
        isOnPlatform = Physics2D.OverlapBox(bottomPoint, _tempCheckSize, 0f, platformLayer);

        // Ground ray check when in air
        if (!isGrounded) {
            _isGroundBelow = Physics2D.Raycast(bottomPoint, Vector2.down, groundCheckVerticalDistance, _combinedGroundMask);
        }

        // Wall checks
        _isTouchingWallOnRight = Physics2D.Raycast(collBody.bounds.center, Vector2.right, 
            collBody.bounds.extents.x + wallCheckDistance, _combinedGroundMask);
        _isTouchingWallOnLeft = Physics2D.Raycast(collBody.bounds.center, Vector2.left, 
            collBody.bounds.extents.x + wallCheckDistance, _combinedGroundMask);

        // Ledge checks when grounded
        if (isGrounded) {
            Vector2 rightLedgePos = new Vector2(bottomPoint.x + collFeet.bounds.extents.x + ledgeCheckHorizontalDistance, bottomPoint.y);
            Vector2 leftLedgePos = new Vector2(bottomPoint.x - collFeet.bounds.extents.x - ledgeCheckHorizontalDistance, bottomPoint.y);

            RaycastHit2D hitGroundRight = Physics2D.Raycast(rightLedgePos, Vector2.down, 
                ledgeCheckVerticalDistance, _combinedGroundMask);
            RaycastHit2D hitGroundLeft = Physics2D.Raycast(leftLedgePos, Vector2.down, 
                ledgeCheckVerticalDistance, _combinedGroundMask);

            ledgeOnRight = !hitGroundRight;
            ledgeOnLeft = !hitGroundLeft;
        }

        #if UNITY_EDITOR
        DrawCollisionChecks(bottomPoint);
        #endif
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        
        if (collision.contactCount == 0) return;


        if (collision.gameObject.TryGetComponent(out Rigidbody2D rb)) {

            if (rb.bodyType != RigidbodyType2D.Static)
            {
                _movingRigidbody = rb;
                _onGroundObject = _movingRigidbody != null;
            }
        }
        
        
        if (collision.gameObject.TryGetComponent(out SoftObject so)) {
            _softObject = so;
        }
        

        switch (collision.gameObject.tag) {
            case "Enemy":
                
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                Vector2 enemyNormal = collision.GetContact(0).normal;
                Vector2 enemyPushForce = enemyNormal * 6f;
                Vector2 enemyDashForce = enemyNormal * dashPushForce;
                
                
                // Damage and push the enemy if the player is dashing
                if (isDashing) {
                    _movingRigidbody = null;
                    _movingRigidbodyLastVelocityX = 0;
                    isDashing = false;
                    rigidBody.linearVelocity = Vector2.zero;
                    enemy.Push(-enemyDashForce);
                    Push(enemyPushForce/2);
                    CameraController.Instance?.ShakeCamera(0.3f, 2f, 1f, 1f);
                    return;
                }
                
                // Damage and push the player and enemy
                if (collision.collider == enemy.collHead) {

                    Push(enemyPushForce);
                    
                } else {
                    _movingRigidbody = null;
                    _movingRigidbodyLastVelocityX = 0;
                    Push(enemyPushForce);
                    DamageHealth(1, true, collision.gameObject.name);
                }
                break;

        }
    }

    private void OnCollisionExit2D(Collision2D other) {

        if (other.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            if (rb == _movingRigidbody) {
                _onGroundObject = false;
                _movingRigidbodyLastVelocityX = rb.linearVelocityX;
            }
        }
        
        if (other.gameObject.TryGetComponent(out SoftObject so))
        {
            if (_softObject  == so) {
                _softObject = null;
            }
        }
    }

    
    #endregion Collision functions
    
    #region Health/Checkpoint functions //------------------------------------
    
    [Button] private void RespawnFromCheckpoint() {

        _currentDeaths += 1;
        if (CheckpointManager.Instance.activeCheckpoint) {
            Respawn(CheckpointManager.Instance.activeCheckpoint.transform.position);
            CheckpointManager.Instance.UseCheckpoint();
        } else { RespawnFromTeleporter();}
    }
    [Button] public void RespawnFromTeleporter() {

        _currentDeaths = 0;
        if (CheckpointManager.Instance.startTeleporter) {
            Respawn(CheckpointManager.Instance.startTeleporter.transform.position);
            CheckpointManager.Instance.UseTeleporter();
        } else {
            RespawnFromSpawnPoint();
        }
    }

    [Button] private void RespawnFromSpawnPoint() {
        
        _currentDeaths = 0;
        _currentTime = 0;
        StartCoroutine(VFXManager.Instance?.LerpColorAdjustments(true, 0.5f));
        Respawn(CheckpointManager.Instance.playerSpawnPoint);
    }
    
    
    private void Respawn(Vector2 position) {
        
        SetPlayerState(PlayerState.Frozen);
        Teleport(position, false);
        PlayAnimationTrigger("TeleportOut");
        SoundManager.Instance?.PlaySoundFX("Player Spawn");
        OnPlayerDeath?.Invoke();
    }
    
    private void Restart()
    {
        if (GameManager.Instance.debugMode) {
            RespawnFromSpawnPoint();
        } else {
            RespawnFromCheckpoint();
        }

    }
    
    public void Teleport(Vector2 position, bool keepMomentum) {
        
        _cachedPosition.Set(position.x, position.y);
        transform.position = _cachedPosition;
        shadowCaster2D.castsShadows = true;
        SetSpriteColor(_defaultColor);
        CameraController.Instance.transform.position = new Vector3(position.x, position.y, CameraController.Instance.transform.position.z);
        VFXManager.Instance?.SpawnParticleEffect(spawnVfx, transform.position, spawnVfx.transform.rotation);


        if (!keepMomentum)
        {
            rigidBody.linearVelocity = Vector2.zero;
            isDashing = false;
            wasRunning = false;
            isWallSliding = false;
            VFXManager.Instance?.StopVfxEffect(jumpVfx, true);
            VFXManager.Instance?.StopVfxEffect(dashVfx, true);
            VFXManager.Instance?.StopVfxEffect(wallSlideVfx, true);
            VFXManager.Instance?.StopVfxEffect(bleedVfx, true);
            VFXManager.Instance?.StopVfxEffect(healVfx, true);
            VFXManager.Instance?.StopVfxEffect(hurtVfx, true);
            VFXManager.Instance?.StopVfxEffect(deathVfx, true);
        }
    }

    public void TeleportOutOfLevel()
    {
        SetPlayerState(PlayerState.Teleporting);
        PlayAnimationTrigger("TeleportIn");
        StartCoroutine(VFXManager.Instance?.LerpChromaticAberration(true, 2.5f));
        StartCoroutine(VFXManager.Instance?.LerpLensDistortion(true, 2f));
        SoundManager.Instance?.PlaySoundFX("Teleporter In");
        CameraController.Instance?.ShakeCamera(2f, 2f, 2, 2);
        GameManager.Instance?.SaveCurrentLevelStats(SceneManager.GetActiveScene().name, _currentDeaths, _currentTime);
    }

    private void CheckIfDead(string cause = "")
    {
        if (_currentHealth > 0) return;  // Make sure the player is dead
        
        // Set dead state
        SetPlayerState(PlayerState.Frozen);
        shadowCaster2D.castsShadows = false;
        SetSpriteColor(deadColor);
        VFXManager.Instance?.StopVfxEffect(bleedVfx,true);
        VFXManager.Instance?.SpawnParticleEffect(deathVfx, transform.position, Quaternion.identity);
        SoundManager.Instance?.PlaySoundFX("Player Death");
        StartCoroutine(SetDeadStateFor(deathTime));
        
        _logText = "Death by: " + cause;
    }

    private IEnumerator SetDeadStateFor(float duration = 0) {

        StartCoroutine(VFXManager.Instance?.LerpColorAdjustments(false, 1f));
        yield return new WaitForSeconds(duration);
        Restart();
        
    }


    public void DamageHealth(int damage, bool setInvincible, string cause = "") {
        
        if (damage <= 0) return;
        if (_currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) { TurnInvincible();}
            CameraController.Instance.ShakeCamera(0.2f, 4,3,3);
            TurnStunLocked();
            _currentHealth -= damage;
            VFXManager.Instance?.SpawnParticleEffect(hurtVfx, transform.position, Quaternion.identity);
            PlayAnimationTrigger("Hurt");
            SoundManager.Instance.PlaySoundFX("Player Hurt");
            if (_currentHealth == 1 && _currentHealth < maxHealth) { VFXManager.Instance?.PlayVfxEffect(bleedVfx, false); }
            _logText = "Damaged by: " + cause;
        } 
        CheckIfDead(cause);
    }
    

    public void HealToFullHealth() {
        if (_currentHealth == maxHealth) return;
        VFXManager.Instance?.StopVfxEffect(bleedVfx, true);
        VFXManager.Instance?.SpawnParticleEffect(healVfx, transform.position, Quaternion.identity);
        SoundManager.Instance?.PlaySoundFX("Player Heal");
        _currentHealth = maxHealth;
    }
    
    private void TurnInvincible(float invincibilityDuration = 0.1f) {

        StartCoroutine(Invisible(invincibilityDuration));
    }
    private void TurnVulnerable() {

        if (_currentHealth <= 0) return; // don't set the player back to normal if he died
        
        isInvincible = false;
        isDashing = false;
        SetSpriteColor(_defaultColor);
    }
    private IEnumerator Invisible(float invincibilityDuration) {
        
        isInvincible = true;
        _invincibilityTime = invincibilityDuration;
        SetSpriteColor(invincibilityColor);

        while (isInvincible && _invincibilityTime > 0) {
            _invincibilityTime -= Time.deltaTime;
            yield return null;
        }

        TurnVulnerable();
    }

    #endregion Health/Checkpoint functions
    
    #region Other functions //------------------------------------
    private void CheckForInput() {

        if (CanMove()) { // Only check for input if the player can move

            // Check for horizontal input
            _horizontalInput = InputManager.Movement.x;

            // Check for vertical input
            _verticalInput = InputManager.Movement.y;


            // Check for jump inputs
            _jumpInputHeld = InputManager.JumpIsHeld;
            _jumpInputUp = InputManager.JumpWasReleased;

            if (_verticalInput > -1 && InputManager.JumpWasPressed) {
                _jumpInputDownRequested = true;
                _holdJumpDownTimer = 0f;

                // Only reset jump cut when starting a new jump
                if (isGrounded || _canCoyoteJump || _remainingAirJumps > 0) {
                    _isJumpCut = false;
                }
            }

            // Check for dash input
            if (dashAbility && _remainingDashes > 0 && InputManager.DashWasPressed) {
                _dashRequested = true;
                _dashBufferTimer = 0f;
            }
            
            // Check for drop down input
            _dropDownInput = _verticalInput <= -1 && InputManager.JumpWasPressed;
            
            // Check for restart input
            if (InputManager.RestartWasPressed)
            {
                Restart();
            }

        } else { // Set inputs to 0 if the player cannot move

            _horizontalInput = 0;
            _verticalInput = 0;
        }
    }
    
    public void Push(Vector2 pushForce) {
        
        if (_currentHealth > 0 && !isInvincible) {
            
            // Reset current velocity before applying push
            rigidBody.linearVelocity = Vector2.zero;
            _moveSpeed = 0;
            // Apply a consistent impulse force
            _moveSpeed = pushForce.x;
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
        _stunLockTime = 0f;
    }
    private IEnumerator StuckLock(float stunLockDuration) {
        
        isStunLocked = true;
        _stunLockTime = stunLockDuration;

        while (isStunLocked && _stunLockTime > 0) {
            _stunLockTime -= Time.deltaTime;
            yield return null;
        }

        UnStuckLock();
    }
    
    private void CheckFaceDirection() {

        if (isWallSliding) return; // Only flip the player based on input if he is not wall sliding

        if (!isFacingRight && _horizontalInput > 0) {
            FlipPlayer("Right");
        } else if (isFacingRight && _horizontalInput < 0) {
            FlipPlayer("Left");
        }
    }
    
    private void FlipPlayer(string side) {
    
        if (isFacingRight && side == "Right" || !isFacingRight && side == "Left") return; // Only flip the player if he is not already facing the wanted direction

        VFXManager.Instance?.StopVfxEffect(groundRunVfx, true);
        VFXManager.Instance?.StopVfxEffect(peakMoveSpeedVfx, true);
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
        return currentPlayerState == PlayerState.Controllable && GameManager.Instance.gameState == GameStates.GamePlay;
    }
    public void SetPlayerState(PlayerState state) {
        
        VFXManager.Instance?.StopVfxEffect(groundRunVfx, true);
        VFXManager.Instance?.StopVfxEffect(peakMoveSpeedVfx, true);
        VFXManager.Instance?.StopVfxEffect(jumpVfx, true);
        VFXManager.Instance?.StopVfxEffect(dashVfx, true);
        VFXManager.Instance?.StopVfxEffect(wallSlideVfx, true);
        VFXManager.Instance?.StopVfxEffect(peakFallSpeedVfx, true);
        _isTouchingWallOnLeft = false;
        _isTouchingWallOnRight = false;
        isGrounded = false;
        currentPlayerState = state;
        
        switch (state) {
            case PlayerState.Controllable:
                rigidBody.simulated = true;
                rigidBody.linearVelocity = Vector2.zero;
                _remainingAirJumps = maxAirJumps;
                _remainingDashes = maxDashes;
                _movingRigidbodyLastVelocityX = 0;
                _isDashCooldownRunning = false;
                _jumpInputDownRequested = false;
                _dashRequested = false;
                fallSpeed = 0;
                _moveSpeed = 0;
                wasRunning = false;
                isDashing = false;
                isWallSliding = false;
                isInvincible = false;
                isStunLocked = false;
                isFalling = false;
                isFastFalling = false;
                isJumping = false;
                _canCoyoteJump = false;
                isTeleporting = false;
                _invincibilityTime = 0f;
                _stunLockTime = 0f;
                _currentHealth = maxHealth;
                CameraController.Instance?.StopCameraShake();
                break;
            case PlayerState.Frozen:
                rigidBody.simulated = true;
                rigidBody.linearVelocity = Vector2.zero;
                break;
            case PlayerState.Teleporting:
                rigidBody.simulated = false;
                rigidBody.linearVelocity = Vector2.zero;
                break;
        }
    }


    public PlayerState CheckPlayerState()
    {
        return  currentPlayerState;
    }

    public void ReceiveAbility(PlayerAbilities ability, bool playEffects = true)
    {
        switch (ability)
        {
            case PlayerAbilities.DoubleJump:
                doubleJumpAbility = true;
                break;
            case  PlayerAbilities.WallSlide:
                wallSlideAbility = true;
                break;
            case PlayerAbilities.WallJump:
                wallJumpAbility = true;
                break;
            case PlayerAbilities.Dash:
                dashAbility = true;
                break;
        }
        
        if (!playEffects) return;
        // SetPlayerState(PlayerState.Frozen);
        // PlayAnimation("ReceiveAbility");
        HealToFullHealth();
        VFXManager.Instance?.SpawnParticleEffect(healVfx, transform.position, Quaternion.identity);
        SoundManager.Instance?.PlaySoundFX("Player Receive Ability");
    }

    public void ToggleAllCosmetics()
    {
        ToggleCosmetic("Propeller Hat", GameManager.Instance.CheckUnlockActive("Propeller Hat"));
        ToggleCosmetic("Curly Mustache", GameManager.Instance.CheckUnlockActive("Curly Mustache"));
        ToggleCosmetic("Googly Eye", GameManager.Instance.CheckUnlockActive("Googly Eye"));
    }

    public void ToggleCosmetic(string cosmetic, bool state)
    {
        switch (cosmetic)
        {
            case "Propeller Hat":
                propellerHat.SetActive(state);
                break;
            case "Curly Mustache":
                curlyMustache.SetActive(state);
                break;
            case "Googly Eye":
                normalEye.SetActive(!state);
                googlyEye.SetActive(state);
                break;
        }
        
        if (propellerHat || curlyMustache)
        {
            googlyEye.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        }
    }
    
    
    private void SetSpriteColor(Color color) {
        
        foreach (SpriteRenderer sr in spriteRenderers) {
            sr.color = color;
        }
    }
    
    public void PlayAnimationTrigger(string trigger) {
        
        if (!animator) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(trigger)) return;
        animator.SetTrigger(trigger);
    }
    
    public void SetAnimationBool(string boolName, bool state) {
        
        if (!animator) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(boolName)) return;
        animator.SetBool(boolName, state);
    }
    
    #endregion Other functions

    #region Camera functions //------------------------------------

    
    private void UpdateCameraPosition()
    {
        if (!CameraController.Instance) return;
    
        if (currentPlayerState != PlayerState.Controllable)
        {
            CameraController.Instance.SetDynamicOffset(Vector3.zero);
            CameraController.Instance.SetDynamicZoom(frozenZoomOffset);
            return;
        }
    
        Vector3 offset = Vector3.zero;

        // Horizontal Offset
        if (isFastFalling) {
            offset.x = 0;
        } else if (wasRunning) {
            offset.x = isFacingRight
                ? baseHorizontalOffset + rigidBody.linearVelocityX
                : -baseHorizontalOffset + rigidBody.linearVelocityX;
        } else {
            offset.x = isFacingRight
                ? baseHorizontalOffset + rigidBody.linearVelocityX / horizontalMoveDiminisher
                : -baseHorizontalOffset + rigidBody.linearVelocityX / horizontalMoveDiminisher;
        }

        // Vertical Offset
        if (isGrounded || isJumping || _isGroundBelow) {
            offset.y = baseVerticalOffset;
        } else if (isFastFalling || isWallSliding) {
            offset.y = -baseVerticalOffset + rigidBody.linearVelocityY / verticalMoveDiminisher;
        } else if (isFalling) {
            offset.y = -baseVerticalOffset + rigidBody.linearVelocityY / verticalMoveDiminisher;
        }

        // Zoom
        float zoomOffset = 0;
        if (wasRunning) {
            zoomOffset += runningZoomOffset;
        }

        // Update Camera
        CameraController.Instance.SetDynamicOffset(offset);
        CameraController.Instance.SetDynamicZoom(zoomOffset);
    }

    #endregion
    
    #region Debugging functions //------------------------------------

    public void UpdateDebugText(TextMeshProUGUI textObject) {
        if (!textObject || !textObject.isActiveAndEnabled) return;
        
        _debugUpdateTimer -= Time.deltaTime;
        if (_debugUpdateTimer <= 0) {
            _debugUpdateTimer = DEBUG_UPDATE_INTERVAL;
            
            _debugText.Clear()
                .AppendLine("\n<color=#00FF00>Player</color>")
                .AppendLine($"HP: {_currentHealth}/{maxHealth}")
                .AppendLine($"Deaths: {_currentDeaths}")
                .AppendLine($"Time: {_currentTime}")
                .AppendLine($"Velocity: X: {rigidBody.linearVelocity.x:F1} Y: {rigidBody.linearVelocity.y:F1}")
                .AppendLine($"Speed: Move: {_moveSpeed:F1} Fall: {fallSpeed:F1}");

            if (doubleJumpAbility) {
                _debugText.AppendLine($"Air Jumps: {_remainingAirJumps}/{maxAirJumps}");
            }
            
            if (dashAbility) {
                _debugText.AppendLine($"Dashes: {_remainingDashes}/{maxDashes}, {_dashCooldownTimer:F1}s/{dashCooldownDuration:F1}s");
            }

            _debugText.AppendLine("\n<color=#00FF00>Movement</color>");
            AppendState("Running", isRunning);
            AppendState("Was Running", wasRunning);
            AppendState("Jumping", isJumping);
            AppendState("Wall Sliding", isWallSliding);
            AppendState("Dashing", isDashing);
            AppendState("Fast Dropping", isFastDropping);
            AppendState("Fast Falling", isFastFalling);
            AppendState("Max Fall Speed", atMaxFallSpeed);

            _debugText.AppendLine("\n<color=#00FF00>Status</color>");
            AppendStateWithTimer("Facing Right", isFacingRight, null);
            AppendStateWithTimer("Invincible", isInvincible, _invincibilityTime);
            AppendStateWithTimer("Stun Locked", isStunLocked, _stunLockTime);
            AppendStateWithTimer("Coyote Jump", _canCoyoteJump, _coyoteJumpTime);

            _debugText.AppendLine("\n<color=#00FF00>Collisions</color>");
            AppendState("Grounded", isGrounded);
            AppendState("On Platform", isOnPlatform);
            AppendState("Wall Right", _isTouchingWallOnRight);
            AppendState("Wall Left", _isTouchingWallOnLeft);
            AppendState("Ledge Right", ledgeOnRight);
            AppendState("Ledge Left", ledgeOnLeft);

            if (_movingRigidbody != null) {
                _debugText.AppendLine($"Ground Object: {_movingRigidbody.gameObject.name} Vel: {_movingRigidbody.linearVelocityX:F1} Last: {_movingRigidbodyLastVelocityX:F1}");
            }

            if (_softObject != null) {
                _debugText.AppendLine($"Soft Object: {_softObject.gameObject.name}");
            }

            if (!string.IsNullOrEmpty(_logText)) {
                _debugText.AppendLine("\n<color=#00FF00>Log</color>")
                    .AppendLine(_logText);
            }

            textObject.text = _debugText.ToString();
        }
    }

    private void AppendState(string label, bool state) {
        _debugText.AppendLine(state ? $"<color=#00FF00>[+]</color> {label}" : $"<color=#FF0000>[-]</color> {label}");
    }

    private void AppendStateWithTimer(string label, bool state, float? timer) {
        _debugText.Append(state ? "<color=#00FF00>[+]</color> " : "<color=#FF0000>[-]</color> ")
            .Append(label);
        
        if (timer.HasValue) {
            _debugText.Append($" ({timer.Value:F1}s)");
        }
        
        _debugText.AppendLine();
    }
        
        
    #if UNITY_EDITOR
    private void DrawCollisionChecks(Vector2 bottomPoint) {
        // Draw ground check box
        Color groundColor = isGrounded ? Color.green : Color.red;
        DrawDebugBox(bottomPoint, _tempCheckSize, groundColor);
        
        // Draw ground detection ray when in air
        if (!isGrounded) {
            Debug.DrawRay(
                bottomPoint, 
                Vector2.down * groundCheckVerticalDistance, 
                _isGroundBelow ? Color.yellow : Color.red
            );
        }

        // Draw wall check rays
        Debug.DrawRay(
            collBody.bounds.center, 
            Vector2.right * (collBody.bounds.extents.x + wallCheckDistance), 
            _isTouchingWallOnRight ? Color.green : Color.red
        );
        Debug.DrawRay(
            collBody.bounds.center, 
            Vector2.left * (collBody.bounds.extents.x + wallCheckDistance), 
            _isTouchingWallOnLeft ? Color.green : Color.red
        );

        // Draw ledge check rays when grounded
        if (isGrounded) {
            Vector2 rightLedgePos = new Vector2(bottomPoint.x + collFeet.bounds.extents.x + ledgeCheckHorizontalDistance, bottomPoint.y);
            Vector2 leftLedgePos = new Vector2(bottomPoint.x - collFeet.bounds.extents.x - ledgeCheckHorizontalDistance, bottomPoint.y);

            Debug.DrawRay(
                rightLedgePos, 
                Vector2.down * ledgeCheckVerticalDistance, 
                ledgeOnRight ? Color.yellow : Color.blue
            );
            Debug.DrawRay(
                leftLedgePos, 
                Vector2.down * ledgeCheckVerticalDistance, 
                ledgeOnLeft ? Color.yellow : Color.blue
            );
        }
    }

    private void DrawDebugBox(Vector2 position, Vector2 size, Color color) {
        Vector2 halfSize = size * 0.5f;
        Vector2 topLeft = position + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = position + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = position + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = position + new Vector2(halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
    #endif
    
    #endregion Debugging functions


}
