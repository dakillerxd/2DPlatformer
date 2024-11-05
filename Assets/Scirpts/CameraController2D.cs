
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
    private Camera _camera;

    [Header("Target")]
    [SerializeField] [Range(0f, 2f)] private float smoothFollowSpeed = 0.5f;
    private Transform _target;
    private Vector3 _targetPosition;
    private PlayerController2D _player;
    
    [Header("Zoom Settings")]
    [SerializeField] private bool allowZoomControl = true;
    [SerializeField] private float targetZoom = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] [Min(2)] private float startingZoom = 2;
    private float _zoomVelocity;

    [Header("Camera Boundaries")]
    [SerializeField] private bool useBoundaries;
    [SerializeField] private float minXLevelBoundary;
    [SerializeField] private float maxXLevelBoundary;
    [SerializeField] private float minYLevelBoundary;
    [SerializeField] private float maxYLevelBoundary;
    
    [Header("Shake Settings")]
    [HideInInspector] public bool isShaking;
    private Vector3 _shakeOffset;
    
    [Header("Player Specific Settings")]
    [SerializeField] private bool verticalOffset = true;
    [SerializeField] private bool horizontalOffset = true;
    [SerializeField] [Range(0f, 2f)] private float verticalOffsetStrength = 1f;
    [SerializeField] [Range(0f, 2f)] private float horizontalOffsetStrength = 1f;
    private Vector3 _targetOffset;
    private Vector3 _currentVelocity;




    


    private void Awake() {

       if (Instance != null && Instance != this) {

            Destroy(gameObject);

       } else {
            
            Instance = this; 
       }
    }


    private void Start() {
        
        // Initialize camera
        _camera = GetComponent<Camera>();
        _camera.orthographicSize = startingZoom;
        
        // Get the player
        PlayerController2D.Instance.TryGetComponent<PlayerController2D>(out _player);
        if (_player) { _target = _player.transform; }
    }

    private void Update() {
        HandleZoomInput();
    }


    private void LateUpdate() {
        
        // Follow a target/player and set zoom levels
        FollowTarget();
        HandleZoom();
        
        // Handle camera boundaries
        // HandleBoundaries();
        
        // Apply shake when needed
        // ApplyShake();
    }

    
    
    
    
#region Target functions


    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    private void FollowTarget() {
        
        if (!_target) return;
        
        _targetPosition = CalculateTargetPosition();
        _targetOffset = CalculateTargetOffset();
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, _targetPosition + _targetOffset, ref _currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);
        transform.position = smoothedPosition;
    }
    
    private Vector3 CalculateTargetPosition() {
        
        Vector3 basePosition = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        return basePosition;
    }
    
    private Vector3 CalculateTargetOffset() {
            
        Vector3 offset = Vector3.zero;
        if (!_target.CompareTag("Player")) return offset;
            
                
        if (horizontalOffset) {
            if (_player.rigidBody.linearVelocity.x != 0 ) {

                if (_player.isFacingRight) {
                    offset.x = horizontalOffsetStrength + (_player.rigidBody.linearVelocity.x/1.5f);
                } else {
                    offset.x = -horizontalOffsetStrength + (_player.rigidBody.linearVelocity.x/1.5f);
                }
            }
        }

        if (verticalOffset) {
            if (_player.isGrounded) { // player is on the ground
                offset.y = 1f;
            } else {
                if (_player.isWallSliding) { // Player is wall sliding
                    offset.y = verticalOffsetStrength + _player.rigidBody.linearVelocity.y;
                        
                }  else { // Player is in the air
                    if (!_player.isWallSliding && _player.rigidBody.linearVelocity.y > 0) { // Player is jumping
                        offset.y = verticalOffsetStrength * _player.rigidBody.linearVelocity.y;
                            
                    } else if (!_player.isWallSliding && _player.rigidBody.linearVelocity.y < 0 && _player.rigidBody.linearVelocity.y > -7) { // Player is falling
                        offset.y = -verticalOffsetStrength + Mathf.Clamp(_player.rigidBody.linearVelocity.y/2f,-1,0);

                    } else if (!_player.isWallSliding && _player.rigidBody.linearVelocity.y < -7) { // Player is fast falling
                        offset.y = -verticalOffsetStrength + Mathf.Clamp(_player.rigidBody.linearVelocity.y/2f,-10,0);

                    }
                }
            }
        }
            
        
        return offset;
    }

