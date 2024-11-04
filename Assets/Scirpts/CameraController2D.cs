
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;

public class CameraController2D : MonoBehaviour
{
    public static CameraController2D Instance { get; private set; }


    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] [Range(0f, 2f)] private float smoothFollowSpeed = 0.5f;
    private Vector3 targetPosition;


    [Header("Target Offset")]
    [SerializeField] private bool verticalOffset = true;
    [SerializeField] private bool horizontalOffset = true;
    [SerializeField] [Range(0f, 2f)] private float verticalOffsetStrength = 1f;
    [SerializeField] [Range(0f, 2f)] private float horizontalOffsetStrength = 1f;
    private Vector3 targetOffset;

    [Header("Shake Settings")]
    [HideInInspector] public bool isShaking;
    private Vector3 shakeOffset;

    [Header("Camera Boundaries")]
    [SerializeField] private bool useBoundaries;
    [SerializeField] private float minXLevelBoundary;
    [SerializeField] private float maxXLevelBoundary;
    [SerializeField] private float minYLevelBoundary;
    [SerializeField] private float maxYLevelBoundary;

    [Header("Zoom Settings")]
    [SerializeField] private bool allowZoomControl = true;
    [SerializeField] private float currentZoom = 3f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 5f;



    private Camera cam;
    private Vector3 currentVelocity;
    private float zoomVelocity;


    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

       } else {
            
            Instance = this; 
       }
    }


    private void Start() {
        cam = GetComponent<Camera>();
        target = GameObject.Find("Player").GetComponent<Transform>();
        cam.orthographicSize = currentZoom;
        isShaking = false;
    }

    private void Update()
    {
        HandleZoomInput();
    }


    private void LateUpdate()
    {
        FollowTarget();
        HandleZoom();
        ApplyShake();
        
    }


    #region Target


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FollowTarget()
    {
        if (!target) return;

        targetPosition = CalculateTargetPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);

        if (useBoundaries) {

            smoothedPosition = HandleBoundaries(smoothedPosition);
        }

        transform.position = smoothedPosition;
    }

    #endregion Target

    #region Zoom
    private void HandleZoomInput()
    {
        if (!allowZoomControl) return;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= zoomInput * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }
    private void HandleZoom()
    {
        if (!allowZoomControl) return;

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, currentZoom, ref zoomVelocity, smoothFollowSpeed, Mathf.Infinity, Time.fixedDeltaTime);
    }


    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    #endregion Zoom

    #region Boundaries
    private Vector3 HandleBoundaries(Vector3 position)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minXBoundaryBoundary = minXLevelBoundary + camWidth;
        float maxXBoundaryBoundary = maxXLevelBoundary - camWidth;
        float minYBoundaryBoundary = minYLevelBoundary + camHeight;
        float maxYBoundaryBoundary = maxYLevelBoundary - camHeight;

        float newX = Mathf.Clamp(position.x, minXBoundaryBoundary, maxXBoundaryBoundary);
        float newY = Mathf.Clamp(position.y, minYBoundaryBoundary, maxYBoundaryBoundary);

        
        return new Vector3(newX, newY, position.z);
    }


    public void SetBoundaries(float minXBoundary, float maxXBoundary, float minYBoundary, float maxYBoundary)
    {
        minXLevelBoundary = minXBoundary;
        maxXLevelBoundary = maxXBoundary;
        minYLevelBoundary = minYBoundary;
        maxYLevelBoundary = maxYBoundary;
    }

    private void OnDrawGizmosSelected()
    {
        if (useBoundaries) {
            Debug.DrawLine(new Vector3(minXLevelBoundary, minYLevelBoundary, 0), new Vector3(minXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Left line
            Debug.DrawLine(new Vector3(maxXLevelBoundary, minYLevelBoundary, 0), new Vector3(maxXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Right line
            Debug.DrawLine(new Vector3(minXLevelBoundary, minYLevelBoundary, 0), new Vector3(maxXLevelBoundary, minYLevelBoundary, 0), Color.blue); // Bottom line
            Debug.DrawLine(new Vector3(minXLevelBoundary, maxYLevelBoundary, 0), new Vector3(maxXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Top line
        }
    }

    #endregion Boundaries
    
    #region Shake
    public void ShakeCamera(float duration, float magnitude, float xShakeRange = 1f, float yShakeRange = 1f)
    {
        if (!target) return;
        StartCoroutine(Shake(duration, magnitude, xShakeRange, yShakeRange));
        // Debug.Log("Shaking camera for: " + duration + ", At: " + magnitude + ", yRange: " + yShakeRange + ", xRange: " + xShakeRange);
    }

    private IEnumerator Shake(float duration, float magnitude, float xShakeRange, float yShakeRange)
    {
        isShaking = true;
        float elapsed = 0f;

        while (isShaking && elapsed < duration)
        {
            float x = Random.Range(-xShakeRange, xShakeRange) * magnitude;
            float y = Random.Range(-yShakeRange, yShakeRange) * magnitude;

            shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }
    private void ApplyShake()
    {
        if (isShaking)
        {
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);

            if (useBoundaries)
            {
                smoothedPosition = HandleBoundaries(smoothedPosition);
            }

            transform.position = smoothedPosition;
        }
    }


    public void StopCameraShake() {
        isShaking = false;
        shakeOffset = Vector3.zero;
    }

    #endregion Shake
    
    #region Calculations
    private Vector3 CalculateTargetOffset()
    {
        Vector3 offset = Vector3.zero;

        if (target.CompareTag("Player"))
        {
            PlayerController2D player = target.GetComponent<PlayerController2D>();
            
            if (horizontalOffset) {
                if (player.rigidBody.linearVelocity.x != 0 ) {

                    if (player.isFacingRight) {
                        offset.x = horizontalOffsetStrength + (player.rigidBody.linearVelocity.x/1.5f);
                    } else {
                        offset.x = -horizontalOffsetStrength + (player.rigidBody.linearVelocity.x/1.5f);
                    }
                }
            }

            if (verticalOffset) {
                if (player.isGrounded) { // player is on the ground
                    offset.y = 1f;
                } else {
                    if (player.isWallSliding) { // Player is wall sliding
                        offset.y = verticalOffsetStrength + player.rigidBody.linearVelocity.y;
                        
                    }  else { // Player is in the air
                        if (!player.isWallSliding && player.rigidBody.linearVelocity.y > 0) { // Player is jumping
                            offset.y = verticalOffsetStrength * player.rigidBody.linearVelocity.y;
                            
                        } else if (!player.isWallSliding && player.rigidBody.linearVelocity.y < 0 && player.rigidBody.linearVelocity.y > -7) { // Player is falling
                            offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.linearVelocity.y/2f,-1,0);

                        } else if (!player.isWallSliding && player.rigidBody.linearVelocity.y < -7) { // Player is fast falling
                            offset.y = -verticalOffsetStrength + Mathf.Clamp(player.rigidBody.linearVelocity.y/2f,-10,0);

                        }
                    }
                }
            }
        }

        targetOffset = offset;
        return offset;
    }

    private Vector3 CalculateTargetPosition()
    {
        CalculateTargetOffset();
        Vector3 basePosition = new Vector3(target.position.x, target.position.y, transform.position.z) + targetOffset;
        return basePosition + shakeOffset;
    }


    #endregion Calculations

    #region Debugging functions
    private readonly StringBuilder debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        
        debugStringBuilder.Clear();
        
        debugStringBuilder.AppendFormat("Camera:\n");
        debugStringBuilder.AppendFormat("Shake Offset: ({0:0.0},{1:0.0})\n", shakeOffset.x, shakeOffset.y);
        debugStringBuilder.AppendFormat("Zoom: {0:0.0} ({1}/{2})\n", currentZoom, minZoom, maxZoom);

        debugStringBuilder.AppendFormat("\nTarget: {0}\n", target.name);
        debugStringBuilder.AppendFormat("Position: ({0:0.0},{1:0.0})\n", targetPosition.x, targetPosition.y);
        debugStringBuilder.AppendFormat("Offset: ({0:0.0},{1:0.0})\n", targetOffset.x, targetOffset.y);

        debugStringBuilder.AppendFormat("\nBoundaries: {0}\n", useBoundaries);
        debugStringBuilder.AppendFormat("Horizontal: {0:0.} / {1:0.}\n", minXLevelBoundary, maxXLevelBoundary);
        debugStringBuilder.AppendFormat("Vertical: {0:0.} / {1:0.}", minYLevelBoundary, maxYLevelBoundary);
                
        textObject.text = debugStringBuilder.ToString(); 


    }
    
    #endregion Debugging functions

}