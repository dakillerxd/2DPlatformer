using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;

public enum CameraState {
    Locked,
    VerticalLocked,
    HorizontalLocked,
    Free
}

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    [Header("Camera State")]
    [SerializeField] private CameraState cameraState = CameraState.Free;
    private float _cameraHeight;
    private float _cameraWidth;

    [Header("Movement")] 
    [SerializeField] private float baseVerticalFollowDelay = 0.5f;
    [SerializeField] private float baseHorizontalFollowDelay = 0.5f;
    private float _currentVelocityX;
    private float _currentVelocityY;
    private Vector3 _smoothVelocity = Vector3.zero;
    private const float SmoothTime = 0.2f;

    [Header("Offset")] 
    [SerializeField] private float baseHorizontalOffset = 1f;
    [SerializeField] private float baseVerticalOffset = 1.5f;
    [Space(10)]
    [SerializeField] private bool useTargetOffsetBoundaries = true;
    [EnableIf("useTargetOffsetBoundaries")]
    [SerializeField] private float minHorizontalOffset = -4;
    [SerializeField] private float maxHorizontalOffset = 4;
    [SerializeField] private float minVerticalOffset = -3;
    [SerializeField] private float maxVerticalOffset = 3;
    [EndIf]
    [Space(10)]
    [SerializeField] [Min(1f)] private float verticalMoveDiminisher;
    [SerializeField] [Min(1f)] private float horizontalMoveDiminisher;
    private Vector3 _targetPosition;
    private Vector3 _targetStateOffset;
    
    [Header("Zoom")]
    [SerializeField] private float defaultZoom = 4;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] private float zoomSpeed = 0.5f;
    private float _targetZoom;
    private float _zoomVelocity;
    private float _triggerZoomOffset;

    [Header("Boundaries")]
    private float _minXBoundary;
    private float _maxXBoundary;
    private float _minYBoundary;
    private float _maxYBoundary;
    
    [Header("Shake Effect")]
    private bool _isShaking;
    private Vector3 _shakeOffset;
    
    [Header("References")]
    [SerializeField] private CameraTrigger triggerPrefab;
    private float _lastStateChangeTime;
    public Transform target;
    private Camera _camera;
    private PlayerController _player;
    private List<CameraTrigger> _activeTriggers = new List<CameraTrigger>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this; 
        }
    }
    
    private void Start() {
        _camera = GetComponent<Camera>();
        _targetZoom = defaultZoom;
        _camera.orthographicSize = defaultZoom;
        _cameraHeight = _camera.orthographicSize * 2;
        _cameraWidth = _cameraHeight * _camera.aspect;
        _triggerZoomOffset = 0f;
        
        if (!PlayerController.Instance) return;
        _player = PlayerController.Instance;
        SetTarget(_player.transform);
    }
    
    private void LateUpdate() {
        HandleActiveTriggers();
        FollowTarget();
        HandleZoom();
        HandleOffsetBoundaries();
    }



