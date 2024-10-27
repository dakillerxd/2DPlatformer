using UnityEngine;

public class RespawnTrigger2D : MonoBehaviour
{
    
    public Vector2 respawnTarget;
    public Vector2 offset;
    public string tagToCheck = "Player";
    public bool resetSpeed = true;


   void OnTriggerEnter(Collider other)
   {
        if(other.CompareTag(tagToCheck))
        {
            other.transform.position = respawnTarget + offset;

            if(resetSpeed)
            {
                other.GetComponent<Rigidbody>().linearVelocity = Vector2.zero;
            }
        }
   }
}
