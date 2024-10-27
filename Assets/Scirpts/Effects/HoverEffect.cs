using UnityEngine;

public class HoverEffect : MonoBehaviour
{


    [Header("Hover Settings")]
    [SerializeField] private bool enableHover = false;
    [SerializeField] private float hoverSpeed = 0f;
    [SerializeField] private float hoverAmount = 0f;
    [SerializeField] private Vector3 hoverDirection;



    private Vector3 initialPosition;
    private float hoverTime;

    private void Start()
    {
        initialPosition = transform.position;
        hoverTime = Random.value * Mathf.PI * 2;

    }

    private void FixedUpdate()
    {
        Hover();
        
    }

    private void Hover()
    {
        if (!enableHover) return;

        hoverTime += hoverSpeed * Time.fixedDeltaTime;

        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmount;
        Vector3 normalizedHoverDir = hoverDirection.normalized;
        Vector3 newPosition = initialPosition + normalizedHoverDir * hoverOffset;
        transform.position = newPosition;
    }
    
}