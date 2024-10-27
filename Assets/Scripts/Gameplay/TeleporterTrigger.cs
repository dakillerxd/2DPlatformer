using UnityEngine;

public class Teleporter : MonoBehaviour
{
    
    public Transform respawnTarget;
    public Vector3 offset;
    public string tagToCheck = "Player";
    public bool resetSpeed = true;

   void OnTriggerEnter(Collider other)
   {
        if(other.CompareTag(tagToCheck))
        {
                other.transform.position = respawnTarget.position + offset;

                if(resetSpeed)
                {
                    other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                }
        }
   }
}
