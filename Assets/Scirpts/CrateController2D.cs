using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class CrateController2D : MonoBehaviour
{
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

    [Header("Gravity")]
    [SerializeField] private float gravityForce = 0.5f;
    [SerializeField] private float fallMultiplier = 4f; // Gravity multiplayer when the enemy is falling
    [SerializeField] public float maxFallSpeed = 20f;
    [HideInInspector] public bool isFastFalling;
    [HideInInspector] public bool atMaxFallSpeed;

    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    [SerializeField] [Range(0, 3f)] private float wallCheckDistance = 0.03f;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D collBody;
    
    [Header("VFX")]
    [SerializeField] private ParticleSystem hurtVfx;
    [SerializeField] private ParticleSystem deathVfx;
    [SerializeField] private ParticleSystem spawnVfx;
    [SerializeField] private ParticleSystem bleedVfx;
    [SerializeField] private ParticleSystem healVfx;
    
    
    private void Start() {
        
        currentHealth = maxHealth;
        isInvincible = false;
        isStunLocked = false;
        stunLockTime = 0f;
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
        
        float acceleration = moveAcceleration;
        if (isGrounded) {acceleration *= groundFriction;} else {acceleration *= airFriction;}

        // // Lerp the player movement
        // float newXVelocity = Mathf.Lerp(rigidBody.linearVelocity.x, speed, acceleration * Time.fixedDeltaTime);
        // rigidBody.linearVelocity = new Vector2(newXVelocity, rigidBody.linearVelocity.y);
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
                

                isFastFalling = false;
            }

            // Check for landing at max speed
            if (atMaxFallSpeed) {
                
                atMaxFallSpeed = false;
                isFastFalling = false;
            }
        }
    }
    
    
    private bool CanMove() {
        return !isStunLocked;
    }
    
    
    
    #endregion Movement functions
    
    //------------------------------------

    #region Collison functions
    
    
    private void CollisionChecks() {

        // Check if the player is grounded
        isGrounded = collBody.IsTouchingLayers(groundLayer);
        
        
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
    

    
    #region Health functions
    
    private void DamageHealth(int damage, bool setInvincible) {

        if (currentHealth > 0 && !isInvincible) {
            
            if (setInvincible) { TurnInvincible();}
            TurnStunLocked();
            currentHealth -= damage;
            Debug.Log("Enemy took " + damage + " damage. Current health: " + currentHealth);
        } 
        CheckIfDead();
    }

    private void Push(Vector2 pushForce) {
        
        rigidBody.linearVelocity = Vector2.zero; // Reset current velocity before applying push
        rigidBody.AddForce(pushForce, ForceMode2D.Impulse); // Apply a consistent impulse force
        float maxPushSpeed = 10f;  // Clamp the resulting velocity to prevent excessive speed
        rigidBody.linearVelocity = Vector2.ClampMagnitude(rigidBody.linearVelocity, maxPushSpeed);
    }
    
    private void CheckIfDead() {

        if (currentHealth <= 0) {
            
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
