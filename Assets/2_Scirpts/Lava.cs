using System.Collections;
using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private  AudioSource audioSource;
    private bool _triggered;
    
    
    private void Start()
    {
        if (!audioSource) return;
        
        SoundManager.Instance?.PlaySoundFX("Lava Boiling", 0, audioSource);
    }
    
    private void OnEnable()
    {
        if (!audioSource) return;
        
        SettingsManager.Instance?.onSoundFXVolumeChange.AddListener(OnSoundFXVolumeChange);
        SettingsManager.Instance?.onMasterVolumeChange.AddListener(OnMasterVolumeChange);
    }
    
    private void OnDestroy()
    {
        if (!audioSource) return;
        
        SettingsManager.Instance?.onSoundFXVolumeChange.RemoveListener(OnSoundFXVolumeChange);
        SettingsManager.Instance?.onMasterVolumeChange.RemoveListener(OnMasterVolumeChange);
    }
    
    private void OnSoundFXVolumeChange() {
        
        if (!audioSource) return;

        audioSource.volume = SettingsManager.Instance.soundFXVolume * SettingsManager.Instance.masterGameVolume;
        
    }
    
    private void OnMasterVolumeChange()
    {
        if (!audioSource) return;

        audioSource.volume = SettingsManager.Instance.soundFXVolume * SettingsManager.Instance.masterGameVolume;

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
