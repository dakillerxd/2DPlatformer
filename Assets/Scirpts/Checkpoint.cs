using UnityEngine;
using UnityEngine.Events;
using VInspector;

public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color disabledColor = Color.white; 
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private ParticleSystem activateVfx;
    [SerializeField] private ParticleSystem deactivateVfx;
    [SerializeField] private UnityEvent[] eventsOnActivate;
    [SerializeField] private UnityEvent[] eventsOnDeactivate;
    private bool _isActive;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;


    private void Start() {
        SeCheckpointColor(disabledColor);
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        switch (collision.gameObject.tag) {
            case "Player":
                
                ActivateCheckpoint();
                break;
            case "Enemy":
                DeactivateCheckpoint();
                break;
        }
    }

    [Button] public void ActivateCheckpoint() {
        
        PlayerController.Instance?.HealToFullHealth();
        if (_isActive) return;
        
        _isActive = true;
        SeCheckpointColor(activeColor);
        foreach (UnityEvent e in eventsOnActivate) {
            e.Invoke();
        }
        VFXManager.Instance?.SpawnParticleEffect(activateVfx, transform, transform.rotation, transform);
        CheckpointManager.Instance?.ActivateCheckpoint(this);
        SoundManager.Instance?.PlaySoundFX("Checkpoint Set");
    }

    [Button] public void DeactivateCheckpoint() {
        if (!_isActive) return;
        _isActive = false;
        SeCheckpointColor(disabledColor);
        foreach (UnityEvent e in eventsOnDeactivate) {
            e.Invoke();
        }
        VFXManager.Instance?.SpawnParticleEffect(deactivateVfx, transform, transform.rotation, transform);
        CheckpointManager.Instance?.DeactivateCheckpoint(this);
    }
    
    private void SeCheckpointColor(Color color) 
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.color = color;
        }
    }
    


}
