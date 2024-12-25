using UnityEngine;
using VInspector;
using CustomAttribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParallaxLayer : MonoBehaviour
{
    [Header("Layer")] 
    [SerializeField] private bool useCustomLayer = true;
    [EnableIf("useCustomLayer")]
    [SerializeField] private int orderInLayer;
    [SerializeField] private SortingLayerField sortingLayer;
    [EndIf]
    
    [Header("Background")]
    [Tooltip("The layer will be directly in the camera view")]
    [SerializeField] private bool followCamera;

    [Header("Parallax")] 
    [SerializeField] private bool useParallaxEffect = true;
    [EnableIf("useParallaxEffect")]
    [SerializeField] private bool applyToParent = false;
    [SerializeField] [Range(-2f, 2f)] private float parallaxEffectX;
    [SerializeField] [Range(-2f, 2f)] private float parallaxEffectY;
    [Tooltip("How far the object from the camera needs to be for the effect to happen")]
    [SerializeField] private float parallaxRange = 20f;
    [SerializeField] private bool useHorizontalLimit = false;
    [SerializeField] private float maxHorizontalOffset = 5f;
    [SerializeField] private bool useVerticalLimit = false;
    [SerializeField] private float maxVerticalOffset = 5f;
    [EndIf]
    
    private Transform[] children;
    private Vector3 initialLocalPosition;  // Store parent's initial position
    private Vector3[] initialLocalPositions;  // Store children's initial positions
    private Vector3 parentOffset;
    private Vector3[] childrenOffsets;
    private GameObject cam;
    private Vector3 lastCameraPos;
    
    private void Start() {
        children = GetComponentsInChildren<Transform>();
        initialLocalPositions = new Vector3[children.Length];
        childrenOffsets = new Vector3[children.Length];
        initialLocalPosition = transform.localPosition;
        
        // Store initial local positions of all children
        for(int i = 0; i < children.Length; i++) {
            if(children[i] != transform) {
                initialLocalPositions[i] = children[i].localPosition;
            }
        }
        
        cam = Camera.main.gameObject;
        lastCameraPos = cam.transform.position;
        SetChildrenLayer();
    }

    private void LateUpdate() {
        MoveBasedOnCamera();
        FollowCamera();
    }

    private Vector3 ApplyMovementLimits(Vector3 offset) {
        if (!useParallaxEffect) return offset;
        
        if (useHorizontalLimit) {
            offset.x = Mathf.Clamp(offset.x, -maxHorizontalOffset, maxHorizontalOffset);
        }
        if (useVerticalLimit) {
            offset.y = Mathf.Clamp(offset.y, -maxVerticalOffset, maxVerticalOffset);
        }
        return offset;
    }
    
    private void MoveBasedOnCamera() {
        if (!cam) return; 
        if (!useParallaxEffect || followCamera) return;
        
        Vector3 cameraDelta = cam.transform.position - lastCameraPos;
        float distanceToCamera;
        float influence;
        Vector3 frameOffset;

        if (applyToParent) {
            // Apply parallax to parent object
            distanceToCamera = Vector2.Distance(transform.position, cam.transform.position);
            
            if (distanceToCamera <= parallaxRange) {
                influence = 1f - (distanceToCamera / parallaxRange);
                influence = Mathf.Clamp01(influence);
                
                frameOffset = new Vector3(
                    cameraDelta.x * parallaxEffectX * influence,
                    cameraDelta.y * parallaxEffectY * influence,
                    0
                );
                
                parentOffset += frameOffset;
                parentOffset = ApplyMovementLimits(parentOffset);
                transform.localPosition = initialLocalPosition + parentOffset;
            }
        } else {
            // Apply parallax to children
            for(int i = 0; i < children.Length; i++) {
                if(children[i] == transform) continue;
                
                distanceToCamera = Vector2.Distance(children[i].position, cam.transform.position);
                
                if (distanceToCamera <= parallaxRange) {
                    influence = 1f - (distanceToCamera / parallaxRange);
                    influence = Mathf.Clamp01(influence);
                    
                    frameOffset = new Vector3(
                        cameraDelta.x * parallaxEffectX * influence,
                        cameraDelta.y * parallaxEffectY * influence,
                        0
                    );
                    
                    childrenOffsets[i] += frameOffset;
                    childrenOffsets[i] = ApplyMovementLimits(childrenOffsets[i]);
                    children[i].localPosition = initialLocalPositions[i] + childrenOffsets[i];
                }
            }
        }
        
        lastCameraPos = cam.transform.position;
    }

    private void FollowCamera() {
        if (!cam) return;
        if (!followCamera) return;

        if (applyToParent) {
            Vector3 worldPos = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y,
                transform.position.z
            );
            transform.position = worldPos;
        } else {
            for(int i = 0; i < children.Length; i++) {
                if(children[i] != transform) {
                    Vector3 worldPos = new Vector3(
                        cam.transform.position.x, 
                        cam.transform.position.y, 
                        children[i].position.z
                    );
                    children[i].position = worldPos;
                }
            }
        }
    }
    
    [Button]
    private void SetChildrenLayer() {
        if(!useCustomLayer) return;
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer child in spriteRenderers) {
            child.sortingLayerID = sortingLayer;
            child.sortingOrder = orderInLayer;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if(!useParallaxEffect) return;
        
        // Draw parallax range
        Gizmos.color = Color.yellow;
        if(applyToParent) {
            Gizmos.DrawWireSphere(transform.position, parallaxRange);
            
            // Draw movement limits if enabled
            if (useHorizontalLimit || useVerticalLimit) {
                Gizmos.color = Color.cyan;
                Vector3 center = transform.position;
                Vector3 size = new Vector3(
                    useHorizontalLimit ? maxHorizontalOffset * 2 : 0.1f,
                    useVerticalLimit ? maxVerticalOffset * 2 : 0.1f,
                    0.1f
                );
                Gizmos.DrawWireCube(center, size);
            }
        } else if(children != null) {
            foreach(Transform child in children) {
                if(child != transform) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(child.position, parallaxRange);
                    
                    // Draw movement limits for each child if enabled
                    if (useHorizontalLimit || useVerticalLimit) {
                        Gizmos.color = Color.yellow;
                        Vector3 size = new Vector3(
                            useHorizontalLimit ? maxHorizontalOffset * 2 : 0.1f,
                            useVerticalLimit ? maxVerticalOffset * 2 : 0.1f,
                            0.1f
                        );
                        Gizmos.DrawWireCube(child.position, size);
                    }
                }
            }
        }
    }
#endif
}