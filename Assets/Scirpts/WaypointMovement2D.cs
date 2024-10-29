using UnityEngine;

public class WaypointMovement2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
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
        
        // Calculate desired velocity
        Vector2 desiredVelocity = directionToTarget * maxSpeed;
        
        // Smoothly transition to desired velocity
        rigidBody.linearVelocity = Vector2.Lerp(rigidBody.linearVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

        // Check if reached waypoint
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
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
        }
    }
}