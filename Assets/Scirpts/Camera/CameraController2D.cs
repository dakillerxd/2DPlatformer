using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using VInspector;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    public static CameraController2D Instance { get; private set; }
    
    public Transform target { get; private set; }
    private float _cameraHeight;
    private float _cameraWidth;
    private Camera _camera;
    private PlayerController2D _player;
    private CameraTrigger2D _activeTrigger;
    
    [Header("Follow Speed")]
    [SerializeField] private float baseVerticalFollowDelay = 0.5f;
    [SerializeField] private float baseHorizontalFollowDelay = 0.5f;
    private float _currentVelocityX;
    private float _currentVelocityY;
    
    [Header("Position")]
    [SerializeField] [Min(0f)] private float baseHorizontalOffset = 1f;
    [SerializeField] [Min(0f)] private float baseVerticalOffset = 1f;
    [SerializeField] [Min(0f)] private float horizontalMoveDiminisher = 1.5f;
    [SerializeField] [Min(0f)] private float verticalMoveDiminisher = 2f;
    [SerializeField] [Min(0f)] private float maxHorizontalOffset = 10f;
    [SerializeField] [Min(0f)] private float maxVerticalOffset = 10f;
    [SerializeField] [Min(0f)] private float runHorizontalOffset = 8f;
    private Vector3 _targetPosition;
    private Vector3 _targetStateOffset;
    private Vector3 _triggerOffset;
    
    [Header("Zoom")]
    [SerializeField] private float defaultZoom = 4;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] private float zoomSpeed = 0.5f;
    private float _currentZoom;
    private float _zoomOffset;
    private float _zoomVelocity;

    [Header("Boundaries")]
    private float _minXBoundary;
    private float _maxXBoundary;
    private float _minYBoundary;
    private float _maxYBoundary;
    
    [Header("Shake Effect")]
    private bool _isShaking;
    private Vector3 _shakeOffset;
    
    [Header("References")]
    [SerializeField] private CameraTrigger2D triggerPrefab;
    

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
        _currentZoom = defaultZoom;
        _camera.orthographicSize = defaultZoom;
        _cameraHeight = _camera.orthographicSize * 2;
        _cameraWidth = _cameraHeight * _camera.aspect;
        
        // Get the player
        PlayerController2D.Instance.TryGetComponent<PlayerController2D>(out _player);
        if (_player) { SetTarget(_player.transform);}
    }

    private void Update() {
        HandleZoomInput();
    }
    
    private void LateUpdate() {
        
        FollowTarget();
        HandleZoom();
        HandleBoundaries();
    }

    
    
#region Target functions


    private void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FollowTarget() {
        
        if (!target) return;
        
        _targetPosition = CalculateTargetPosition();
        _targetStateOffset = CalculateTargetOffset();
        Vector3 targetPos = _targetPosition + _triggerOffset + _targetStateOffset + _shakeOffset;
        
        float smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x, ref _currentVelocityX, baseHorizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y, ref _currentVelocityY, baseVerticalFollowDelay, Mathf.Infinity, Time.deltaTime);
        float smoothedZ = transform.position.z; // Keep Z as is, or smooth it too if needed

        Vector3 smoothedPosition = new Vector3(smoothedX, smoothedY, smoothedZ);
        transform.position = smoothedPosition;
    }
    
    private Vector3 CalculateTargetPosition() {
        
        Vector3 basePosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        return basePosition;
    }
    
    private Vector3 CalculateTargetOffset() {
            
        Vector3 offset = Vector3.zero;
        if (!target.CompareTag("Player")) return offset;
        if (_player.currentPlayerState == PlayerState.Frozen) return offset;

        if (!_player.wasRunning)
        {
            offset.x = _player.isFacingRight 
                ? Mathf.Clamp(baseHorizontalOffset + _player.rigidBody.linearVelocityX/horizontalMoveDiminisher,-maxHorizontalOffset, maxHorizontalOffset) 
                : Mathf.Clamp(-baseHorizontalOffset + _player.rigidBody.linearVelocityX/horizontalMoveDiminisher,-maxHorizontalOffset, maxHorizontalOffset);
        }
        else
        {
            offset.x = _player.isFacingRight ? runHorizontalOffset : -runHorizontalOffset;
        }
        
            
        if (_player.isGrounded) { // player is on the ground
            
            if (_player.isFastDropping && (_player.ledgeOnLeft || _player.ledgeOnRight)) // Near a ledge and looking down
            {
                offset.y = -baseVerticalOffset*2;
            } else {
                offset.y = baseVerticalOffset;
            }
            
        } else if (_player.isWallSliding) { // Player is wall sliding
            offset.y = -baseVerticalOffset + _player.rigidBody.linearVelocityY;
                    
        }  else if (!_player.isGrounded && !_player.isWallSliding) { // Player is in the air

            switch (_player.rigidBody.linearVelocityY)
            {
                case  > 0: // Player is jumping
                    offset.y = Mathf.Clamp(baseVerticalOffset + _player.rigidBody.linearVelocityY/verticalMoveDiminisher,-maxVerticalOffset,maxVerticalOffset);
                break;
                case < 0: // Player is falling
                    offset.y = Mathf.Clamp(-baseVerticalOffset + _player.rigidBody.linearVelocityY/verticalMoveDiminisher,-maxVerticalOffset,maxVerticalOffset);
                break;
            }
        }
        return offset;
    }
    
    public void SetTriggerOffset(Vector3 offset)
    {
        _triggerOffset = offset;
    }
    
    public void ResetTriggerOffset()
    {
        _triggerOffset = Vector3.zero;
    }


