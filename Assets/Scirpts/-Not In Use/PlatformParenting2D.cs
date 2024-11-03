using System;
using UnityEngine;
using VInspector;

public class PlatformParenting2D : MonoBehaviour
{
    [SerializeField] private bool moveObjects = true;
    
    private Transform GetRootParent(Transform transform)
    {
        Transform current = transform;
        while (current.parent != null && current.parent != this.transform)
        {
            current = current.parent;
        }
        return current;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!moveObjects) return;
        
        Transform rootParent = GetRootParent(collision.transform);
        if (rootParent.parent != transform) // Only parent if not already parented to this platform
        {
            rootParent.SetParent(transform);
            Debug.Log($"Parented: {rootParent.name} to platform");
        }
        
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        Transform rootParent = GetRootParent(collision.transform);
        if (rootParent.parent == transform)
        {
            rootParent.SetParent(null);
            Debug.Log($"Unparented: {rootParent.name} from platform");
        }
    }
}