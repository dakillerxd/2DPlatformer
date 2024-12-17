using UnityEngine;
using UnityEditor;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraTrigger : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private bool useColliderAsBoundary = true;
    [DisableIf(nameof(useColliderAsBoundary))]
    [SerializeField] private float boundaryMinX;
    [SerializeField] private float boundaryMaxX;
    [SerializeField] private float boundaryMinY;
    [SerializeField] private float boundaryMaxY;
    [EndIf] 
    
    [Header("Movement")] 
    [SerializeField] public bool setCameraStateOnEnter;
    [EnableIf(nameof(setCameraStateOnEnter))]
    [SerializeField] public CameraState cameraStateOnEnter;
    [EndIf]
    
    [SerializeField] public bool setCameraStateOnExit;
    [EnableIf(nameof(setCameraStateOnExit))]
    [SerializeField] public CameraState cameraStateOnExit = CameraState.Free;
    [EndIf]
    
    [Header("Offset")]
    [SerializeField] public bool setCameraOffset;
    [EnableIf(nameof(setCameraOffset))] 
    [SerializeField] public Vector3 offset;
    [EndIf]
    
    [Header("Zoom")]
    [SerializeField] public bool setCameraZoom;
    [EnableIf(nameof(setCameraZoom))]
    [SerializeField] public float zoomOffset = 0f;
    [EndIf]
    
    [Header("References")]
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] public Transform cameraPosition;
    
    private void Start()
    {
        if (!boxCollider2D) {
            boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.transform.root != CameraController.Instance.target) return;
        CameraController.Instance.AddActiveTrigger(this);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.transform.root != CameraController.Instance.target) return;
        CameraController.Instance.RemoveActiveTrigger(this);
    }



#region Debug
#if UNITY_EDITOR
    
    private void OnValidate()
    {
        if (useColliderAsBoundary) MatchBoundaryToCollider();
        
        if (!boxCollider2D) {
            boxCollider2D = GetComponent<BoxCollider2D>();
            boxCollider2D.isTrigger = true;
        }
    }
    
    [Button] 
    private void MatchColliderToBoundary() 
    {
        float height = Mathf.Abs(boundaryMaxY - boundaryMinY);
        float width = Mathf.Abs(boundaryMaxX - boundaryMinX);
        boxCollider2D.size = new Vector2(width, height);
        transform.position = new Vector3(
            (boundaryMaxX + boundaryMinX) / 2,
            (boundaryMaxY + boundaryMinY) / 2,
            0
        );
    }

    [Button] 
    private void MatchBoundaryToCollider()
    {
        boundaryMinX = boxCollider2D.bounds.min.x;
        boundaryMaxX = boxCollider2D.bounds.max.x;
        boundaryMinY = boxCollider2D.bounds.min.y;
        boundaryMaxY = boxCollider2D.bounds.max.y;
    }

    [Button] 
    private void ResetBoundary()
    {
        boundaryMinX = 0;
        boundaryMaxX = 0;
        boundaryMinY = 0;
        boundaryMaxY = 0;
    }

    [Button] 
    private void DeleteBoundary()
    {
        DestroyImmediate(gameObject);
    }
    
    private void OnDrawGizmos() 
    {
        if (!boxCollider2D || Application.isPlaying) return;
        if (useColliderAsBoundary) { MatchBoundaryToCollider(); }
        
        Color infoColor = Color.cyan;
        Gizmos.color = infoColor;
        GUIStyle style = new GUIStyle
        {
            normal = { textColor = infoColor },
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };
        
        // Draw boundary lines
        Vector3[] points = {
            new Vector3(boundaryMinX, boundaryMinY, 0),
            new Vector3(boundaryMinX, boundaryMaxY, 0),
            new Vector3(boundaryMaxX, boundaryMaxY, 0),
            new Vector3(boundaryMaxX, boundaryMinY, 0)
        };
        
        for (int i = 0; i < points.Length; i++)
        {
            Debug.DrawLine(points[i], points[(i + 1) % points.Length], infoColor);
        }
        
        // Draw base trigger name
        DrawTriggerLabel(boxCollider2D.bounds.center + Vector3.up, "Camera Trigger", style);

        // Draw boundary center and connection line if using custom boundaries
        if (!useColliderAsBoundary && (boundaryMinX != 0 || boundaryMaxX != 0 || boundaryMinY != 0 || boundaryMaxY != 0))
        {
            Vector3 boundaryCenter = new Vector3(
                (boundaryMaxX + boundaryMinX) / 2,
                (boundaryMaxY + boundaryMinY) / 2,
                0
            );
            Debug.DrawLine(boxCollider2D.bounds.center, boundaryCenter, infoColor);
            Gizmos.DrawSphere(boundaryCenter, 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!boxCollider2D || Application.isPlaying) return;

        GUIStyle style = new GUIStyle
        {
            normal = { textColor = Color.yellow },
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };

        // Calculate base position for labels
        Vector3 labelBasePosition = boxCollider2D.bounds.center + Vector3.down;
        float labelSpacing = 0.5f;
        int labelCount = 1; // Start at 1 since the trigger name is at 0

        // Draw State information
        if (setCameraStateOnEnter)
        {
            DrawTriggerLabel(labelBasePosition + Vector3.up * (labelSpacing * labelCount++), 
                $"Enter State: {cameraStateOnEnter}", style);
        }

        if (setCameraStateOnExit)
        {
            DrawTriggerLabel(labelBasePosition + Vector3.up * (labelSpacing * labelCount++), 
                $"Exit State: {cameraStateOnExit}", style);
        }

        // Draw Offset information
        if (setCameraOffset)
        {
            DrawTriggerLabel(labelBasePosition + Vector3.up * (labelSpacing * labelCount++), 
                $"Offset: ({offset.x:0.0}, {offset.y:0.0})", style);
        }

        // Draw Zoom information
        if (setCameraZoom)
        {
            DrawTriggerLabel(labelBasePosition + Vector3.up * (labelSpacing * labelCount++), 
                $"Zoom: {(zoomOffset >= 0 ? "+" : "")}{zoomOffset:0.0}", style);
        }

        // Draw Boundary information
        string boundaryInfo = useColliderAsBoundary ? "Using Collider Bounds" : "Custom Bounds";
        DrawTriggerLabel(labelBasePosition + Vector3.up * (labelSpacing * labelCount), 
            $"{boundaryInfo}", style);
    }

    private void DrawTriggerLabel(Vector3 position, string text, GUIStyle style)
    {
        Handles.Label(position, text, style);
    }
    
    
#endif
#endregion
}