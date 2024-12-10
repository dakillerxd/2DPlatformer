using System.Collections;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;
    
    [Header("References")]
    [SerializeField] private Transform teleportLocation;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool _triggered;
    
    private void Start()
    {
        if(hideOnStart) spriteRenderer.enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player") ||  !teleportLocation || _triggered) return;
        
        _triggered = true;

        // Calculate the relative X offset from the trigger's position
        float xOffset = other.transform.position.x - transform.position.x;
        
        // Apply that same offset to the teleport location
        Vector3 newPosition = new Vector3(
            teleportLocation.position.x + xOffset,
            teleportLocation.position.y,
            other.transform.position.z
        );
        
        PlayerController.Instance.transform.position = newPosition;
        
        StartCoroutine(ResetTrigger());
    }

    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(1f);
        _triggered = false;
    }
}