using UnityEngine;

public class RespawnTrigger2D : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private string  playerTag = "Player";
    [SerializeField] private Transform respawnPointTransform;
    
    
   private void OnTriggerEnter2D(Collider2D other)
   {
        if(!other.CompareTag(playerTag)) return;
        
        other.transform.position = respawnPointTransform.position;
   }
}
