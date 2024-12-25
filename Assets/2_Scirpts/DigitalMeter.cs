using UnityEngine;
using VInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DigitalMeter : MonoBehaviour
{
   [Header("Settings")]
   [SerializeField] private Transform target;
   [SerializeField] private bool showXYZComponents = true;
   [SerializeField] private bool showAngle = true;

   
   [Header("Display Settings")]
   [SerializeField] private Color lineColor = Color.red;
   [SerializeField] private Color fontColor = Color.white;
   [SerializeField] private float fontSize = 12f;
   [SerializeField] private float labelOffset = 0.1f;
   [SerializeField] private string units = "m";
   [SerializeField] [Range(0, 10)] private int decimals = 2;
   
#if UNITY_EDITOR
    
   private void OnDrawGizmos()
   {
       if (!target) return;

       Vector3 start = transform.position;
       Vector3 end = target.position;
       float distance = Vector3.Distance(start, end);
       Vector3 direction = (end - start).normalized;
       Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);

       #if UNITY_EDITOR
       // Draw line
       Handles.color = lineColor;
       Handles.DrawLine(start, end);

       // Prepare label style
       GUIStyle style = new GUIStyle();
       style.normal.textColor = fontColor;
       style.fontSize = (int)fontSize;
       style.alignment = TextAnchor.MiddleCenter;
       
       // Create measurement text
       string measurementText = $"{distance.ToString($"F{decimals}")}{units}";
       
       // Add XYZ components
       if (showXYZComponents)
       {
           Vector3 difference = end - start;
           measurementText += $"\nX: {Mathf.Abs(difference.x).ToString($"F{decimals}")}{units}";
           measurementText += $"\nY: {Mathf.Abs(difference.y).ToString($"F{decimals}")}{units}";
           // measurementText += $"\nZ: {Mathf.Abs(difference.z).ToString($"F{decimals}")}{units}";
       }

       // Add angle
       if (showAngle)
       {
           float angle = Vector3.Angle(direction, Vector3.up);
           measurementText += $"\n∠ {angle.ToString($"F{1}")}°";
       }

       // Draw label
       Handles.Label(midPoint + Vector3.up * labelOffset, measurementText, style);
       #endif
   }

   
   private void OnValidate()
   {
       decimals = Mathf.Max(0, decimals);
       fontSize = Mathf.Max(1, fontSize);
       labelOffset = Mathf.Max(0, labelOffset);
   }
   
   #endif
}