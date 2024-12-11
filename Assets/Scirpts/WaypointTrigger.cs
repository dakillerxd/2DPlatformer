using System;
using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{
    [SerializeField] private WaypointMovement waypointScript;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!waypointScript || waypointScript.IsMovementEnabled()) return;
        
       
        if (other.CompareTag("Player"))
        {
            waypointScript.EnableMovement();
        }
    }
}
