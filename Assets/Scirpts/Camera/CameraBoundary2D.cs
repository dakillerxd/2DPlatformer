
using UnityEditor;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundary2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useColliderAsBoundary;
    [DisableIf(nameof(useColliderAsBoundary))]
    [SerializeField] private float minXAreaBoundary;
    [SerializeField] private float maxXAreaBoundary;
    [SerializeField] private float minYAreaBoundary;
    [SerializeField] private float maxYAreaBoundary;
    [EndIf]
    
    [Header("Zoom")]
    public bool setCameraZoom;
    [EnableIf(nameof(setCameraZoom))]
    [SerializeField] [Min(1f)] private float boundaryZoom = 4f;
    [EndIf]
    
    [Header("Boundary")]
    public bool limitCameraToBoundary;
    
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
        if (!limitCameraToBoundary) return Vector4.zero;
        
        if (useColliderAsBoundary)
        {
            return  new Vector4(boxCollider2D.bounds.min.x, boxCollider2D.bounds.max.x, boxCollider2D.bounds.min.y, boxCollider2D.bounds.max.y);
        }
        return  new Vector4(minXAreaBoundary, maxXAreaBoundary, minYAreaBoundary, maxYAreaBoundary);
    }

    public float GetBoundaryZoom()
    {
        if (!setCameraZoom) return 0;
            return boundaryZoom;
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
        DestroyImmediate(gameObject);
    }
    
    
    private void OnDrawGizmos() {
        
        if (!boxCollider2D) return;
        if (useColliderAsBoundary) { MatchBoundaryToCollider(); }
        
        Color infoColor = Color.cyan;
        Gizmos.color = infoColor;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = infoColor;
        style.fontSize = 10;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperCenter;
        
        
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), infoColor); // Left line
        Debug.DrawLine(new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), infoColor); // Right line
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), infoColor); // Bottom line
        Debug.DrawLine(new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), infoColor); // Top line
        
        // Draw collider info
        if (useColliderAsBoundary)
        {
            Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 1, 0), "Camera Collider&Boundary", style);
        }
        else
        {
            // Draw collider info
            Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 2, 0), "Player Collider", style);
            Debug.DrawLine(boxCollider2D.bounds.center, new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2, 0), infoColor);
            
            
            // Draw boundary info
            if (minXAreaBoundary != 0 || maxXAreaBoundary != 0 || minYAreaBoundary != 0 || maxYAreaBoundary != 0)
            {
                Handles.Label(new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2 + 1, 0), "Camera Boundary", style);
                Gizmos.DrawSphere(new Vector3((maxXAreaBoundary + minXAreaBoundary) / 2, (maxYAreaBoundary + minYAreaBoundary) / 2, 0), 0.3f);
            }

        }

    }
    
#endif


}