#endregion Target functions

#region Zoom

    private void HandleZoomInput() {
        if (!allowZoomControl) return;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoom = SetZoom(zoomInput * zoomSpeed);
    }
    private void HandleZoom() {

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetZoom, ref _zoomVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);
    }

    private float SetZoom(float zoom) {
        return targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
    
    private float CalculateTargetZoomOffset() {
        
        if (_player.isRunning || _player.wasRunning)
        {
            return 1;
        }

        return 0;
    }
    
#endregion Zoom





#region Boundaries
    
    private Vector3 HandleBoundaries(Vector3 position)
    {
        float camHeight = _camera.orthographicSize;
        float camWidth = camHeight * _camera.aspect;

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

 

#endregion Boundaries
    
#region Shake

    public void ShakeCamera(float duration, float magnitude, float xShakeRange = 1f, float yShakeRange = 1f) {
        if (!_target) return;
        StartCoroutine(Shake(duration, magnitude, xShakeRange, yShakeRange));

    }

    private IEnumerator Shake(float duration, float magnitude, float xShakeRange, float yShakeRange)
    {
        isShaking = true;
        float elapsed = 0f;

        while (isShaking && elapsed < duration)
        {
            float x = Random.Range(-xShakeRange, xShakeRange) * magnitude;
            float y = Random.Range(-yShakeRange, yShakeRange) * magnitude;

            _shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _shakeOffset = Vector3.zero;
        isShaking = false;
    }
    private void ApplyShake()
    {
        if (isShaking)
        {
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, _targetPosition, ref _currentVelocity, smoothFollowSpeed, Mathf.Infinity, Time.deltaTime);

            if (useBoundaries)
            {
                smoothedPosition = HandleBoundaries(smoothedPosition);
            }

            transform.position = smoothedPosition;
        }
    }


    public void StopCameraShake() {
        isShaking = false;
        _shakeOffset = Vector3.zero;
    }
    
#endregion Shake
    
#region Debugging functions
    private void OnDrawGizmos()
    {
        if (useBoundaries) {
            Debug.DrawLine(new Vector3(minXLevelBoundary, minYLevelBoundary, 0), new Vector3(minXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Left line
            Debug.DrawLine(new Vector3(maxXLevelBoundary, minYLevelBoundary, 0), new Vector3(maxXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Right line
            Debug.DrawLine(new Vector3(minXLevelBoundary, minYLevelBoundary, 0), new Vector3(maxXLevelBoundary, minYLevelBoundary, 0), Color.blue); // Bottom line
            Debug.DrawLine(new Vector3(minXLevelBoundary, maxYLevelBoundary, 0), new Vector3(maxXLevelBoundary, maxYLevelBoundary, 0), Color.blue); // Top line
        }
    }
    
    
    
    private readonly StringBuilder debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        
        debugStringBuilder.Clear();
        
        debugStringBuilder.AppendFormat("Camera:\n");
        debugStringBuilder.AppendFormat("Shake Offset: ({0:0.0},{1:0.0})\n", _shakeOffset.x, _shakeOffset.y);
        debugStringBuilder.AppendFormat("Zoom: {0:0.0} ({1}/{2})\n", targetZoom, minZoom, maxZoom);

        debugStringBuilder.AppendFormat("\nTarget: {0}\n", _target.name);
        debugStringBuilder.AppendFormat("Position: ({0:0.0},{1:0.0})\n", _targetPosition.x, _targetPosition.y);
        debugStringBuilder.AppendFormat("Offset: ({0:0.0},{1:0.0})\n", _targetOffset.x, _targetOffset.y);

        debugStringBuilder.AppendFormat("\nBoundaries: {0}\n", useBoundaries);
        debugStringBuilder.AppendFormat("Horizontal: {0:0.} / {1:0.}\n", minXLevelBoundary, maxXLevelBoundary);
        debugStringBuilder.AppendFormat("Vertical: {0:0.} / {1:0.}", minYLevelBoundary, maxYLevelBoundary);
                
        textObject.text = debugStringBuilder.ToString(); 


    }
    
#endregion Debugging functions


}



    

