using UnityEngine;

public class WaypointMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTime = 0.5f;

    private Vector3 currentVelocity;
    private Vector3 previousPosition;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;

    private void Start()
    {
        if (waypoints.Length == 0)
        {
            waypoints = new Vector3[]
            {
                transform.position,
                transform.position + Vector3.right * 5f
            };
        }

        transform.position = waypoints[0];
        previousPosition = transform.position;
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
            currentVelocity = Vector3.zero;
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);
        
        currentVelocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
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