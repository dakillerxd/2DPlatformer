using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    
   private void OnTriggerEnter2D(Collider2D other)
   {
        if(!other.CompareTag("Player")) return;
        SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
        PlayerController.Instance?.RespawnFromCheckpoint();
   }
   
   
}
