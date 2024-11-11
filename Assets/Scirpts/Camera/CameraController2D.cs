
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    public static CameraController2D Instance { get; private set; }
    private Camera _camera;
    private float _cameraHeight;
    private float _cameraWidth;
    
    [Header("Target")]
    [SerializeField] private Transform target;
    private float _targetFollowDelay;
    private Vector3 _targetPosition;
    private Vector3 _targetOffset;
    private Vector3 _currentVelocity;
    private float _currentVelocityX;
    private float _currentVelocityY;
    
    [Header("Zoom")]
    [SerializeField] private bool allowZoomControl = true;
    [SerializeField] private float targetZoom = 5f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 6f;
    [SerializeField] [Min(2)] private float startingZoom = 2;
    private float _zoomOffset;
    private float _zoomVelocity;

    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries;
    [SerializeField] private CameraBoundary2D boundaryPrefab;
    private CameraBoundary2D _activeBoundary;
    private float _minXBoundary;
    private float _maxXBoundary;
    private float _minYBoundary;
    private float _maxYBoundary;
    
    [Header("Shake Effect")]
    private bool _isShaking;
    private Vector3 _shakeOffset;
    
    [Header("Player Settings")]
    [SerializeField] [Range(0f, 2f)] private float verticalFollowDelay = 0.5f;
    [SerializeField] [Range(0f, 2f)] private float horizontalFollowDelay = 0.5f;
    [Space(10)]
    [SerializeField] [Min(0f)] private float baseHorizontalOffset = 1f;
    [SerializeField] [Min(0f)] private float horizontalMoveDiminisher = 1.5f;
    [SerializeField] [Min(0f)] private float maxHorizontalOffset = 10f;
    [SerializeField] [Min(0f)] private float runHorizontalOffset = 8f;
    [Space(10)]
    [SerializeField] [Min(0f)] private float baseVerticalOffset = 1f;
    [SerializeField] [Min(0f)] private float verticalMoveDiminisher = 2f;
    [SerializeField] [Min(0f)] private float maxVerticalOffset = 10f;
    [SerializeField] private float fallFollowDelaySubtraction = -0.35f;
    [SerializeField] private float fastFallFollowDelaySubtraction = -0.7f;
    private PlayerController2D _player;

    

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
        targetZoom = startingZoom;
        _camera.orthographicSize = startingZoom;
        _cameraHeight = _camera.orthographicSize * 2;
        _cameraWidth = _cameraHeight * _camera.aspect;
        
        // Get the player
        PlayerController2D.Instance.TryGetComponent<PlayerController2D>(out _player);
        if (_player) { target = _player.transform; }
        

    }

    private void Update() {
        HandleZoomInput();
        // HandleTargetSelection();
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
        _targetOffset = CalculateTargetOffset();
        _targetFollowDelay = CalculateTargetFollowDelay();
        Vector3 targetPos = _targetPosition + _targetOffset + _shakeOffset;
        
        float smoothedX = Mathf.SmoothDamp(transform.position.x, targetPos.x, ref _currentVelocityX, horizontalFollowDelay, Mathf.Infinity, Time.deltaTime);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos.y, ref _currentVelocityY, verticalFollowDelay + _targetFollowDelay, Mathf.Infinity, Time.deltaTime);
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
    
    private float CalculateTargetFollowDelay() {

        float delay = 0;
        if (!target.CompareTag("Player")) return delay;
        if (_player.currentPlayerState == PlayerState.Frozen) return delay;

        
        switch (_player.rigidBody.linearVelocityY)
        {
            case < 0: // Player is falling

                delay = fallFollowDelaySubtraction;
                if (_player.isFastFalling) {delay = fastFallFollowDelaySubtraction;}
                break;
        }
        
        
        return delay;
    }
    

#endregion Target functions

