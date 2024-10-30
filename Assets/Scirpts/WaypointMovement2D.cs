using UnityEngine;

public class WaypointMovement2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField, Range(0, 100)] private float accelerationPercent = 100f;
    [SerializeField, Range(0, 100)] private float decelerationPercent = 100f;
    [SerializeField] private float decelerationDistance = 1f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTime = 0.5f;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    
    [Header("References")]
    private Rigidbody2D rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        if (waypoints.Length == 0)
        {
            waypoints = new Vector3[]
            {
                transform.position,
                transform.position + Vector3.right * 5f
            };
        }

        transform.position = waypoints[0];
    }

    private void FixedUpdate()
    {
        MoveBetweenWaypoints();
    }

    
    
    private void MoveBetweenWaypoints()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            rigidBody.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector2 directionToTarget = ((Vector2)(targetPosition - transform.position)).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        
        // Calculate speed based on distance
        float targetSpeed = maxSpeed;
        if (decelerationPercent > 0 && distanceToTarget < decelerationDistance)
        {
            float decelerationFactor = distanceToTarget / decelerationDistance;
            targetSpeed = Mathf.Lerp(0, maxSpeed, decelerationFactor);
        }

        // Calculate desired velocity
        Vector2 desiredVelocity = directionToTarget * targetSpeed;
        
        // Apply acceleration or deceleration based on percentages
        float currentRate = distanceToTarget < decelerationDistance ? 
            decelerationPercent / 100f * 15f : // Base deceleration rate of 15
            accelerationPercent / 100f * 10f;  // Base acceleration rate of 10

        rigidBody.linearVelocity = Vector2.Lerp(
            rigidBody.linearVelocity, 
            desiredVelocity, 
            currentRate
        );

        // Check if reached waypoint
        if (distanceToTarget < 0.1f)
        {
            rigidBody.linearVelocity = Vector2.zero;
            waitTimer = waitTime;
            
            if (loop)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else
            {
                if (currentWaypointIndex >= waypoints.Length - 1)
                {
                    System.Array.Reverse(waypoints);
                    currentWaypointIndex = 0;
                }
                else
                {
                    currentWaypointIndex++;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i], 0.2f);
            if (i < waypoints.Length - 1)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }
            else if (loop)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[0]);
            }

            // Draw deceleration zones
            if (decelerationPercent > 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(waypoints[i], decelerationDistance);
                Gizmos.color = Color.yellow;
            }
        }
    }
}