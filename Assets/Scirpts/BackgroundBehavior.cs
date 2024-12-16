using UnityEngine;
using CustomAttributes;
using VInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif







public enum BackgroundType
{
    Static,
    FollowCamera,
    ParallaxLayer,
    ParallaxObject
}

public class BackgroundBehavior : MonoBehaviour
{
    [Header("Background")]
    [Tooltip("Controls how this object moves in relation to the camera:\n" +
             "• Static: Stays fixed in place\n" +
             "• Follow Camera: Moves with camera at offset\n" +
             "• Parallax Layer: All child objects move with parallax\n" +
             "• Parallax Object: This object moves with parallax")]
    [SerializeField] private BackgroundType backgroundType = BackgroundType.ParallaxLayer;
    
    [Tooltip("When enabled, allows you to set custom sorting layer settings for this object and its children")]
    [SerializeField] private bool useCustomLayer = true;
    
    [Tooltip("Determines render order within the sorting layer (higher numbers appear in front)")]
    [SerializeField] private int orderInLayer;
    
    [Tooltip("The Unity sorting layer used to organize sprite rendering order")]
    [SerializeField] private SortingLayerField sortingLayer;
    
    [Header("Parallax Settings")]
    [Tooltip("How much the object moves horizontally relative to camera movement (0-1):\n" +
             "• 0.0 = No movement\n" +
             "• 0.5 = Half camera speed\n" +
             "• 1.0 = Same as camera\n" +
             "Typically: Further objects = lower values (0.1-0.3)\n" +
             "          Closer objects = higher values (0.7-0.9)")]
    [SerializeField] private float maxHorizontalParallax = 0.5f;
    
    [Tooltip("How much the object moves vertically relative to camera movement (0-1):\n" +
             "• 0.0 = No movement\n" +
             "• 0.5 = Half camera speed\n" +
             "• 1.0 = Same as camera\n" +
             "Usually kept similar to horizontal parallax")]
    [SerializeField] private float maxVerticalParallax = 0.5f;
    
    [Tooltip("Distance threshold where parallax movement begins (in units).\n" +
             "Objects closer than this distance won't move.")]
    [SerializeField] [Min(0f)] private float nearestDistance;
    
    [Tooltip("Maximum distance that affects parallax movement (in units).\n" +
             "Objects beyond this distance won't move any further.")]
    [SerializeField] [Min(0f)] private float farthestDistance = 100f;
    
    [Tooltip("Toggle visibility of movement range indicators in the Scene view")]
    [SerializeField] private bool drawGizmos = true;

    [Header("Follow Camera Settings")]
    [Tooltip("Position offset from camera when using Follow Camera mode.\n" +
             "X = Left/Right offset\n" +
             "Y = Up/Down offset\n" +
             "Z = Forward/Back offset")]
    [SerializeField] private Vector3 cameraOffset;

    private Camera _mainCamera;
    private Transform[] _childObjects;
    private Vector3[] _childStartPositions;
    private Vector2[] _childStartPositions2D;
    private float _startPosX;
    private float _startPosY;
    private float _startPosZ;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        _startPosX = transform.position.x;
        _startPosY = transform.position.y;
        _startPosZ = transform.position.z;

        if (backgroundType == BackgroundType.ParallaxLayer)
        {
            _childObjects = new Transform[transform.childCount];
            _childStartPositions = new Vector3[transform.childCount];
            _childStartPositions2D = new Vector2[transform.childCount];
            
            for (int i = 0; i < transform.childCount; i++)
            {
                _childObjects[i] = transform.GetChild(i);
                _childStartPositions[i] = _childObjects[i].position;
                _childStartPositions2D[i] = new Vector2(_childStartPositions[i].x, _childStartPositions[i].y);
            }
        }
        
