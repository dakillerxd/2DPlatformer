using System.Collections;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform teleportLocation;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool _triggered;
    
    
    private void Start()
    {
        spriteRenderer.enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player") ||  !teleportLocation || _triggered) return;
        
        _triggered = true;

        Vector3 XYPosition = new Vector3(teleportLocation.position.x, teleportLocation.position.y + 10f, CameraController.Instance.transform.position.z);
        PlayerController.Instance.transform.position = teleportLocation.position;
        // CameraController.Instance.transform.position = XYPosition;
        
        StartCoroutine(ResetTrigger());
    }


    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(1f);
        _triggered = false;
    }
   
}