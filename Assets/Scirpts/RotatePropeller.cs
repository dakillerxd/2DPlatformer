using UnityEngine;

public class RotatePropeller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 300f;
    [SerializeField] private float maxRotationSpeed = 1000f;
    [SerializeField] private float stopLerpSpeed = 2f;
    [SerializeField] private Rigidbody2D rigidBody2D;

    private float currentRotationSpeed;
    private float targetRotationSpeed;

    private void Update()
    {
        if (!rotate) return;

        if (rigidBody2D)
        {
            float clampedLinearVelocityX = Mathf.Clamp(rigidBody2D.linearVelocityX, -5f, 5f);
            float clampedLinearVelocityY = Mathf.Clamp(rigidBody2D.linearVelocityY, -5f, 5f);
            
            // Check if the rigidbody is effectively stopped
            if (Mathf.Abs(rigidBody2D.linearVelocityX) < 0.01f && Mathf.Abs(rigidBody2D.linearVelocityY) < 0.01f)
            {
                targetRotationSpeed = 0f;
            }
            else
            {
                targetRotationSpeed = rotationSpeed * (clampedLinearVelocityX + (clampedLinearVelocityY * 2f));
                targetRotationSpeed = Mathf.Clamp(targetRotationSpeed, -maxRotationSpeed, maxRotationSpeed);
            }

            // Smoothly lerp to target rotation speed
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, stopLerpSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up * (currentRotationSpeed * Time.deltaTime));
        }
        else
        {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }
}