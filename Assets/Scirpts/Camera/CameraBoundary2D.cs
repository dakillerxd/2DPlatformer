
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundary2D : MonoBehaviour
{

    [SerializeField] private float minXAreaBoundary;
    [SerializeField] private float maxXAreaBoundary;
    [SerializeField] private float minYAreaBoundary;
    [SerializeField] private float maxYAreaBoundary;
    private  BoxCollider2D _collider;
    
    private void Start() {

        SetColliderSize();
    }


    public float GetMinXAreaBoundary() {
        return minXAreaBoundary;
    }
    public float GetMaxXAreaBoundary() {
        return maxXAreaBoundary;
    }
    public float GetMinYAreaBoundary() {
        return minYAreaBoundary;
    }
    public float GetMaxYAreaBoundary() {
        return maxYAreaBoundary;
    }

    
    [Button] private void SetColliderSize() {
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
        float height = Mathf.Abs(maxYAreaBoundary - minYAreaBoundary);
        float width = Mathf.Abs(maxXAreaBoundary - minXAreaBoundary);
        _collider.size = new Vector2(width, height);
    }

    private void OnValidate() {
        SetColliderSize();
    }

    private void OnDrawGizmos() {
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Left line
        Debug.DrawLine(new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Right line
        Debug.DrawLine(new Vector3(minXAreaBoundary, minYAreaBoundary, 0), new Vector3(maxXAreaBoundary, minYAreaBoundary, 0), Color.blue); // Bottom line
        Debug.DrawLine(new Vector3(minXAreaBoundary, maxYAreaBoundary, 0), new Vector3(maxXAreaBoundary, maxYAreaBoundary, 0), Color.blue); // Top line
    }
}
