using UnityEngine;

public enum EyeMode
{
    LookAtMouse,
    PhysicsEffected,
    Combined
}

public class GooglyEyes : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private EyeMode eyeMode;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    
    [Header("Look At Mouse Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] [Range(0, 1)] private float maxHorizontalDistance = 0.03f;
    [SerializeField] [Range(0, 1)] private float maxVerticalDistance = 0.05f;
    
    [Header("Physics Mode Settings")]
    [SerializeField] private float wobbleSpeed = 5f;
    [SerializeField] private float wobbleIntensity = 0.3f;
    [SerializeField] [Range(0, 1)] private float maxHorizontalOffset = 0.05f;
    [SerializeField] [Range(0, 1)] private float maxVerticalOffset = 0.05f;
    private float _timeOffset;

    [Header("Combined Mode Settings")]
    [SerializeField] [Range(0, 1)] private float mousePriority = 0.7f;
    [SerializeField] [Range(0, 1)] private float wobblePriority = 0.3f;

    [Header("References")]
    [SerializeField] private Transform eye;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRigidbody;

    private Camera _mainCamera;
    private bool _isFacingRight = true;

    private void Start()
    {
        _mainCamera = Camera.main;
        _startPosition = eye.localPosition;
        _timeOffset = Random.Range(0f, 10f);
    }
    
    private void Update()
    {
        // Check if player is flipped using Y rotation
        _isFacingRight = Mathf.Approximately(playerTransform.rotation.eulerAngles.y, 0f);

        switch (eyeMode)
        {
            case EyeMode.LookAtMouse:
                LookAtMouse();
                break;
            case EyeMode.PhysicsEffected:
                PhysicsEffected();
                break;
            case EyeMode.Combined:
                CombinedMode();
                break;
        }
    }

    private void LookAtMouse()
    {
        if (_mainCamera != null)
        {
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Calculate direction from eye to mouse
            Vector3 direction = (mouseWorldPos - transform.position).normalized;

            // Calculate distance to mouse
            float distanceToMouse = Vector3.Distance(transform.position, mouseWorldPos);

            // Prepare to clamp horizontal and vertical movement separately
            float horizontalOffset = direction.x * Mathf.Min(Mathf.Abs(direction.x), maxHorizontalDistance);
            float verticalOffset = direction.y * Mathf.Min(Mathf.Abs(direction.y), maxVerticalDistance);

            // Flip the horizontal direction if facing left
            if (!_isFacingRight)
            {
                horizontalOffset *= -1;
            }

            // Calculate target position with clamped offsets
            _targetPosition = _startPosition + new Vector3(horizontalOffset, verticalOffset, 0);
        }

        // Smoothly move the eye
        eye.localPosition = Vector3.Lerp(eye.localPosition, _targetPosition, moveSpeed * Time.deltaTime);
    }

    private void PhysicsEffected()
    {
        // Use Perlin noise for smooth, random wobble
        float xWobble = Mathf.PerlinNoise(_timeOffset + Time.time * wobbleSpeed, 0) * 2 - 1;
        float yWobble = Mathf.PerlinNoise(0, _timeOffset + Time.time * wobbleSpeed) * 2 - 1;

        // Create wobble offset
        Vector3 wobbleOffset = new Vector3(
            xWobble * wobbleIntensity, 
            yWobble * wobbleIntensity, 
            0
        );

        // Apply physics-based movement if rigidbody is assigned
        if (playerRigidbody != null)
        {
            // Add some subtle physics-based randomness
            Vector2 physicsOffset = playerRigidbody.linearVelocity * 0.1f;
            wobbleOffset += new Vector3(physicsOffset.x, physicsOffset.y, 0);
        }

        // Flip the wobble if facing left
        if (!_isFacingRight)
        {
            wobbleOffset.x *= -1;
        }

        // Clamp horizontal and vertical offsets
        wobbleOffset.x = Mathf.Clamp(wobbleOffset.x, -maxHorizontalOffset, maxHorizontalOffset);
        wobbleOffset.y = Mathf.Clamp(wobbleOffset.y, -maxVerticalOffset, maxVerticalOffset);

        // Calculate target position with wobble
        _targetPosition = _startPosition + wobbleOffset;

        // Smoothly move the eye
        eye.localPosition = Vector3.Lerp(eye.localPosition, _targetPosition, moveSpeed * Time.deltaTime);
    }

    private void CombinedMode()
    {
        Vector3 mouseOffset = Vector3.zero;
        Vector3 wobbleOffset = Vector3.zero;

        // Calculate mouse offset
        if (_mainCamera != null)
        {
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // Calculate direction from eye to mouse
            Vector3 direction = (mouseWorldPos - transform.position).normalized;

            // Prepare to clamp horizontal and vertical movement separately
            float horizontalOffset = direction.x * Mathf.Min(Mathf.Abs(direction.x), maxHorizontalDistance);
            float verticalOffset = direction.y * Mathf.Min(Mathf.Abs(direction.y), maxVerticalDistance);

            // Flip the horizontal direction if facing left
            if (!_isFacingRight)
            {
                horizontalOffset *= -1;
            }

            mouseOffset = new Vector3(horizontalOffset, verticalOffset, 0);
        }

        // Calculate wobble offset
        float xWobble = Mathf.PerlinNoise(_timeOffset + Time.time * wobbleSpeed, 0) * 2 - 1;
        float yWobble = Mathf.PerlinNoise(0, _timeOffset + Time.time * wobbleSpeed) * 2 - 1;

        wobbleOffset = new Vector3(
            xWobble * wobbleIntensity, 
            yWobble * wobbleIntensity, 
            0
        );

        // Apply physics-based movement if rigidbody is assigned
        if (playerRigidbody != null)
        {
            // Add some subtle physics-based randomness
            Vector2 physicsOffset = playerRigidbody.linearVelocity * 0.1f;
            wobbleOffset += new Vector3(physicsOffset.x, physicsOffset.y, 0);
        }

        // Flip the wobble if facing left
        if (!_isFacingRight)
        {
            wobbleOffset.x *= -1;
        }

        // Clamp horizontal and vertical offsets for wobble
        wobbleOffset.x = Mathf.Clamp(wobbleOffset.x, -maxHorizontalOffset, maxHorizontalOffset);
        wobbleOffset.y = Mathf.Clamp(wobbleOffset.y, -maxVerticalOffset, maxVerticalOffset);

        // Blend mouse and wobble offsets based on priorities
        Vector3 combinedOffset = (mouseOffset * mousePriority) + (wobbleOffset * wobblePriority);

        // Calculate target position with combined offset
        _targetPosition = _startPosition + combinedOffset;

        // Smoothly move the eye
        eye.localPosition = Vector3.Lerp(eye.localPosition, _targetPosition, moveSpeed * Time.deltaTime);
    }
}