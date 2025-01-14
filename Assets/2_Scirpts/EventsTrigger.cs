using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EventsTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private UnityEvent[] eventsOnTrigger;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool _triggered;
    
    private void Start()
    {
        if(hideOnStart) spriteRenderer.enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player") || _triggered) return;
        
        _triggered = true;
        
        foreach (var unityEvent in eventsOnTrigger)
        {
            unityEvent.Invoke();
        }

        
        if (this.isActiveAndEnabled) StartCoroutine(ResetTrigger());
    }

    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(1f);
        _triggered = false;
    }
}