#region Zoom functions

    public void SetCameraTargetZoom(float zoom)
    {
        targetZoom = zoom;
    }
    public void ResetTargetZoom()
    {
        targetZoom = startingZoom;
    }
    private void HandleZoomInput() {
        if (!allowZoomControl) return;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoom += -zoomInput * zoomSpeed;
    }
    private void HandleZoom() {
        if (!target) return;
        _zoomOffset = CalculateTargetZoomOffset();
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetZoom + _zoomOffset, ref _zoomVelocity, zoomSpeed, Mathf.Infinity, Time.deltaTime);
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

    public void SetBoundaries(CameraBoundary2D boundaryObject, Vector4  boundaries) {
        
        _activeBoundary = boundaryObject;
        _minXBoundary = boundaries.x + (_cameraWidth/2);
        _maxXBoundary = boundaries.y - (_cameraWidth/2);
        _minYBoundary = boundaries.z + (_cameraHeight/2);
        _maxYBoundary = boundaries.w - (_cameraHeight/2);;
    }
    
    public void  ResetBoundaries() {
        _activeBoundary = null;
        _minXBoundary = 0;
        _maxXBoundary = 0;
        _minYBoundary = 0;
        _maxYBoundary = 0;
    }
    
    private void HandleBoundaries() {
        if (!_activeBoundary || !useBoundaries) return;

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, _minXBoundary, _maxXBoundary);
        position.y = Mathf.Clamp(position.y, _minYBoundary, _maxYBoundary);
        transform.position = position;
    }

    [Button] private void CreateNewBoundary()
    {
        if (!boundaryPrefab) return;

        CameraBoundary2D newBoundary = Instantiate(boundaryPrefab, transform.position, Quaternion.identity);

        #if UNITY_EDITOR // Select the new boundary
        UnityEditor.Selection.activeObject = newBoundary.gameObject;
        #endif
        
    }

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
    private void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit) {
                if (hit.collider.CompareTag("Player")) {
                    CameraController2D.Instance.SetTarget(hit.collider.transform.parent.parent);
                    Debug.Log("Set camera target to: " + hit.collider.transform.parent.parent.name);
                }
                else if (hit.collider.CompareTag("Enemy")) {
                    CameraController2D.Instance.SetTarget(hit.collider.transform.parent.parent);
                }
                else if (hit.collider.CompareTag("Checkpoint")) {
                    CheckpointManager2D.Instance.ActivateCheckpoint(hit.collider.gameObject.GetComponent<Checkpoint2D>());
                }
                else {
                    Debug.Log("Clicked on: " + hit.collider.gameObject.name);
                }
            }
        }
    }

    private readonly StringBuilder _debugStringBuilder = new StringBuilder(256);
    public void UpdateDebugText(TextMeshProUGUI textObject) {
        
        _debugStringBuilder.Clear();
        
        _debugStringBuilder.AppendFormat("Camera:\n");
        _debugStringBuilder.AppendFormat("\nTarget: {0}\n", target.name);
        // _debugStringBuilder.AppendFormat("Follow Delay: {0:0.0} + {1:0.0}\n", followDelay, _targetFollowDelay);
        _debugStringBuilder.AppendFormat("Position Offset: {0:0.0}, {1:0.0}\n", _targetOffset.x, _targetOffset.y);
        _debugStringBuilder.AppendFormat("Zoom: {0:0.0} + {1:0.0}, {2:0.0} ({3}/{4})\n", targetZoom,_zoomOffset , _camera.orthographicSize, minZoom, maxZoom);
        _debugStringBuilder.AppendFormat("Shake Offset: {0} ({1:0.0},{2:0.0})\n", _isShaking, _shakeOffset.x, _shakeOffset.y);
        


        if (useBoundaries && _activeBoundary) 
        {
            _debugStringBuilder.AppendFormat("\nBoundaries: {0}\n", _activeBoundary.name);
            _debugStringBuilder.AppendFormat("Horizontal: {0:0.0} / {1:0.0}\n", _minXBoundary, _maxXBoundary);
            _debugStringBuilder.AppendFormat("Vertical: {0:0.0} / {1:0.0}", _minYBoundary, _maxYBoundary);
        }

                
        textObject.text = _debugStringBuilder.ToString(); 
        
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos() { // Draw active bounds
        
        if (!_activeBoundary || !useBoundaries) return;
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



    

