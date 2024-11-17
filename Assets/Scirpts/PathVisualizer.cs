using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathVisualizer : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color pointColor = Color.red;
    [SerializeField] private float pointSize = 0.25f;
    
    [Header("Display Options")]
    [SerializeField] private bool showDistances = true;
    [SerializeField] private bool showTotalDistance = true;
    [SerializeField] private bool connectEndToStart = false;
    [SerializeField] private float fontSize = 12f;

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Length < 2) return;

        float totalDistance = 0f;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null) continue;

            // Draw point
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(pathPoints[i].position, pointSize);

            // Draw line to next point
            if (i < pathPoints.Length - 1 && pathPoints[i + 1] != null)
            {
                DrawPathSegment(pathPoints[i].position, pathPoints[i + 1].position, ref totalDistance);
            }
            else if (connectEndToStart && i == pathPoints.Length - 1 && pathPoints[0] != null)
            {
                DrawPathSegment(pathPoints[i].position, pathPoints[0].position, ref totalDistance);
            }
        }

        if (showTotalDistance)
        {
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = pathColor;
            style.fontSize = (int)fontSize;
            Handles.Label(transform.position + Vector3.up, $"Total Distance: {totalDistance:F2}m", style);
            #endif
        }
    }

    private void DrawPathSegment(Vector3 start, Vector3 end, ref float totalDistance)
    {
        Gizmos.color = pathColor;
        Gizmos.DrawLine(start, end);

        float distance = Vector3.Distance(start, end);
        totalDistance += distance;

        if (showDistances)
        {
            #if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = pathColor;
            style.fontSize = (int)fontSize;
            Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
            Handles.Label(midPoint, $"{distance:F2}m", style);
            #endif
        }
    }
}