#endregion Target functions

#region Zoom functions

    public void SetCameraTargetZoom(float zoom)
    {
        _currentZoom = zoom;
    }
    public void ResetZoom()
    {
        _currentZoom = defaultZoom;
    }
    private void HandleZoomInput() {

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        _currentZoom += -zoomInput * zoomSpeed;
    }
    private void HandleZoom() {
        if (!target) return;
        _zoomOffset = CalculateTargetZoomOffset();
        _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _currentZoom + _zoomOffset, ref _zoomVelocity, zoomSpeed, Mathf.Infinity, Time.deltaTime);
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);
    }
    
    private float CalculateTargetZoomOffset()
    {
        float offset = 0;
        
        if (_player.wasRunning)
        {
            offset += 2;
        }
        else if (_player.isTeleporting)
        { 
            offset-= 2;
        }

        return offset;
    }
    
    
#endregion Zoom functions

#region Boundaries functions

    public void SetBoundaries(CameraTrigger2D triggerObject, Vector4  boundaries) {
        
        _activeTrigger = triggerObject;
        _minXBoundary = boundaries.x + (_cameraWidth/2);
        _maxXBoundary = boundaries.y - (_cameraWidth/2);
        _minYBoundary = boundaries.z + (_cameraHeight/2);
        _maxYBoundary = boundaries.w - (_cameraHeight/2);;
    }
    
    public void  ResetBoundaries() {
        _activeTrigger = null;
        _minXBoundary = 0;
        _maxXBoundary = 0;
        _minYBoundary = 0;
        _maxYBoundary = 0;
    }
    
    private void HandleBoundaries() {

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, _minXBoundary, _maxXBoundary);
        position.y = Mathf.Clamp(position.y, _minYBoundary, _maxYBoundary);
        transform.position = position;
    }

#if UNITY_EDITOR // Select the new boundary
    [Button] private void CreateNewTrigger()
    {
        if (!triggerPrefab) return;

        CameraTrigger2D newTrigger = Instantiate(triggerPrefab, transform.position, Quaternion.identity);

        
        UnityEditor.Selection.activeObject = newTrigger.gameObject;
        
        
    }
#endif

#endregion Boundaries functions
    
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
    
#endregion Shake functions
    
#region Debugging functions
    private readonly StringBuilder _debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        
        _debugStringBuilder.Clear();
        
        _debugStringBuilder.AppendFormat("Camera:\n");
        _debugStringBuilder.AppendFormat("\nTarget: {0}\n", target.name);
        _debugStringBuilder.AppendFormat("Position Offset: {0:0.0}, {1:0.0}\n", _targetStateOffset.x, _targetStateOffset.y);
        _debugStringBuilder.AppendFormat("Zoom: {0:0.0} + {1:0.0}, {2:0.0} ({3}/{4})\n", _currentZoom,_zoomOffset , _camera.orthographicSize, minZoom, maxZoom);
        _debugStringBuilder.AppendFormat("Shake Offset: {0} ({1:0.0},{2:0.0})\n", _isShaking, _shakeOffset.x, _shakeOffset.y);
        


        if (_activeTrigger) 
        {
            _debugStringBuilder.AppendFormat("\nBoundaries: {0}\n", _activeTrigger.name);
            _debugStringBuilder.AppendFormat("Horizontal: {0:0.0} / {1:0.0}\n", _minXBoundary, _maxXBoundary);
            _debugStringBuilder.AppendFormat("Vertical: {0:0.0} / {1:0.0}", _minYBoundary, _maxYBoundary);
        }

                
        textObject.text = _debugStringBuilder.ToString(); 
        
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos() { // Draw active bounds
        
        if (!_activeTrigger) return;
        float minXBoundaryPoint = _minXBoundary - (_cameraWidth/2);
        float maxXBoundaryPoint = _maxXBoundary + (_cameraWidth/2);
        float minYBoundaryPoint = _minYBoundary - (_cameraHeight/2);
        float maxYBoundaryPoint = _maxYBoundary + (_cameraHeight/2);;
        
        Debug.DrawLine(new Vector3(minXBoundaryPoint, minYBoundaryPoint, 0), new Vector3(minXBoundaryPoint, maxYBoundaryPoint, 0), Color.red); // Left line
        Debug.DrawLine(new Vector3(maxXBoundaryPoint, minYBoundaryPoint, 0), new Vector3(maxXBoundaryPoint, maxYBoundaryPoint, 0), Color.red); // Right line
        Debug.DrawLine(new Vector3(minXBoundaryPoint, minYBoundaryPoint, 0), new Vector3(maxXBoundaryPoint, minYBoundaryPoint, 0), Color.red); // Bottom line
        Debug.DrawLine(new Vector3(minXBoundaryPoint, maxYBoundaryPoint, 0), new Vector3(maxXBoundaryPoint, maxYBoundaryPoint, 0), Color.red); // Top line
    }
#endif
    
#endregion Debugging functions


}



    

