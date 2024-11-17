using System.Collections;
using UnityEngine;
using VInspector;

public class EnemyController : MonoBehaviour
{
    [Tab("Settings")]
    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    private int currentHealth;
    private bool isInvincible;
    private float invincibilityTime;
    private bool isStunLocked;
    private float stunLockTime;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float airMoveSpeed = 2f;
    [SerializeField] private float moveAcceleration = 2f; // How fast the enemy gets to max speed
    [SerializeField] private float groundFriction = 5f; // The higher the friction there is less resistance
    [SerializeField] private float airFriction = 1f; // The higher the friction there is less resistance
    private bool isFacingRight;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    
    [Header("Gravity")]
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 4f; // Gravity multiplayer when the enemy is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [HideInInspector] public bool isFastFalling;
    [HideInInspector] public bool atMaxFallSpeed;

    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    private bool isGrounded;
    private bool isTouchingGroundOnRight;
    private bool isTouchingGroundOnLeft;
    private bool isTouchingWall;
    private bool isTouchingWallOnRight;
    private bool isTouchingWallOnLeft;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.03f;
    [EndTab]
    
    [Tab("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D  collHead;
    [SerializeField] public Collider2D collBody;
    [SerializeField] public Collider2D collFeet;
    
    [Header("VFX")]
    [SerializeField] private ParticleSystem hurtVfx;
    [SerializeField] private ParticleSystem deathVfx;
    [SerializeField] private ParticleSystem spawnVfx;
    [SerializeField] private ParticleSystem bleedVfx;
    [SerializeField] private ParticleSystem healVfx;

    [Header("SFX")]
    [SerializeField] private AudioSource jumpSfx;
    [SerializeField] private AudioSource spawnSfx;
    [SerializeField] private AudioSource deathSfx;
    [EndTab]
    
    private void Start() {
        
        currentHealth = maxHealth;
        isInvincible = false;
        isStunLocked = false;
        isFacingRight = true;
        stunLockTime = 0f;
    }

    private void Update() {
        
        CheckFaceDirection();
    }

    private void FixedUpdate() {

        CollisionChecks();
        HandleGravity();
        HandleMovement();
    }

    //------------------------------------
    
    #region  Movement functions
    
    private void HandleMovement() {

        if (!CanMove()) return;
        
        float speed = isFacingRight ? 1f : -1f;
        float acceleration = moveAcceleration;

        
        if (isGrounded) { // On Ground
            
            speed *= moveSpeed;
            
        } else if (!isGrounded) { // In air

            speed *= airMoveSpeed;
        } 
        

        // Apply friction
        if (isGrounded) {acceleration *= groundFriction;} else {acceleration *= airFriction;}

        // Lerp the player movement
        float newXVelocity = Mathf.Lerp(rigidBody.linearVelocity.x, speed, acceleration * Time.fixedDeltaTime);
        rigidBody.linearVelocity = new Vector2(newXVelocity, rigidBody.linearVelocity.y);
    }
    
    
    private void HandleGravity() {
        
        if (!isGrounded) // Apply gravity when not grounded
        {
            // Apply gravity and apply the fall multiplier if the player is falling
            float gravityMultiplier = rigidBody.linearVelocity.y > 0 ? 1f : fallMultiplier; 
            rigidBody.linearVelocity = new Vector2 ( rigidBody.linearVelocity.x, Mathf.Lerp(rigidBody.linearVelocity.y, -maxFallSpeed, gravityForce * gravityMultiplier * Time.fixedDeltaTime));
            
            // Cap fall speed
            isFastFalling = rigidBody.linearVelocity.y < -(maxFallSpeed/2);
            if (rigidBody.linearVelocity.y < -maxFallSpeed) {

                atMaxFallSpeed = true;
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, -maxFallSpeed);

            } else { atMaxFallSpeed = false; }
            

        } else { // Apply gravity when grounded

            const float groundGravityForce = 0.1f;
            rigidBody.linearVelocity += groundGravityForce * Time.fixedDeltaTime * Vector2.down;

            // Check for landing at fast falling
            if (isFastFalling) {
                
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce/2); // Make the player bop
                isFastFalling = false;
            }

            // Check for landing at max speed
            if (atMaxFallSpeed) {
                
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce); // Make the player bop
                atMaxFallSpeed = false;
                isFastFalling = false;
            }
        }
    }
    
    
    private bool CanMove() {
        return !isStunLocked;
    }
    
    private void CheckFaceDirection()
    {
        if (!isGrounded) return;
        
        // Check if there is ground and flip
        if (!isFacingRight && !isTouchingGroundOnLeft) {
            FlipEnemy("Right");
        } else if (isFacingRight && !isTouchingGroundOnRight) {
            FlipEnemy("Left");
        }
        
        // Check if there is a wall and flip
        else if (!isFacingRight && isTouchingWallOnLeft) {
            FlipEnemy("Right");
        } else if (isFacingRight && isTouchingWallOnRight) {
            FlipEnemy("Left");
        }
    }
    
    private void FlipEnemy(string side) {
    
        if (isFacingRight && side == "Right") return; 

        if (side == "Left") {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
            
        } else if (side == "Right") {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
    }
    
    #endregion Movement functions
    
    //------------------------------------

    #region Collison functions
    
    
    private void CollisionChecks() {

        // Check if the enemy is grounded
        isGrounded = collFeet.IsTouchingLayers(groundLayer) || collFeet.IsTouchingLayers(platformLayer);
        
        if (isGrounded) {
            if (isFacingRight) {
                RaycastHit2D hitRight = Physics2D.Raycast(collBody.bounds.max + new Vector3(0.04f,0,0), Vector2.down, 0.3f, groundLayer);
                RaycastHit2D hitRightPlat = Physics2D.Raycast(collBody.bounds.max + new Vector3(0.04f,0,0), Vector2.down, 0.3f, platformLayer);
                Debug.DrawRay(collBody.bounds.max + new Vector3(0.04f,0,0), Vector2.down * 0.3f, Color.red);
                isTouchingGroundOnRight = hitRight || hitRightPlat;
            } else {
                RaycastHit2D hitLeft = Physics2D.Raycast(collBody.bounds.min - new Vector3(0.04f,0,0), Vector2.down, 0.5f, groundLayer);
                RaycastHit2D hitLeftPlat = Physics2D.Raycast(collBody.bounds.min - new Vector3(0.04f,0,0), Vector2.down, 0.5f, platformLayer);
                Debug.DrawRay(collBody.bounds.min - new Vector3(0.04f,0,0), Vector2.down * 0.05f, Color.red);
                isTouchingGroundOnLeft = hitLeft || hitLeftPlat;
            }
        }
        
        // Check if the enemy is touching a wall
        isTouchingWall = collBody.IsTouchingLayers(groundLayer);
        
        if (isTouchingWall) {

            // Check collision with walls on the right
            RaycastHit2D hitRight = Physics2D.Raycast(collBody.bounds.center, Vector2.right, collBody.bounds.extents.x + wallCheckDistance, groundLayer);
            Debug.DrawRay(collBody.bounds.center, Vector2.right * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnRight = hitRight;

            // Check collision with walls on the left
            RaycastHit2D hitLeft = Physics2D.Raycast(collBody.bounds.center, Vector2.left, collBody.bounds.extents.x + wallCheckDistance, groundLayer);
            Debug.DrawRay(collBody.bounds.center, Vector2.left * (collBody.bounds.extents.x + wallCheckDistance), Color.red);
            isTouchingWallOnLeft = hitLeft;
        }

        
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        
        if (collision.contactCount == 0) return;

        switch (collision.gameObject.tag) {

            case "Spike":

                Vector2 spikeNormal = collision.GetContact(0).normal;
                Vector2 spikePushForce = spikeNormal * 5f;
                Push(spikePushForce);
                DamageHealth(1, true);
                break;
        }
    }
    
    #endregion Collision functions
    
    //------------------------------------
    
    #region Sfx/Vfx functions
    private void PlayVfxEffect(ParticleSystem  effect, bool forcePlay = false) {
        
        if (!effect) return;
        if (forcePlay) {effect.Play();} else { if (!effect.isPlaying) { effect.Play(); } }
    }

    private void StopVfxEffect(ParticleSystem effect, bool clear = false) {
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

    private void PlaySfxEffect(AudioSource effect) {
        if (!effect) return;
        effect.Play();
    }
    
    
    #endregion Sfx/Vfx functions
    
    //------------------------------------
    
    #region Health functions
    
    public void DamageHealth(int damage, bool setInvincible) {

        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) { TurnInvincible();}
            TurnStunLocked();
            currentHealth -= damage;
            SpawnVfxEffect(hurtVfx);
            Debug.Log("Enemy took " + damage + " damage. Current health: " + currentHealth);
        } 
        CheckIfDead();
    }

    public void Push(Vector2 pushForce) {
        
        TurnStunLocked(0.3f);
        rigidBody.linearVelocity = Vector2.zero; // Reset current velocity before applying push
        rigidBody.AddForce(pushForce, ForceMode2D.Impulse); // Apply a consistent impulse force
        float maxPushSpeed = 10f;  // Clamp the resulting velocity to prevent excessive speed
        rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity, maxPushSpeed);
    }
    
    private void CheckIfDead() {

        if (currentHealth <= 0) {

            SpawnVfxEffect(deathVfx);
            PlaySfxEffect(deathSfx);
            Destroy(gameObject);
        }
    }
    private void TurnInvincible(float invincibilityDuration = 0.3f) {

        StartCoroutine(Invisible(invincibilityDuration));
    }
    
    private void TurnVulnerable() {

        isInvincible = false;
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
    
    #endregion Health function
}
