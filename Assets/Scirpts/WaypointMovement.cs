using System;
using UnityEngine;
using UnityEditor;
using VInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class WaypointMovement : MonoBehaviour {
    

    [Header("Settings")]
    [SerializeField] public bool enableMovement = true;
    [SerializeField] private bool loopBetweenPoints = true;
    [SerializeField] private bool resetPositionOnPlayerDeath;
    [SerializeField] private bool disableOnPlayerDeath;
    [SerializeField, Min(0)] private float maxSpeed = 2f;
    [SerializeField, Range(0, 100)] private float accelerationPercent = 100f;
    [SerializeField, Range(0, 100)] private float decelerationPercent = 100f;
    [SerializeField, Min(0)] private float decelerationRadius = 1.3f;
    [SerializeField, Range(0, 10)] private float waypointWaitTime;
    [SerializeField] private List<GameObject> waypoints = new List<GameObject>();
    private int currentWaypointIndex;
    private float waypointWaitTimer;
    
    
    [Header("References")]
    [SerializeField] private Rigidbody2D objectRigidBody;
    [SerializeField] private GameObject waypointPrefab;

    private void Start() {
        
        if (!objectRigidBody) { objectRigidBody = GetComponentInChildren<Rigidbody2D>(); } // Find the rigidbody
        if (waypoints.Count == 0) { AddWaypoint(); } // Add a waypoint if there is non
        ResetPosition(); // Move to first waypoint
    }
    

    private void OnEnable()
    {
        if (resetPositionOnPlayerDeath) {
            PlayerController.Instance?.onPlayerDeath.AddListener(OnPlayerDeath);
        }
    }

    private void OnDestroy()
    {
        if (resetPositionOnPlayerDeath)
        {
            PlayerController.Instance?.onPlayerDeath.RemoveListener(OnPlayerDeath); 
        }
    }
    
    
    private void FixedUpdate()
    {


        
        MoveBetweenWaypoints();
    }
    
    
    private void OnPlayerDeath() {
        ResetPosition();
        
        if (disableOnPlayerDeath) { enableMovement = false; }
    }

    public void EnableMovement()
    {
        enableMovement = true;
    }

    public bool IsMovementEnabled()
    {
        return enableMovement;
    }
    
    

    [Button] private void ResetPosition()
    {
        objectRigidBody.transform.position = waypoints[0].transform.position; 
        currentWaypointIndex = 0;
        waypointWaitTimer = waypointWaitTime;
        objectRigidBody.linearVelocity = Vector2.zero;
        
    }
    
    [Button] private void AddWaypoint() {
        
        GameObject waypoint = Instantiate(waypointPrefab, transform.position, Quaternion.identity, transform);
        waypoints.Add(waypoint);
        waypoint.name = "Waypoint " + (waypoints.Count);
        RenameWaypoints();
    
        #if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = waypoint;
        #endif
    }

    [Button] private void RemoveAllWaypoints() {
        
        foreach (GameObject waypoint in waypoints) {
            DestroyImmediate(waypoint);
        }
        waypoints.Clear();
        
    }
    
    
    [Button] private void RenameWaypoints()
    {
        // Remove any null elements from the list
        waypoints.RemoveAll(w => w == null);
    
        // Rename all waypoints to match their current position
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypoints[i].name = "Waypoint " + i;
        }
        
        objectRigidBody.transform.position = waypoints[0].transform.position; // Move to first waypoint
    }
    
    private void MoveBetweenWaypoints() {
        
        if (waypoints.Count <= 1 || enableMovement == false) return;
        
        if (waypointWaitTimer > 0)
        {
            waypointWaitTimer -= Time.fixedDeltaTime;
            objectRigidBody.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].transform.position;
        Vector2 directionToTarget = ((Vector2)(targetPosition - objectRigidBody.transform.position)).normalized;
        float distanceToTarget = Vector2.Distance(objectRigidBody.transform.position, targetPosition);
        
        // Calculate speed based on distance
        float targetSpeed = maxSpeed;
        if (decelerationPercent > 0 && distanceToTarget < decelerationRadius)
        {
            float decelerationFactor = distanceToTarget / decelerationRadius;
            targetSpeed = Mathf.Lerp(0, maxSpeed, decelerationFactor);
        }

        // Calculate desired velocity
        Vector2 desiredVelocity = directionToTarget * targetSpeed;
        
        // Apply acceleration or deceleration based on percentages
        float currentRate = distanceToTarget < decelerationRadius ? 
            decelerationPercent / 100f * 15f : // Base deceleration rate of 15
            accelerationPercent / 100f * 15f;  // Base acceleration rate of 15

        objectRigidBody.linearVelocity = Vector2.Lerp(
            objectRigidBody.linearVelocity, 
            desiredVelocity, 
            currentRate
        );

        // Check if reached waypoint
        if (distanceToTarget < 0.1f)
        {
            objectRigidBody.linearVelocity = Vector2.zero;
            waypointWaitTimer = waypointWaitTime;
            
            if (loopBetweenPoints)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }
            else
            {
                if (currentWaypointIndex >= waypoints.Count - 1)
                {
                    waypoints.Reverse();
                    currentWaypointIndex = 0;
                }
                else
                {
                    currentWaypointIndex++;
                }
            }
        }
    }
    