#region Triggers

    public void AddActiveTrigger(CameraTrigger trigger)
    {
        if (!trigger || _activeTriggers.Contains(trigger)) return;
        _activeTriggers.Add(trigger);
    }

    public void RemoveActiveTrigger(CameraTrigger trigger)
    {
        if (!trigger) return;
        if (!_activeTriggers.Contains(trigger)) return;
    
        bool wasLastTrigger = _activeTriggers.Count == 1;
        _activeTriggers.Remove(trigger);
    
        // Handle the exit state of the trigger we're leaving
        if (trigger.setCameraStateOnExit)
        {
            _lastStateChangeTime = Time.time;
            cameraState = trigger.cameraStateOnExit;
        }
    
        if (_activeTriggers.Count > 0)
        {
            // Update state and other properties based on remaining triggers
            HandleActiveTriggers();
        }
        else
        {
            _targetStateOffset = Vector3.zero;
            _triggerZoomOffset = 0f;
            _targetZoom = defaultZoom;
                
            if (trigger.limitCameraToBoundary && trigger.resetBoundaryOnExit)
            {
                ResetTriggerBoundaries();
            }
        }
    }

    private void HandleActiveTriggers()
    {
        // Reset offset and zoom at the start
        _targetStateOffset = Vector3.zero;
        _triggerZoomOffset = 0f;

        if (_activeTriggers.Count == 0) return;

        // Get the most recent trigger for state settings
        CameraTrigger currentTrigger = _activeTriggers[_activeTriggers.Count - 1];

        // Handle camera state from most recent trigger
        if (currentTrigger.setCameraStateOnEnter)
        {
            cameraState = currentTrigger.cameraStateOnEnter;
        }

        // Accumulate zoom offsets from all active triggers
        foreach (var trigger in _activeTriggers)
        {
            if (trigger.setCameraZoom)
            {
                _triggerZoomOffset += trigger.zoomOffset;
            }
        }

        // Accumulate offsets from all active triggers
        foreach (var trigger in _activeTriggers)
        {
            if (trigger.setCameraOffset)
            {
                _targetStateOffset += trigger.offset;
            }
        }

        // Handle boundaries from the most recent trigger
        if (currentTrigger.limitCameraToBoundary)
        {
            Vector4 boundaries = currentTrigger.GetBoundaries();
            Vector3 position = transform.position;
            _minXBoundary = boundaries.x + (_cameraWidth/2);
            _maxXBoundary = boundaries.y - (_cameraWidth/2);
            _minYBoundary = boundaries.z + (_cameraHeight/2);
            _maxYBoundary = boundaries.w - (_cameraHeight/2);

            Vector3 targetPosition = new Vector3(
                Mathf.Clamp(position.x, _minXBoundary, _maxXBoundary),
                Mathf.Clamp(position.y, _minYBoundary, _maxYBoundary),
                position.z
            );

            transform.position = Vector3.SmoothDamp(position, targetPosition, ref _smoothVelocity, SmoothTime);
        }
    }
    

    #if UNITY_EDITOR 
        [Button] 
        private void CreateNewTrigger()
        {
            if (!triggerPrefab) return;
            CameraTrigger newTrigger = Instantiate(triggerPrefab, transform.position, Quaternion.identity);
            UnityEditor.Selection.activeObject = newTrigger.gameObject;
        }
    #endif

#endregion
    

    
#region States
    public void SetCameraState(CameraState newState) {
        cameraState = newState;
    }
#endregion
    
