using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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
    
    [Header("Player Abilities")]
    [SerializeField] private bool receiveAbilityOnUse;
    [EnableIf(nameof(receiveAbilityOnUse))]
    [SerializeField] private bool doubleJump;
    [SerializeField] private bool wallSlide;
    [SerializeField] private bool wallJump;
    [SerializeField] private bool dash;
    [EndIf]
    
    [Header("References")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] public Animator animator;


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
    
    public void PlayCheckpointEffects()
    {
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Spawn");
        
        if (!receiveAbilityOnUse) return;
        if (doubleJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.DoubleJump, false);
        if (wallSlide) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallSlide, false);
        if (wallJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallJump, false);
        if (dash) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.Dash, false);
    }
    
    private void SeCheckpointColor(Color color) 
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers) {
            spriteRenderer.color = color;
        }
    }
    


}
