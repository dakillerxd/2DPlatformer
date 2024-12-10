using System;
using UnityEngine;

public class RotatePropeller : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private Rigidbody2D rigidBody2D;
    
    private void Update()
    {
        if (!rotate) return;
        


        if (rigidBody2D) {
            
            float  clampedLinearVelocityX = Mathf.Clamp( rigidBody2D.linearVelocityX, -5f, 5f);
            float  clampedLinearVelocityY = Mathf.Clamp( rigidBody2D.linearVelocityY, -5f, 5f);
            float rotationAmount = rotationSpeed * (clampedLinearVelocityX + (clampedLinearVelocityY) * 2f) * Time.deltaTime;
            transform.Rotate(Vector3.up * rotationAmount);
            
        } else {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }



}
