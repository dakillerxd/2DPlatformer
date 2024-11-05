using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoundsVisualizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color boundsColor = Color.green;
    [SerializeField] private bool showColliderBounds = true;
    [SerializeField] private bool showRendererBounds;
    [SerializeField] private bool showCenterPoint = true;
    [SerializeField] private float centerPointSize = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = boundsColor;

        if (showColliderBounds)
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                if (showCenterPoint)
                {
                    Gizmos.DrawSphere(collider.bounds.center, centerPointSize);
                }
            }
        }

        if (showRendererBounds)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
                if (showCenterPoint)
                {
                    Gizmos.DrawSphere(renderer.bounds.center, centerPointSize);
                }
            }
        }
    }
}