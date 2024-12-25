using UnityEngine;

public class MustacheAnimation : MonoBehaviour
{

    
    [Header("Animation Settings")]
    [SerializeField] private float waveAmount = 15f;
    [SerializeField] private float waveSpeed = 2f;
    
    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody2D;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        Vector3 newPosition = startPosition;
        Quaternion newRotation = startRotation;

        if (rigidBody2D)
        {
            // Only apply wave effect when there's horizontal movement
            if (Mathf.Abs(rigidBody2D.linearVelocity .x) > 0)
            {
                float horizontalSpeed = Mathf.Abs(rigidBody2D.linearVelocity .x);
                float waveMultiplier = 1f + (horizontalSpeed * 0.1f);
                float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmount * waveMultiplier;
                newRotation *= Quaternion.Euler(0, 0, wave);
            }
        }
        else
        {
            // Basic wave effect when no Rigidbody2D (unchanged)
            float wave = Mathf.Sin(Time.time * waveSpeed) * waveAmount;
            newRotation *= Quaternion.Euler(0, 0, wave);
        }

        // Apply transformations
        transform.localPosition = newPosition;
        transform.localRotation = newRotation;
    }
}