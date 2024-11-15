using UnityEngine;
using UnityEditor;


public class FakeObject : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool destroyOnTrigger = true;
    
    [Header("Colors")]
    [SerializeField] private Color invincibilityColor = new Color(1,1,1,0.5f);
    private readonly Color _defaultColor = Color.white;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (destroyOnTrigger)
        {
            Destroy(gameObject);
        }
        else
        {
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = invincibilityColor;
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
            if (!other.CompareTag("Player")) return;
        
            if (!destroyOnTrigger)
            {
                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.color = _defaultColor;
                }
            }

        
    }
    
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 10;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.UpperCenter;
        
        Handles.Label(transform.position, "Fake Object", style);
    }
#endif


}
