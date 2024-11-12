using UnityEngine;
using UnityEditor;
using VInspector;

public class BackgroundController : MonoBehaviour
{
    
    [SerializeField] [Range(0f, 2f)] private float parallaxEffectX;
    [SerializeField] [Range(0f, 2f)] private float parallaxEffectY;
    [SerializeField] private int orderInLayer;
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
    }

    
    private void MoveBasedOnCamera() {
        
        if (!cam) return; 
        if (parallaxEffectY == 0 && parallaxEffectX == 0) return;
        
        // Calculate distance move based on cam movement
        Vector2 distance = cam.transform.position * new Vector2(parallaxEffectX, parallaxEffectY);

        // Move based of distance
        transform.position = new Vector3(startPos.x + distance.x, startPos.y + distance.y, transform.position.z);
        
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
