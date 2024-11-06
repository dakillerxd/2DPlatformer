
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundary2D : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private bool useColliderAsBoundary;
    [HideIf("useColliderAsBoundary")]
    [SerializeField] private float minXAreaBoundary;
    [SerializeField] private float maxXAreaBoundary;
    [SerializeField] private float minYAreaBoundary;
    [SerializeField] private float maxYAreaBoundary;
    [EndIf]
    [Header("References")]
    [SerializeField] private  BoxCollider2D boxCollider2D;
    
    


    private void Start()
    {
        if (boxCollider2D == null)
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
        }
    }

    public Vector4 GetBoundaries()
    {
        if (useColliderAsBoundary)
        {
            return  new Vector4(boxCollider2D.bounds.min.x, boxCollider2D.bounds.max.x, boxCollider2D.bounds.min.y, boxCollider2D.bounds.max.y);
        }
        return  new Vector4(minXAreaBoundary, maxXAreaBoundary, minYAreaBoundary, maxYAreaBoundary);
    }
    
    
    

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (useColliderAsBoundary) MatchBoundaryToCollider();
    }
    
    

    [Button] private void MatchColliderToBoundary() 
    {
        float height = Mathf.Abs(maxYAreaBoundary - minYAreaBoundary);
        float width = Mathf.Abs(maxXAreaBoundary - minXAreaBoundary);
        boxCollider2D.size = new Vector2(width, height);
        transform.position = new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2, 0);
    }

    [Button] private void MatchBoundaryToCollider()
    {
        minXAreaBoundary = boxCollider2D.bounds.min.x;
        maxXAreaBoundary = boxCollider2D.bounds.max.x;
        minYAreaBoundary = boxCollider2D.bounds.min.y;
        maxYAreaBoundary = boxCollider2D.bounds.max.y;
    }

    [Button] private void ResetBoundary()
    {
        minXAreaBoundary = 0;
        maxXAreaBoundary = 0;
        minYAreaBoundary = 0;
        maxYAreaBoundary = 0;
    }

    [Button] private void DeleteBoundary()
    {
        DestroyImmediate(this.gameObject);
    }
    
    
    private void OnDrawGizmos() {
        
        if (useColliderAsBoundary) return;
        // Draw boundary if not using collider size
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Left line
        Debug.DrawLine(new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Right line
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), Color.blue); // Bottom line
        Debug.DrawLine(new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Top line
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 10;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperCenter;
        
        // Draw collider info
        if (boxCollider2D)
        {
            Gizmos.DrawSphere(boxCollider2D.bounds.center, 0.5f);
            if (useColliderAsBoundary)
            {
                Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 2, 0), "Camera Collider&Boundary", style);
            }
            else
            {
                Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 2, 0), "Player Collider", style);
                Debug.DrawLine(boxCollider2D.bounds.center, new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2, 0), Color.red);
            }
            
        }
        
        // Draw boundary info
        if ((minXAreaBoundary != 0 || maxXAreaBoundary != 0 || minYAreaBoundary != 0 || maxYAreaBoundary != 0) && !useColliderAsBoundary)
        {
            
            Handles.Label(new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2 + 2, 0), "Camera Boundary", style);
            Gizmos.DrawSphere(new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2, 0), 0.5f);
        }
        

    }
#endif


}
