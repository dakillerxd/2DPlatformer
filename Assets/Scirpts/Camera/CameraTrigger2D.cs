
using System;
using UnityEditor;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraTrigger2D : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private bool useColliderAsBoundary = true;
    [DisableIf(nameof(useColliderAsBoundary))]
    [SerializeField] private float boundaryMinX;
    [SerializeField] private float boundaryMaxX;
    [SerializeField] private float boundaryMinY;
    [SerializeField] private float boundaryMaxY;
    [EndIf]
    
    [Header("Offset")]
    public bool setCameraOffset;
    [EnableIf(nameof(setCameraOffset))] 
    [SerializeField] private Vector3 offset;
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
        if (boxCollider2D) return;
        
        boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.transform.root != CameraController2D.Instance.target) return;
        
        if (setCameraZoom) {CameraController2D.Instance.SetCameraTargetZoom(GetBoundaryZoom());}
        if (setCameraOffset) {CameraController2D.Instance.SetTriggerOffset(offset);}
        if (limitCameraToBoundary) {CameraController2D.Instance.SetBoundaries(gameObject.GetComponent<CameraTrigger2D>(), GetBoundaries());}
        
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.transform.root != CameraController2D.Instance.target) return;
        
        if (limitCameraToBoundary) {CameraController2D.Instance.ResetBoundaries();}
        if (setCameraZoom) {CameraController2D.Instance.ResetZoom();}
        if (setCameraOffset) {CameraController2D.Instance.ResetTriggerOffset();}
    }

    private Vector4 GetBoundaries()
    {
        if (!limitCameraToBoundary) return Vector4.zero;
        
        if (useColliderAsBoundary)
        {
            return  new Vector4(boxCollider2D.bounds.min.x, boxCollider2D.bounds.max.x, boxCollider2D.bounds.min.y, boxCollider2D.bounds.max.y);
        }
        return  new Vector4(boundaryMinX, boundaryMaxX, boundaryMinY, boundaryMaxY);
    }

    private float GetBoundaryZoom()
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
        float height = Mathf.Abs(boundaryMaxY - boundaryMinY);
        float width = Mathf.Abs(boundaryMaxX - boundaryMinX);
        boxCollider2D.size = new Vector2(width, height);
        transform.position = new Vector3((boundaryMaxX + boundaryMinX) / 2, (boundaryMaxY + boundaryMinY) / 2, 0);
    }

    [Button] private void MatchBoundaryToCollider()
    {
        boundaryMinX = boxCollider2D.bounds.min.x;
        boundaryMaxX = boxCollider2D.bounds.max.x;
        boundaryMinY = boxCollider2D.bounds.min.y;
        boundaryMaxY = boxCollider2D.bounds.max.y;
    }

    [Button] private void ResetBoundary()
    {
        boundaryMinX = 0;
        boundaryMaxX = 0;
        boundaryMinY = 0;
        boundaryMaxY = 0;
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
        
        
        Debug.DrawLine(new Vector3(boundaryMinX, boundaryMinY, 0), new Vector3(boundaryMinX, boundaryMaxY, 0), infoColor); // Left line
        Debug.DrawLine(new Vector3(boundaryMaxX, boundaryMinY, 0), new Vector3(boundaryMaxX, boundaryMaxY, 0), infoColor); // Right line
        Debug.DrawLine(new Vector3(boundaryMinX, boundaryMinY, 0), new Vector3(boundaryMaxX, boundaryMinY, 0), infoColor); // Bottom line
        Debug.DrawLine(new Vector3(boundaryMinX, boundaryMaxY, 0), new Vector3(boundaryMaxX, boundaryMaxY, 0), infoColor); // Top line
        
        // Draw collider info
        if (useColliderAsBoundary)
        {
            Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 1, 0), "Camera Trigger&Boundary", style);
        }
        else
        {
            // Draw collider info
            Handles.Label(boxCollider2D.bounds.center + new Vector3(0, 2, 0), "Camera Trigger", style);
            Debug.DrawLine(boxCollider2D.bounds.center, new Vector3((boundaryMaxX + boundaryMinX) / 2, (boundaryMaxY + boundaryMinY) / 2, 0), infoColor);
            
            
            // Draw boundary info
            if (boundaryMinX != 0 || boundaryMaxX != 0 || boundaryMinY != 0 || boundaryMaxY != 0)
            {
                Handles.Label(new Vector3((boundaryMaxX + boundaryMinX) / 2, (boundaryMaxY + boundaryMinY) / 2 + 1, 0), "Camera Boundary", style);
                Gizmos.DrawSphere(new Vector3((boundaryMaxX + boundaryMinX) / 2, (boundaryMaxY + boundaryMinY) / 2, 0), 0.3f);
            }

        }

    }
    
#endif


}
