using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class DigitalMeter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Color lineColor = Color.red;
    private void OnDrawGizmos() {
        if (!target) return;
        Debug.DrawLine(transform.position, target.transform.position, lineColor);
        Handles.Label(Vector3.Lerp(transform.position, target.transform.position, .5f), Vector3.Distance(transform.position, target.transform.position).ToString());
    }
}