        SetChildrenLayer();
        SetObjectLayer();
    }

    private void LateUpdate()
    {
        switch (backgroundType)
        {
            case BackgroundType.Static:
                break;

            case BackgroundType.FollowCamera:
                if (_mainCamera == null) return;
                transform.position = new Vector3(_mainCamera.transform.position.x + cameraOffset.x, _mainCamera.transform.position.y + cameraOffset.y,_startPosZ);
                break;

            case BackgroundType.ParallaxLayer:
                HandleParallaxLayer();
                break;

            case BackgroundType.ParallaxObject:
                HandleParallaxObject();
                break;
        }
    }

    private Vector3 CalculatePosition(Vector2 startPos, float zPos)
    {
        // Get camera position in 2D
        Vector2 cameraPos = new Vector2(_mainCamera.transform.position.x, _mainCamera.transform.position.y);
        
        // Calculate direction and distance from start position to camera
        Vector2 toCamera = cameraPos - startPos;
        float distanceToCamera = toCamera.magnitude;
        Vector2 direction = toCamera.normalized;

        // Calculate movement distance
        float effect = Mathf.InverseLerp(nearestDistance, farthestDistance, distanceToCamera);
        float moveDistance = Mathf.Min(effect * distanceToCamera, farthestDistance);
        
        // Calculate movement
        Vector2 movement = direction * moveDistance;
        movement.x *= maxHorizontalParallax;
        movement.y *= maxVerticalParallax;

        return new Vector3(
            startPos.x + movement.x,
            startPos.y + movement.y,
            zPos
        );
    }

    private void HandleParallaxLayer()
    {
        if (_childObjects == null || _childObjects.Length == 0)
            return;

        for (int i = 0; i < _childObjects.Length; i++)
        {
            _childObjects[i].position = CalculatePosition(
                _childStartPositions2D[i],
                _childStartPositions[i].z
            );
        }
    }

    private void HandleParallaxObject()
    {
        transform.position = CalculatePosition(
            new Vector2(_startPosX, _startPosY),
            transform.position.z
        );
    }

    // [CustomButton(tooltip:"Apply the layer settings to all child SpriteRenderer components")]
    [Button]
    private void SetChildrenLayer()
    {
        if (!useCustomLayer) return;
        
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer child in spriteRenderers)
        {
            child.sortingLayerID = sortingLayer;
            child.sortingOrder = orderInLayer;
        }
    }

    // [CustomButton(tooltip:"Apply the layer settings to this object's SpriteRenderer component")]
    [Button]
    private void SetObjectLayer()
    {
        if (!useCustomLayer) return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerID = sortingLayer;
            spriteRenderer.sortingOrder = orderInLayer;
        }
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        DrawParallaxGizmos(true);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
        // If this is a child object, and it's selected, draw its parent's gizmos
        if (Selection.activeGameObject != null && 
            Selection.activeGameObject != gameObject && 
            Selection.activeGameObject.transform.IsChildOf(transform))
        {
            DrawParallaxGizmos(false);
        }
    }

    private void DrawParallaxGizmos(bool isParentSelected)
    {
        if (backgroundType != BackgroundType.ParallaxLayer && backgroundType != BackgroundType.ParallaxObject)
            return;

        void DrawBounds(Vector3 center)
        {
            // Draw nearest distance bound (green)
            Gizmos.color = Color.green;
            Vector3 nearSize = new Vector3(
                nearestDistance * 2 * maxHorizontalParallax,
                nearestDistance * 2 * maxVerticalParallax,
                0
            );
            Gizmos.DrawWireCube(center, nearSize);

            // Draw the farthest distance bound (yellow)
            Gizmos.color = Color.yellow;
            Vector3 farSize = new Vector3(
                farthestDistance * 2 * maxHorizontalParallax,
                farthestDistance * 2 * maxVerticalParallax,
                0
            );
            Gizmos.DrawWireCube(center, farSize);
        }

        if (backgroundType == BackgroundType.ParallaxObject)
        {
            Vector3 center = Application.isPlaying 
                ? new Vector3(_startPosX, _startPosY, transform.position.z)
                : transform.position;
            DrawBounds(center);
        }
        else if (Application.isPlaying && _childObjects != null)
        {
            foreach (Vector3 startPos in _childStartPositions)
            {
                DrawBounds(startPos);
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                DrawBounds(child.position);
            }
        }
    }
#endif
}
    
    