#if UNITY_EDITOR
    
    private List<GameObject> waypointsToDestroy = new List<GameObject>();
    private HashSet<GameObject> previousWaypoints = new HashSet<GameObject>();

    private void OnValidate()
    {
        // Remove null entries from the waypoints list
        waypoints.RemoveAll(w => w == null);

        // Check for deleted GameObjects
        HashSet<GameObject> currentWaypoints = new HashSet<GameObject>(waypoints);
        
        // Schedule waypoints for destruction (removed from list)
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Waypoint") && !waypoints.Contains(child.gameObject))
            {
                waypointsToDestroy.Add(child.gameObject);
            }
        }
        
        // If we have waypoints to destroy, queue up the cleanup
        if (waypointsToDestroy.Count > 0)
        {
            EditorApplication.delayCall += CleanupWaypoints;
        }

        // Rename remaining waypoints to maintain order
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                waypoints[i].name = "Waypoint " + i;
            }
        }

        previousWaypoints = currentWaypoints;
        
        if (waypoints.Count > 0 && objectRigidBody)
        {
            objectRigidBody.transform.position = waypoints[0].transform.position; // Move to first waypoint
        }
    }

    private void CleanupWaypoints()
    {
        // Make sure we're not in play modes
        if (Application.isPlaying) return;
        
        // Cleanup might be called multiple times, so remove the callback
        EditorApplication.delayCall -= CleanupWaypoints;

        foreach (var waypoint in waypointsToDestroy)
        {
            if (waypoint != null)
            {
                DestroyImmediate(waypoint);
            }
        }
        
        waypointsToDestroy.Clear();
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        // Check for any null elements in the list
        bool hasNullWaypoints = waypoints.Any(w => w == null);
        if (hasNullWaypoints) return;

        // Check if this platform or any waypoint is selected
        bool shouldDraw = false;
        GameObject selectedObject = UnityEditor.Selection.activeGameObject;
    
        if (selectedObject != null)
        {
            // Check if the selected object is the platform or a child of it
            shouldDraw = selectedObject == gameObject || 
                         (selectedObject.transform.parent != null && selectedObject.transform.parent == transform);
        }

        if (!shouldDraw) return;

        Gizmos.color = Color.green;

        // Create a style for the labels
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        float yOffSet = 0.5f;

        // Draw lines between consecutive waypoints
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
        
            // Draw waypoint number slightly above the waypoint position
            Vector3 labelPosition = waypoints[i].transform.position + Vector3.up * yOffSet;
            Handles.Label(labelPosition, i.ToString(), style);
        }

        // Draw the last waypoint number
        Vector3 lastLabelPosition = waypoints[waypoints.Count - 1].transform.position + Vector3.up * yOffSet;
        Handles.Label(lastLabelPosition, (waypoints.Count - 1).ToString(), style);

        if (loopBetweenPoints)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].transform.position, waypoints[0].transform.position);
        }

        // Draw spheres showing deceleration distance
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Semi-transparent yellow
        foreach (var waypoint in waypoints)
        {
            Gizmos.DrawWireSphere(waypoint.transform.position, decelerationRadius);
        }
    }
#endif

}