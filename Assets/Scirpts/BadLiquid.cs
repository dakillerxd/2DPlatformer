using System.Collections;
using UnityEngine;

public class BadLiquid : MonoBehaviour
{
    private bool _triggered;
    
   private void OnTriggerEnter2D(Collider2D other)
   {
        if(!other.CompareTag("Player") || _triggered) return;
        _triggered = true;
        var player = other.gameObject.GetComponentInParent<PlayerController>();
        player.DamageHealth(player.maxHealth,false, "Respawn Trigger");
        SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
        StartCoroutine(ResetTrigger());
   }


   private IEnumerator ResetTrigger()
   {
       yield return new WaitForSeconds(1f);
       _triggered = false;
   }
   
}
