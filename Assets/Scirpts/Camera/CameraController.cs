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

    private void HandleActiveTriggers()
    {
        // Reset offset and zoom at the start
        _targetTriggerOffset = Vector3.zero;
        _triggerZoomOffset = 0f;

        if (_activeTriggers.Count == 0) return;

        // Get the most recent trigger for state settings
        CameraTrigger currentTrigger = _activeTriggers[^1];

        // Handle camera state from most recent trigger
        if (currentTrigger.setCameraStateOnEnter)
        {
            cameraState = currentTrigger.cameraStateOnEnter;
        }

        // Only accumulate zoom offsets from triggers that have setCameraZoom enabled
        foreach (var trigger in _activeTriggers)
        {
            if (trigger.setCameraZoom)
            {
                _triggerZoomOffset += trigger.zoomOffset;
            }
        }

        // Only accumulate offsets from triggers that have setCameraOffset enabled
        foreach (var trigger in _activeTriggers)
        {
            if (trigger.setCameraOffset)  // Only add offset if setCameraOffset is true
            {
                _targetTriggerOffset += trigger.offset;
            }
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


#endregion

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
#endregion
    
#region Debugging

    private readonly StringBuilder _debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        _debugStringBuilder.Clear();
        
        // Core Stats Section
        AppendHeader("Camera");
        AppendStat("State", cameraState.ToString());
        AppendStat("Target", target ? target.name : "None");
        AppendStat("Position", $"X: {transform.position.x:F1} Y: {transform.position.y:F1}");
        AppendStat("Zoom", $"Current: {_camera.orthographicSize:F1} Base: {defaultZoom:F1}");
        
        // Offset Section
        AppendHeader("Offset");
        Vector2 targetOffset = target ? _targetDynamicOffset : Vector2.zero;
        AppendStat("Dynamic", $"X: {targetOffset.x:F1} Y: {targetOffset.y:F1}");
        AppendStat("Trigger", $"X: {_targetTriggerOffset.x:F1} Y: {_targetTriggerOffset.y:F1}");
        AppendStat("Total", $"X: {(targetOffset.x + _targetTriggerOffset.x):F1} Y: {(targetOffset.y + _targetTriggerOffset.y):F1}");
        
        // Zoom Section
        AppendHeader("Zoom");
        AppendStatGroup(new Dictionary<string, (float value, float? max)> {
            { "Dynamic", (_targetDynamicZoom, null) },
            { "Trigger", (_triggerZoomOffset, null) },
            { "Total", (_camera.orthographicSize, maxZoom) }
        });
        

        
        // Effects Section
        AppendHeader("Effects");
        AppendStatGroup(new Dictionary<string, bool> {
            { "Shaking", _isShaking }
        });
        if (_isShaking) {
            AppendStat("Shake Offset", $"X: {_shakeOffset.x:F1} Y: {_shakeOffset.y:F1}");
        }
        
        // Active Triggers Section
        if (_activeTriggers.Count > 0) {
            AppendHeader($"Active Triggers ({_activeTriggers.Count})");
            for (int i = 0; i < _activeTriggers.Count; i++) {
                var trigger = _activeTriggers[i];
                var properties = new List<string>();
                
                if (trigger.setCameraStateOnEnter) 
                    properties.Add(trigger.cameraStateOnEnter.ToString());
                if (trigger.setCameraZoom) 
                    properties.Add($"Zoom: {trigger.zoomOffset:+0.0;-0.0;0}");
                if (trigger.setCameraOffset) 
                    properties.Add($"Offset: ({trigger.offset.x:F1}, {trigger.offset.y:F1})");
                    
                string propertyString = properties.Count > 0 ? $" [{string.Join(", ", properties)}]" : "";
                _debugStringBuilder.AppendLine($"{i + 1}. Trigger{propertyString}");
            }
        }

        textObject.text = _debugStringBuilder.ToString();
    }

    private void AppendHeader(string header) {
        _debugStringBuilder.AppendLine($"\n<color=#00FF00>{header}</color>");
    }

    private void AppendStat(string label, string value) {
        _debugStringBuilder.AppendLine($"{label}: {value}");
    }

    private void AppendStatGroup(Dictionary<string, bool> states) {
        foreach (var (label, state) in states) {
            if (state) {
                _debugStringBuilder.AppendLine($"<color=#00FF00>[+]</color> {label}");
            } else {
                _debugStringBuilder.AppendLine($"<color=#FF0000>[-]</color> {label}");
            }
        }
    }

    private void AppendStatGroup(Dictionary<string, (float value, float? max)> states) {
        foreach (var (label, info) in states) {
            string maxText = info.max.HasValue ? $"/{info.max:F1}" : "";
            _debugStringBuilder.AppendLine($"{label}: {info.value:F1}{maxText}");
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

            bool drawHorizontalOffsets = true;
            bool drawVerticalOffsets = true;
            

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
        
    }
    
#endif
#endregion


}