#region Target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FollowTarget() {
        if (!target) return;

        Vector3 dynamicOffset = CalculateTargetOffset();
        _targetPosition = CalculateTargetPosition();
        Vector3 targetPos = _targetPosition + dynamicOffset + _shakeOffset;
        float smoothedX = transform.position.x;
        float smoothedY = transform.position.y;

        // Handle different camera states
        switch (cameraState) {
            case CameraState.Locked:
                if (_activeTriggers.Count > 0 && _activeTriggers[_activeTriggers.Count - 1].cameraPosition != null) {
                    Transform cameraPos = _activeTriggers[_activeTriggers.Count - 1].cameraPosition;
                    smoothedX = Mathf.SmoothDamp(transform.position.x, cameraPos.position.x + _targetStateOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                    smoothedY = Mathf.SmoothDamp(transform.position.y, cameraPos.position.y + _targetStateOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                break;

            case CameraState.VerticalLocked:
                if (_activeTriggers.Count > 0 && _activeTriggers[_activeTriggers.Count - 1].cameraPosition != null) {
                    smoothedY = Mathf.SmoothDamp(transform.position.y, _activeTriggers[_activeTriggers.Count - 1].cameraPosition.position.y + _targetStateOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x + _targetStateOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;

            case CameraState.HorizontalLocked:
                if (_activeTriggers.Count > 0 && _activeTriggers[_activeTriggers.Count - 1].cameraPosition != null) {
                    smoothedX = Mathf.SmoothDamp(transform.position.x, _activeTriggers[_activeTriggers.Count - 1].cameraPosition.position.x + _targetStateOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y + _targetStateOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;

            case CameraState.Free:
            default:
                smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x + _targetStateOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y + _targetStateOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;
        }
        
        transform.position = new Vector3(smoothedX, smoothedY, transform.position.z);
    }
        
    private Vector3 CalculateTargetPosition() {
        Vector3 basePosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        return basePosition;
    }
        
#endregion

#region Offset

    private void HandleOffsetBoundaries()
    {
        if (!target || !useTargetOffsetBoundaries || cameraState == CameraState.Locked) return;
        
        if (Time.time < _lastStateChangeTime + 2f)
        {
            return;
        }
        Vector3 position = transform.position;
        
        // Only apply boundary handling if we have active triggers
        // AND they have non-zero boundaries
        if (_activeTriggers.Count > 0 && (_minXBoundary != 0 || _maxXBoundary != 0 || _minYBoundary != 0 || _maxYBoundary != 0))
        {
            // Calculate offset-based boundaries
            float offsetMinX = target.position.x + minHorizontalOffset;
            float offsetMaxX = target.position.x + maxHorizontalOffset;
            float offsetMinY = target.position.y + minVerticalOffset;
            float offsetMaxY = target.position.y + maxVerticalOffset;

            // Check if offset boundaries are within trigger boundaries and respect camera state
            bool useOffsetX = offsetMinX >= _minXBoundary && offsetMaxX <= _maxXBoundary 
                && cameraState != CameraState.HorizontalLocked;
            bool useOffsetY = offsetMinY >= _minYBoundary && offsetMaxY <= _maxYBoundary 
                && cameraState != CameraState.VerticalLocked;

            // Apply X boundaries
            if (useOffsetX)
            {
                // Use both offset and trigger boundaries
                position.x = Mathf.Clamp(
                    position.x,
                    Mathf.Max(offsetMinX, _minXBoundary),
                    Mathf.Min(offsetMaxX, _maxXBoundary)
                );
            }
            else if (cameraState != CameraState.HorizontalLocked)
            {
                // Use only trigger boundaries if not horizontal locked
                position.x = Mathf.Clamp(position.x, _minXBoundary, _maxXBoundary);
            }

            // Apply Y boundaries
            if (useOffsetY)
            {
                // Use both offset and trigger boundaries
                position.y = Mathf.Clamp(
                    position.y,
                    Mathf.Max(offsetMinY, _minYBoundary),
                    Mathf.Min(offsetMaxY, _maxYBoundary)
                );
            }
            else if (cameraState != CameraState.VerticalLocked)
            {
                // Use only trigger boundaries if not vertical locked
                position.y = Mathf.Clamp(position.y, _minYBoundary, _maxYBoundary);
            }
        }
        else if (cameraState == CameraState.Free) // No active triggers, just use offset boundaries if in free state
        {
            position.x = Mathf.Clamp(position.x, target.position.x + minHorizontalOffset, target.position.x + maxHorizontalOffset);
            position.y = Mathf.Clamp(position.y, target.position.y + minVerticalOffset, target.position.y + maxVerticalOffset);
        }
        else // Handle locked states without triggers
        {
            if (cameraState != CameraState.HorizontalLocked)
            {
                position.x = Mathf.Clamp(position.x, target.position.x + minHorizontalOffset, target.position.x + maxHorizontalOffset);
            }
            if (cameraState != CameraState.VerticalLocked)
            {
                position.y = Mathf.Clamp(position.y, target.position.y + minVerticalOffset, target.position.y + maxVerticalOffset);
            }
        }
        
        transform.position = position;
    }

    private Vector3 CalculateTargetOffset() {
        Vector3 offset = Vector3.zero;
        if (!target.CompareTag("Player")) return offset;
        if (_player.currentPlayerState == PlayerState.Frozen) return offset;
        
        // Horizontal Offset
        if (_player.isFastFalling) offset.x = 0;
        else if (_player.wasRunning) {
            offset.x = _player.isFacingRight
                ? baseHorizontalOffset + _player.rigidBody.linearVelocityX
                : -baseHorizontalOffset + _player.rigidBody.linearVelocityX;
        } else {
            offset.x = _player.isFacingRight
                ? baseHorizontalOffset + _player.rigidBody.linearVelocityX / horizontalMoveDiminisher
                : -baseHorizontalOffset + _player.rigidBody.linearVelocityX / horizontalMoveDiminisher;
        }
        
        // Vertical Offset
        if (_player.isGrounded || _player.isJumping) {
            offset.y = baseVerticalOffset;
        } else if (_player.isFastFalling || _player.isWallSliding) {
            offset.y = -baseVerticalOffset + _player.rigidBody.linearVelocityY / verticalMoveDiminisher;
        } else if (_player.isFalling) {
            offset.y = -baseVerticalOffset;
        }
        
        return offset;
    }
#endregion

#region Zoom functions
private void HandleZoom() {
    if (!target) return;
    float dynamicZoomOffset = CalculateTargetZoomOffset();
    _targetZoom = defaultZoom + _triggerZoomOffset;
    _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        
    float finalZoom = _targetZoom + dynamicZoomOffset;
    finalZoom = Mathf.Clamp(finalZoom, minZoom, maxZoom);
        
    _camera.orthographicSize = Mathf.SmoothDamp(
        _camera.orthographicSize, 
        finalZoom, 
        ref _zoomVelocity, 
        zoomSpeed, 
        Mathf.Infinity, 
        Time.deltaTime
    );
}
    
    private float CalculateTargetZoomOffset()
    {
        float offset = 0;
        if (!_player) return 0;
        
        if (_player.wasRunning)
        {
            offset += 2;
        }
        else if (_player.isTeleporting)
        { 
            offset -= 2;
        }

        return offset;
    }
#endregion

#region Boundaries functions
    private void ResetTriggerBoundaries() {
        _minXBoundary = 0;
        _maxXBoundary = 0;
        _minYBoundary = 0;
        _maxYBoundary = 0;
    }
#endregion
    
#region Shake functions
    private IEnumerator Shake(float duration, float magnitude, float xShakeRange, float yShakeRange)
    {
        _isShaking = true;
        float elapsed = 0f;

        while (_isShaking && elapsed < duration)
        {
            float x = Random.Range(-xShakeRange, xShakeRange) * magnitude;
            float y = Random.Range(-yShakeRange, yShakeRange) * magnitude;

            _shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _shakeOffset = Vector3.zero;
        _isShaking = false;
    }
    
    public void ShakeCamera(float duration, float magnitude, float xShakeRange = 2f, float yShakeRange = 2f) {
        if (!target) return;
        StartCoroutine(Shake(duration, magnitude, xShakeRange, yShakeRange));
    }
    
    public void StopCameraShake() {
        _isShaking = false;
        _shakeOffset = Vector3.zero;
    }
#endregion
    
#region Debugging functions
    private readonly StringBuilder _debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        _debugStringBuilder.Clear();

        // Basic Camera Info
        _debugStringBuilder.AppendFormat("Camera State: {0}\n", cameraState);
        _debugStringBuilder.AppendFormat("Target: {0}\n", target ? target.name : "None");

        // Position and Movement
        _debugStringBuilder.AppendFormat("Position: ({0:0.0}, {1:0.0})\n", transform.position.x, transform.position.y);
        
        // Offset Info
        Vector2 targetOffset = target ? CalculateTargetOffset() : Vector2.zero;
        _debugStringBuilder.AppendFormat("Offset: ({0:0.0}, {1:0.0}) (Target: ({2:0.0}, {3:0.0}), Triggers: ({4:0.0}, {5:0.0}))\n",
            targetOffset.x + _targetStateOffset.x,
            targetOffset.y + _targetStateOffset.y,
            targetOffset.x,
            targetOffset.y,
            _targetStateOffset.x,
            _targetStateOffset.y);
        
        
        // Zoom Info 
        float dynamicZoomOffset = CalculateTargetZoomOffset();
        _debugStringBuilder.AppendFormat("Zoom: {0:0.0} (Base: {1:0.0}, Target: {2:0.0}, Trigger: {3:0.0})\n", 
            _camera.orthographicSize, 
            defaultZoom,
            dynamicZoomOffset,
            _triggerZoomOffset);

        // Boundaries
        if (_activeTriggers.Count > 0 && (_minXBoundary != 0 || _maxXBoundary != 0 || _minYBoundary != 0 || _maxYBoundary != 0)) {
            _debugStringBuilder.AppendFormat("\nBoundaries:\n");
            _debugStringBuilder.AppendFormat("X: {0:0.0} to {1:0.0}\n", _minXBoundary, _maxXBoundary);
            _debugStringBuilder.AppendFormat("Y: {0:0.0} to {1:0.0}\n", _minYBoundary, _maxYBoundary);
        }

        // Shake Effect
        if (_isShaking) {
            _debugStringBuilder.AppendFormat("\nShake Active: ({0:0.0}, {1:0.0})\n", _shakeOffset.x, _shakeOffset.y);
        }

        // Active Triggers
        if (_activeTriggers.Count > 0) {
            _debugStringBuilder.AppendFormat("\nActive Triggers ({0}):\n", _activeTriggers.Count);
            for (int i = 0; i < _activeTriggers.Count; i++) {
                var trigger = _activeTriggers[i];
                _debugStringBuilder.AppendFormat("{0}. {1} [{2}]{3}{4}{5}\n", 
                    i + 1, 
                    "Trigger",
                    trigger.setCameraStateOnEnter ? trigger.cameraStateOnEnter.ToString() : "No State",
                    trigger.limitCameraToBoundary ? " (Bounded)" : "",
                    trigger.setCameraZoom ? string.Format(" (Zoom: {0:+0.0;-0.0;0})", trigger.zoomOffset) : "",
                    trigger.setCameraOffset ? string.Format(" (Offset: {0:0.0}, {1:0.0})", trigger.offset.x, trigger.offset.y) : "");
            }
        }

        textObject.text = _debugStringBuilder.ToString();
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() 
    { 
        if (target && useTargetOffsetBoundaries && cameraState != CameraState.Locked)
        {
            // Calculate offset and trigger boundaries
            float offsetMinX = target.position.x + minHorizontalOffset;
            float offsetMaxX = target.position.x + maxHorizontalOffset;
            float offsetMinY = target.position.y + minVerticalOffset;
            float offsetMaxY = target.position.y + maxVerticalOffset;

            bool drawHorizontalOffsets = cameraState != CameraState.HorizontalLocked;
            bool drawVerticalOffsets = cameraState != CameraState.VerticalLocked;

            // Check if we have active triggers with boundaries
            if (_activeTriggers.Count > 0 && (_minXBoundary != 0 || _maxXBoundary != 0 || _minYBoundary != 0 || _maxYBoundary != 0))
            {
                // Check each axis independently
                drawHorizontalOffsets &= offsetMinX >= _minXBoundary && offsetMaxX <= _maxXBoundary;
                drawVerticalOffsets &= offsetMinY >= _minYBoundary && offsetMaxY <= _maxYBoundary;
            }

            // Draw vertical lines for horizontal offset boundaries
            if (drawHorizontalOffsets)
            {
                Debug.DrawLine(
                    new Vector3(offsetMinX, offsetMinY, 0),
                    new Vector3(offsetMinX, offsetMaxY, 0),
                    Color.blue);
                Debug.DrawLine(
                    new Vector3(offsetMaxX, offsetMaxY, 0),
                    new Vector3(offsetMaxX, offsetMinY, 0),
                    Color.blue);
            }

            // Draw horizontal lines for vertical offset boundaries
            if (drawVerticalOffsets)
            {
                Debug.DrawLine(
                    new Vector3(offsetMinX, offsetMinY, 0),
                    new Vector3(offsetMaxX, offsetMinY, 0),
                    Color.blue);
                Debug.DrawLine(
                    new Vector3(offsetMinX, offsetMaxY, 0),
                    new Vector3(offsetMaxX, offsetMaxY, 0),
                    Color.blue);
            }
        }
        
        // Draw boundaries for all active triggers
        for (int i = 0; i < _activeTriggers.Count; i++)
        {
            Vector4 boundaries = _activeTriggers[i].GetBoundaries();

            // Calculate color - darker red for outer triggers, brighter red for inner triggers
            float intensity = 0.3f + (0.7f * (i / Mathf.Max(1, (float)(_activeTriggers.Count - 1))));
            Color boundaryColor = new Color(1f, 0f, 0f, intensity);
            
            // Draw actual camera constraint area if limitCameraToBoundary is true
            if (_activeTriggers[i].limitCameraToBoundary)
            {
                float minX = boundaries.x + (_cameraWidth/2);
                float maxX = boundaries.y - (_cameraWidth/2);
                float minY = boundaries.z + (_cameraHeight/2);
                float maxY = boundaries.w - (_cameraHeight/2);
                
                Debug.DrawLine(
                    new Vector3(minX, minY, 0),
                    new Vector3(minX, maxY, 0),
                    Color.blue);
                Debug.DrawLine(
                    new Vector3(maxX, minY, 0),
                    new Vector3(maxX, maxY, 0),
                    Color.blue);
                Debug.DrawLine(
                    new Vector3(minX, minY, 0),
                    new Vector3(maxX, minY, 0),
                    Color.blue);
                Debug.DrawLine(
                    new Vector3(minX, maxY, 0),
                    new Vector3(maxX, maxY, 0),
                    Color.blue);
            }
            
            // Draw full trigger area boundaries with dashed lines
            Debug.DrawLine(
                new Vector3(boundaries.x, boundaries.z, 0),
                new Vector3(boundaries.x, boundaries.w, 0),
                boundaryColor);
            Debug.DrawLine(
                new Vector3(boundaries.y, boundaries.z, 0),
                new Vector3(boundaries.y, boundaries.w, 0),
                boundaryColor);
            Debug.DrawLine(
                new Vector3(boundaries.x, boundaries.z, 0),
                new Vector3(boundaries.y, boundaries.z, 0),
                boundaryColor);
            Debug.DrawLine(
                new Vector3(boundaries.x, boundaries.w, 0),
                new Vector3(boundaries.y, boundaries.w, 0),
                boundaryColor);
        }
    }
#endif
#endregion
}