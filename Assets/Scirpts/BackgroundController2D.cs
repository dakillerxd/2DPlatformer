using UnityEngine;
using VInspector;

public class BackgroundController : MonoBehaviour
{
    private float startPos;
    [SerializeField] private GameObject cam;
    [SerializeField] private Vector2 parallaxEffect;
    [SerializeField] private int orderInLayer;
    
    
    private void Start() {
        
        // Set starting position
        startPos = transform.position.x;
        
        
        SetChildrenLayer();
    }

    private void LateUpdate() {
        
        MoveBasedOnCamera();
    }

    
    
    private void MoveBasedOnCamera() {
        
        if (!cam) return; 
        
        // Calculate distance move based on cam movement
        Vector2 distance = cam.transform.position * parallaxEffect;

        // Move based of distance
        transform.position = new Vector3(startPos + distance.x, startPos + distance.y, transform.position.z);
        
    }

    [Button]
    private void SetChildrenLayer() {
        
        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer child in children)
        {
            child.sortingOrder = orderInLayer;
        }
        
    }
}
