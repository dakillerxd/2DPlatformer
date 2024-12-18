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
    
    [Header("Movement")] 
    [SerializeField] private CameraState cameraState = CameraState.Free;
    [SerializeField] private float baseVerticalFollowDelay = 0.5f;
    [SerializeField] private float baseHorizontalFollowDelay = 0.5f;
    private float _currentVelocityX;
    private float _currentVelocityY;
    private Vector3 _smoothVelocity = Vector3.zero;

    [Header("Offset")] 
    [SerializeField] private bool useTargetOffsetBoundaries = true;
    [EnableIf("useTargetOffsetBoundaries")]
    [SerializeField] private float minHorizontalOffset = -4;
    [SerializeField] private float maxHorizontalOffset = 4;
    [SerializeField] private float minVerticalOffset = -3;
    [SerializeField] private float maxVerticalOffset = 3;
    [EndIf]
    private Vector3 _targetPosition;
    private Vector3 _targetTriggerOffset;
    private Vector3 _targetDynamicOffset;
    
    [Header("Zoom")]
    [SerializeField] private float defaultZoom = 4;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] private float zoomSpeed = 0.5f;
    private float _zoomVelocity;
    private float _triggerZoomOffset;
    private float _targetDynamicZoom;
    
    [Header("Shake Effect")]
    private bool _isShaking;
    private Vector3 _shakeOffset;
    
    [Header("References")]
    [SerializeField] private CameraTrigger triggerPrefab;
    public Transform target;
    private List<CameraTrigger> _activeTriggers = new List<CameraTrigger>();
    private float _lastStateChangeTime;
    private Camera _camera;
    private float _cameraHeight;
    private float _cameraWidth;
    
    
    [Header("Debug")]
    private Vector3 _cachedPosition;
    private StringBuilder _debugText = new StringBuilder(256);
    private float _debugUpdateTimer;
    private const float DEBUG_UPDATE_INTERVAL = 0.1f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this; 
        }
    }
    
    private void Start() {
        _camera = GetComponent<Camera>();
        _camera.orthographicSize = defaultZoom;
        _cameraHeight = _camera.orthographicSize * 2;
        _cameraWidth = _cameraHeight * _camera.aspect;
        _triggerZoomOffset = 0f;
        _targetTriggerOffset = Vector3.zero;
        _targetDynamicZoom = 0f;
        _targetDynamicOffset = Vector3.zero;
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
            _targetTriggerOffset = Vector3.zero;
            _triggerZoomOffset = 0f;
            
        }
    }

    private void HandleActiveTriggers() {
        // Reset values
        _targetTriggerOffset = Vector3.zero;
        _triggerZoomOffset = 0f;

        if (_activeTriggers.Count == 0) return;

        // Get latest trigger for state
        CameraTrigger currentTrigger = _activeTriggers[^1];
        if (currentTrigger.setCameraStateOnEnter) {
            cameraState = currentTrigger.cameraStateOnEnter;
        }

        // Single loop instead of multiple loops
        foreach (CameraTrigger trigger in _activeTriggers)
        {
            if (trigger.setCameraZoom) _triggerZoomOffset += trigger.zoomOffset;
            if (trigger.setCameraOffset) _targetTriggerOffset += trigger.offset;
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

#endregion Triggers

#region States

    public void SetCameraState(CameraState newState) {
        cameraState = newState;
    }
    
#endregion States
    
#region Target

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


    private void FollowTarget() {
        if (!target) return;
        
        _targetPosition = CalculateTargetPosition();
        Vector3 targetPos = _targetPosition + _targetDynamicOffset;
        float smoothedX = transform.position.x;
        float smoothedY = transform.position.y;

        // Find the trigger that matches the current camera state
        CameraTrigger matchingTrigger = null;
        if (_activeTriggers.Count > 0) {
            foreach (var trigger in _activeTriggers) {
                if (trigger.setCameraStateOnEnter && trigger.cameraStateOnEnter == cameraState) {
                    matchingTrigger = trigger;
                    break;
                }
            }
        }

        // Handle different camera states
        switch (cameraState) {
            case CameraState.Locked:
                if (matchingTrigger != null && matchingTrigger.cameraPosition != null) {
                    Vector3 fixedPosWithOffset = matchingTrigger.cameraPosition.position + _targetTriggerOffset + _shakeOffset;
                    smoothedX = Mathf.SmoothDamp(transform.position.x, fixedPosWithOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                    smoothedY = Mathf.SmoothDamp(transform.position.y, fixedPosWithOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                break;

            case CameraState.VerticalLocked:
                if (matchingTrigger != null && matchingTrigger.cameraPosition != null) {
                    // Apply Y offset to fixed position
                    smoothedY = Mathf.SmoothDamp(transform.position.y, matchingTrigger.cameraPosition.position.y + _targetTriggerOffset.y + _shakeOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                } else {
                    smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y + _targetTriggerOffset.y + _shakeOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x + _targetTriggerOffset.x + _shakeOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;

            case CameraState.HorizontalLocked:
                if (matchingTrigger != null && matchingTrigger.cameraPosition != null) {
                    // Apply X offset to fixed position
                    smoothedX = Mathf.SmoothDamp(transform.position.x, matchingTrigger.cameraPosition.position.x + _targetTriggerOffset.x + _shakeOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                } else {
                    smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x + _targetTriggerOffset.x + _shakeOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                }
                smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y + _targetTriggerOffset.y + _shakeOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;

            case CameraState.Free:
            default:
                smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x + _targetTriggerOffset.x + _shakeOffset.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
                smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y + _targetTriggerOffset.y + _shakeOffset.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
                break;
        }
        
        _cachedPosition.Set(smoothedX, smoothedY, transform.position.z);
        transform.position = _cachedPosition;
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
        
        bool horizontalDelay = Time.time < _lastStateChangeTime + 1f;
        Vector3 position = transform.position;
        
        if (!horizontalDelay)
        {
            position.x = Mathf.Clamp(position.x, target.position.x + minHorizontalOffset, target.position.x + maxHorizontalOffset);
        }
        position.y = Mathf.Clamp(position.y, target.position.y + minVerticalOffset, target.position.y + maxVerticalOffset);
        
        transform.position = position;
    }
    
    public void SetDynamicOffset(Vector3 offset)
    {
        _targetDynamicOffset = offset;
    }


#endregion Offset

#region Zoom

    private void HandleZoom() 
    {
        if (!target) return;
        
        float finalZoom = defaultZoom + _triggerZoomOffset + _targetDynamicZoom;
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
    
    public void SetDynamicZoom(float zoomOffset)
    {
        _targetDynamicZoom = zoomOffset;
    }

#endregion

#region Shake

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
#endregion Shake
    
#region Debugging
    
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        if (!textObject || !textObject.isActiveAndEnabled) return;
        
        _debugUpdateTimer -= Time.deltaTime;
        if (_debugUpdateTimer <= 0) {
            _debugUpdateTimer = DEBUG_UPDATE_INTERVAL;
            
            _debugText.Clear()
                .AppendLine("\n<color=#00FF00>Camera</color>")
                .AppendLine($"State: {cameraState}")
                .AppendLine($"Target: {(target ? target.name : "None")}")
                .AppendLine($"Position: X: {transform.position.x:F1} Y: {transform.position.y:F1}")
                .AppendLine($"Zoom: Current: {_camera.orthographicSize:F1} Base: {defaultZoom:F1}")
                
                .AppendLine("\n<color=#00FF00>Offset</color>")
                .AppendLine($"Dynamic: X: {_targetDynamicOffset.x:F1} Y: {_targetDynamicOffset.y:F1}")
                .AppendLine($"Trigger: X: {_targetTriggerOffset.x:F1} Y: {_targetTriggerOffset.y:F1}")
                .AppendLine($"Total: X: {(_targetDynamicOffset.x + _targetTriggerOffset.x):F1} Y: {(_targetDynamicOffset.y + _targetTriggerOffset.y):F1}")
                
                .AppendLine("\n<color=#00FF00>Zoom</color>")
                .AppendLine($"Dynamic: {_targetDynamicZoom:F1}")
                .AppendLine($"Trigger: {_triggerZoomOffset:F1}")
                .AppendLine($"Total: {_camera.orthographicSize:F1}/{maxZoom:F1}");



            // Add Triggers section with detailed info
            if (_activeTriggers.Count > 0) {
                _debugText.AppendLine($"\n<color=#00FF00>Active Triggers ({_activeTriggers.Count})</color>");
                
                for (int i = 0; i < _activeTriggers.Count; i++) {
                    var trigger = _activeTriggers[i];
                    _debugText.Append($"{i + 1}. Trigger [");
                    
                    bool needsComma = false;
                    
                    if (trigger.setCameraStateOnEnter) {
                        _debugText.Append(trigger.cameraStateOnEnter);
                        needsComma = true;
                    }
                    
                    if (trigger.setCameraZoom) {
                        if (needsComma) _debugText.Append(", ");
                        _debugText.Append($"Zoom: {trigger.zoomOffset:+0.0;-0.0;0}");
                        needsComma = true;
                    }
                    
                    if (trigger.setCameraOffset) {
                        if (needsComma) _debugText.Append(", ");
                        _debugText.Append($"Offset: ({trigger.offset.x:F1}, {trigger.offset.y:F1})");
                    }
                    
                    _debugText.AppendLine("]");
                }
            }
            
            // Add Effects section if shaking
            if (_isShaking) {
                _debugText.AppendLine("\n<color=#00FF00>Effects</color>")
                    .AppendLine($"<color=#00FF00>[+]</color> Shaking")
                    .AppendLine($"Shake Offset: X: {_shakeOffset.x:F1} Y: {_shakeOffset.y:F1}");
            }
                
            textObject.text = _debugText.ToString();
        }
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
            

            // Draw vertical lines for horizontal offset boundaries
            Debug.DrawLine(
                new Vector3(offsetMinX, offsetMinY, 0),
                new Vector3(offsetMinX, offsetMaxY, 0),
                Color.blue);
            Debug.DrawLine(
                new Vector3(offsetMaxX, offsetMaxY, 0),
                new Vector3(offsetMaxX, offsetMinY, 0),
                Color.blue);

            // Draw horizontal lines for vertical offset boundaries
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
    
#endif
#endregion Debugging


}