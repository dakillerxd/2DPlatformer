using UnityEngine;
using VInspector;
using CustomAttribute;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParallaxLayer : MonoBehaviour
{
    [Header("Layer")] 
    [SerializeField] private int orderInLayer;
    [SerializeField] private SortingLayerField sortingLayer;
    
    [Header("Background")]
    [SerializeField] private bool followCamera;
    
    [Header("Parallax")]
    [SerializeField] [Range(0f, 2f)] private float parallaxEffectX;
    [SerializeField] [Range(0f, 2f)] private float parallaxEffectY;
    
    
    private Vector3 startPos;
    private GameObject cam;
    
    
    private void Start() {
        startPos.x = transform.position.x;
        startPos.y = transform.position.y;
        cam = Camera.main.gameObject;
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
        
        // Calculate distance move based on cam movement
        Vector2 distance = cam.transform.position * new Vector2(parallaxEffectX, parallaxEffectY);

        // Move based of distance
        transform.position = new Vector3(startPos.x + distance.x, startPos.y + distance.y, transform.position.z);
        
    }

    private void FollowCamera()
    {
        if (!cam) return;
        if (!followCamera) return;
        
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }

    
    
    [Button]
    private void SetChildrenLayer() {
        
        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer child in children)
        {
            child.sortingLayerID = sortingLayer;
            child.sortingOrder = orderInLayer;
        }
        
    }
    

}
