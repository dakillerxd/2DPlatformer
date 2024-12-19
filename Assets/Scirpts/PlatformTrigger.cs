using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlatformTrigger : MonoBehaviour
{
    [SerializeField] private PlatformMovement platformScript;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!platformScript || platformScript.IsMovementEnabled()) return;
        
       
        if (other.CompareTag("Player"))
        {
            if (platformScript.ShouldResetOnTrigger()) { platformScript.ResetPosition();}
            platformScript.EnableMovement();
        }
    }
}
