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
    [SerializeField] public bool limitCameraToBoundary;
    [EnableIf(nameof(limitCameraToBoundary))]
    [SerializeField] public bool resetBoundaryOnExit = true;
    [EndIf]
    
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
    [SerializeField] [Min(3f)] public float boundaryZoom = 4f;
    public bool resetZoomOnExit = true; 
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

    public Vector4 GetBoundaries()
    {
        if (useColliderAsBoundary)
        {
            return new Vector4(
                boxCollider2D.bounds.min.x,
                boxCollider2D.bounds.max.x,
                boxCollider2D.bounds.min.y,
                boxCollider2D.bounds.max.y
            );
        }
    
        return new Vector4(boundaryMinX, boundaryMaxX, boundaryMinY, boundaryMaxY);
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
        
        Debug.DrawLine(
            new Vector3(boundaryMinX, boundaryMinY, 0),
            new Vector3(boundaryMinX, boundaryMaxY, 0),
            infoColor
        );
        Debug.DrawLine(
            new Vector3(boundaryMaxX, boundaryMinY, 0),
            new Vector3(boundaryMaxX, boundaryMaxY, 0),
            infoColor
        );
        Debug.DrawLine(
            new Vector3(boundaryMinX, boundaryMinY, 0),
            new Vector3(boundaryMaxX, boundaryMinY, 0),
            infoColor
        );
        Debug.DrawLine(
            new Vector3(boundaryMinX, boundaryMaxY, 0),
            new Vector3(boundaryMaxX, boundaryMaxY, 0),
            infoColor
        );
        
        if (useColliderAsBoundary)
        {
            Handles.Label(
                boxCollider2D.bounds.center + new Vector3(0, 1, 0),
                "Camera Trigger",
                style
            );
        }
        else
        {
            Handles.Label(
                boxCollider2D.bounds.center + new Vector3(0, 2, 0),
                "Camera Trigger",
                style
            );
            
            Vector3 boundaryCenter = new Vector3(
                (boundaryMaxX + boundaryMinX) / 2,
                (boundaryMaxY + boundaryMinY) / 2,
                0
            );
            Debug.DrawLine(boxCollider2D.bounds.center, boundaryCenter, infoColor);
            
            if (boundaryMinX != 0 || boundaryMaxX != 0 || boundaryMinY != 0 || boundaryMaxY != 0)
            {
                Handles.Label(
                    boundaryCenter + new Vector3(0, 1, 0),
                    "Camera Boundary",
                    style
                );
                Gizmos.DrawSphere(boundaryCenter, 0.3f);
            }
        }
    }
    #endif
    #endregion
}
