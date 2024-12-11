using System.Collections;
using UnityEngine;

public class BadLiquid : MonoBehaviour
{
    private bool _triggered;
    
   
   private void OnCollisionEnter2D(Collision2D collision)
   {
       if(!collision.gameObject.CompareTag("Player") || _triggered) return;
       
       _triggered = true;
       var player = collision.gameObject.GetComponentInParent<PlayerController>();
       player.DamageHealth(player.maxHealth,false, "Bad Liquid");
       SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
       StartCoroutine(ResetTrigger());
   }


   private IEnumerator ResetTrigger()
   {
       yield return new WaitForSeconds(1f);
       _triggered = false;
   }
   
}
