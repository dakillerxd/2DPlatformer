
using UnityEngine;

public class PlatformParenting2D : MonoBehaviour
{
    [SerializeField] private bool moveObjects = true;
    
    [Header("References")]
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public Collider2D collider;
    
    


    // private void OnTriggerStay(Collider other)
    // {
    //     if (!moveObjects) return;
    //
    //     Rigidbody playerRb = other.gameObject.GetComponentInParent<Rigidbody>();
    //     playerRb.linearVelocity += currentVelocity;
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (!moveObjects) return;
        Debug.Log("Collision with: " + other.gameObject.name);
        other.transform.SetParent(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision exit with: " + other.gameObject.name);
        
        if (other.transform.parent == transform)
        {
            other.transform.SetParent(null);
        }
    }
    
}