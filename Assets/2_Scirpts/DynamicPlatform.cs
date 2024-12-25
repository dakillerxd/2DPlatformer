using System.Linq;
using UnityEngine;

public class DynamicPlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float velocityMultiplier = 0.1f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float maxDepression = 0.5f;

    private Rigidbody2D rb;
    private Vector2 originalPosition;
    private bool isDepressed;
    private bool playerOnTop;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        originalPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Only return to original position when player is not on top AND platform is depressed
        if (!playerOnTop && isDepressed)
        {
            Vector2 currentPos = transform.position;
            transform.position = Vector2.Lerp(currentPos, originalPosition, Time.fixedDeltaTime * returnSpeed);

            if (Vector2.Distance(currentPos, originalPosition) < 0.01f)
            {
                isDepressed = false;
                transform.position = originalPosition;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!collision.contacts.Any(contact => contact.normal.y < -0.7f)) return;
        playerOnTop = true;
        isDepressed = true;
                    
        float verticalVelocity = Mathf.Abs(collision.relativeVelocity.y);
        float depression = Mathf.Min(
            verticalVelocity * velocityMultiplier,
            maxDepression
        );
                    
        Vector2 newPosition = (Vector2)transform.position + Vector2.down * depression;
        rb.MovePosition(newPosition);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTop = false;
        }
    }
}