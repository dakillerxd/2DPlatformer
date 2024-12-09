using System;
using UnityEngine;

public class RotatePropeller : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotationSpeed = 200f;
    
    private void Update()
    {
        if (rotate)
        {
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }



}
