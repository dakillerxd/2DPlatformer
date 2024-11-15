using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    
   private void OnTriggerEnter(Collider other)
   {
        if(!other.CompareTag("Player")) return;
        SoundManager.Instance?.PlaySoundFX("Player Fall off Map");
        PlayerController.Instance?.RespawnFromCheckpoint();
   }
   
   
}
