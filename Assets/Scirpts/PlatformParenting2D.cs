using UnityEngine;
using System.Collections.Generic;
using VInspector;

public class PlatformParenting2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool moveObjects = true;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float maxHorizontalVelocity = 5f;
    [SerializeField] private float maxVerticalVelocity = 10f;
    private HashSet<Rigidbody2D> attachedRigidbodies = new HashSet<Rigidbody2D>();
    
    [Header("References")]
    private Rigidbody2D rigidBody;
    

    
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!moveObjects) return;

        foreach(var rb in attachedRigidbodies)
        {
            if (rb != null)
            {
                // Add platform velocity
                rb.linearVelocity += Time.fixedDeltaTime * velocityMultiplier * rigidBody.linearVelocity ;
                
                // Clamp the velocity
                rb.linearVelocity = new Vector2(
                    Mathf.Clamp(rb.linearVelocity.x, -maxHorizontalVelocity, maxHorizontalVelocity),
                    Mathf.Clamp(rb.linearVelocity.y, -maxVerticalVelocity, maxVerticalVelocity)
                );
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!moveObjects) return;
        
        // Try to get a Rigidbody2D from the colliding object
        Rigidbody2D objectRb = other.gameObject.GetComponentInParent<Rigidbody2D>();
        
        // If we found a rigidbody, add it to our tracking set
        if (objectRb != null)
        {
            attachedRigidbodies.Add(objectRb);
        }
        
        Transform rootParent = GetRootParent(other.transform);
        if (rootParent.parent == transform) return;
        rootParent.SetParent(transform);
        
        Debug.Log($"Object entered: {rootParent.name}, Total objects: {attachedRigidbodies.Count}");
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (!moveObjects) return;
        
        // Remove the rigidbody from our tracking
        Rigidbody2D objectRb = other.gameObject.GetComponentInParent<Rigidbody2D>();
        if (objectRb != null)
        {
            attachedRigidbodies.Remove(objectRb);
        }
        
        Transform rootParent = GetRootParent(other.transform);
        if (rootParent.parent == transform)
        {
            rootParent.SetParent(null);
            Debug.Log($"Object exited: {rootParent.name}, Total objects: {attachedRigidbodies.Count}");
        }
    }

    private Transform GetRootParent(Transform transform)
    {
        Transform current = transform;
        while (current.parent != null && current.parent != this.transform)
        {
            current = current.parent;
        }
        return current;
    }
    
    private void OnDisable()
    {
        attachedRigidbodies.Clear();
    }
}