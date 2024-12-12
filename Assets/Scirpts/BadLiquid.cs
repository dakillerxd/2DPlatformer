using System.Collections;
using UnityEngine;

public class BadLiquid : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private  AudioSource audioSource;
    private bool _triggered;
    
    
    private void Start()
    {
        if (audioSource) SoundManager.Instance?.PlaySoundFX("Lava Boiling", 0 , audioSource);
        
    }
   
   private void OnCollisionEnter2D(Collision2D collision)
   {
       if(_triggered) return;

       if (collision.gameObject.CompareTag("Player"))
       {
           _triggered = true;
           var player = collision.gameObject.GetComponentInParent<PlayerController>();
           player.DamageHealth(player.maxHealth,false, "Bad Liquid");
           SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
           StartCoroutine(ResetTrigger());
           
       } else if (collision.gameObject.CompareTag("Enemy")) {
           var enemy = collision.gameObject.GetComponentInParent<EnemyController>();
           enemy.DamageHealth(enemy.maxHealth, false);
           StartCoroutine(ResetTrigger());
       }

   }


   private IEnumerator ResetTrigger()
   {
       yield return new WaitForSeconds(1f);
       _triggered = false;
   }
   
}
