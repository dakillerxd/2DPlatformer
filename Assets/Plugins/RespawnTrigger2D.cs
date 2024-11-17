using UnityEngine;
using System.Collections;

public class RespawnTrigger2D : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private string  playerTag = "Player";
    [SerializeField] private Transform respawnPointTransform;
    private bool _triggered;
    
    
   private void OnTriggerEnter2D(Collider2D other)
   {
        if(!other.CompareTag(playerTag) || _triggered) return;
        _triggered = true;
        other.transform.position = respawnPointTransform.position;
        StartCoroutine(ResetTrigger());
   }
   
   
   private IEnumerator ResetTrigger()
   {
       yield return new WaitForSeconds(1f);
       _triggered = false;
   }
}
