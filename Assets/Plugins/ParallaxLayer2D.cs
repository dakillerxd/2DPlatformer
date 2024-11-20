using UnityEngine;
using CustomAttribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParallaxLayer2D : MonoBehaviour
{
    [Header("Layer")] 
    [SerializeField] private int orderInLayer;
    [SerializeField] private SortingLayerField sortingLayer;
    
    [Header("Background")]
    [SerializeField] private bool followCamera;
    
    [Header("Parallax")]
    [SerializeField] [Range(-2f, 2f)] private float parallaxEffectX;
    [SerializeField] [Range(-2f, 2f)] private float parallaxEffectY;
    [Tooltip("How far the object from the camera needs to be for the effect to happen")]
    [SerializeField] private float parallaxRange = 20f;
    
    private Transform[] children;
    private Vector3[] initialLocalPositions;  // Store initial local positions
    private Vector3[] childrenOffsets;
    private GameObject cam;
    private Vector3 lastCameraPos;
    
    private void Start() {
        children = GetComponentsInChildren<Transform>();
        initialLocalPositions = new Vector3[children.Length];
        childrenOffsets = new Vector3[children.Length];
        
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
    
    private void MoveBasedOnCamera() {
        if (!cam) return; 
        if (parallaxEffectY == 0 && parallaxEffectX == 0) return;
        if (followCamera) return;
        
        Vector3 cameraDelta = cam.transform.position - lastCameraPos;
        
        for(int i = 0; i < children.Length; i++) {
            if(children[i] == transform) continue;
            
            float distanceToCamera = Vector2.Distance(children[i].position, cam.transform.position);
            
            if (distanceToCamera <= parallaxRange) {
                float influence = 1f - (distanceToCamera / parallaxRange);
                influence = Mathf.Clamp01(influence);
                
                // Calculate the new offset for this frame
                Vector3 frameOffset = new Vector3(
                    cameraDelta.x * parallaxEffectX * influence,
                    cameraDelta.y * parallaxEffectY * influence,
                    0
                );
                
                // Add to accumulated offset
                childrenOffsets[i] += frameOffset;
                
                // Apply offset to the initial position
                children[i].localPosition = initialLocalPositions[i] + childrenOffsets[i];
            }
        }
        
        lastCameraPos = cam.transform.position;
    }

    private void FollowCamera() {
        if (!cam) return;
        if (!followCamera) return;
        
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
    
    private void SetChildrenLayer() {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer child in spriteRenderers) {
            child.sortingLayerID = sortingLayer;
            child.sortingOrder = orderInLayer;
        }
    }


#if UNITY_EDITOR
    private void OnValidate() {
        SetChildrenLayer();
    }
    
    private void OnDrawGizmosSelected() {
        if(children != null) {
            Gizmos.color = Color.yellow;
            foreach(Transform child in children) {
                if(child != transform) {
                    Gizmos.DrawWireSphere(child.position, parallaxRange);
                }
            }
        }
    }
#endif
    


}
