using System.Collections;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    
    private bool _triggered;
    
   private void OnTriggerEnter2D(Collider2D other)
   {
        if(!other.CompareTag("Player") || _triggered) return;
        
        _triggered = true;
        SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
        PlayerController.Instance?.RespawnFromCheckpoint();
        StartCoroutine(ResetTrigger());
   }


   private IEnumerator ResetTrigger()
   {
       yield return new WaitForSeconds(1f);
       _triggered = false;
   }
   
}
