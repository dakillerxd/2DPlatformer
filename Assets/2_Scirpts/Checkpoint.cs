using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using VInspector;


public class Checkpoint : MonoBehaviour
{
    [System.Serializable]
    public class StateGfx
    {
        public Color innerSpriteColor;
        public Color outerSpriteColor;
        public Color lightColor;
        public bool rotateEffect;
        public ParticleSystem particleEffect;
        public UnityEvent[] events;

        public void InvokeExtraEvents()
        {
            foreach (UnityEvent e in events) {
                e.Invoke();
            }
        }
    }
    
    [Header("Settings")]
    [SerializeField] private StateGfx activeStateGfx;
    [SerializeField] private StateGfx disabledStateGfx;
    [SerializeField] private UnityEvent[] eventsOnActivate;
    [SerializeField] private UnityEvent[] eventsOnDeactivate;
    private bool _isActive = false;

    
    [Foldout("Player Abilities")]
    [SerializeField] private bool receiveAbilityOnUse;
    [EnableIf(nameof(receiveAbilityOnUse))]
    [SerializeField] private bool doubleJump;
    [SerializeField] private bool wallSlide;
    [SerializeField] private bool wallJump;
    [SerializeField] private bool dash;
    [EndIf]
    [EndFoldout]
    
    [Header("References")]
    [SerializeField] private RotateEffect[] rotateEffects;
    [SerializeField] private SpriteRenderer innerSprite;
    [SerializeField] private SpriteRenderer outerSprite;
    [SerializeField] private Light2D light2d;
    [SerializeField] public Animator animator;

    

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
        SetCheckpointGfx(activeStateGfx);
        VFXManager.Instance?.SpawnParticleEffect(activeStateGfx.particleEffect, transform, transform.rotation, transform);
        SoundManager.Instance?.PlaySoundFX("Checkpoint Set");
        CheckpointManager.Instance?.ActivateCheckpoint(this);
    }

    [Button] public void DeactivateCheckpoint() {
        if (!_isActive) return;
        _isActive = false;
        SetCheckpointGfx(disabledStateGfx);
        VFXManager.Instance?.SpawnParticleEffect(disabledStateGfx.particleEffect, transform, transform.rotation, transform);
        foreach (UnityEvent e in eventsOnDeactivate) {
            e.Invoke();
        }
        CheckpointManager.Instance?.DeactivateCheckpoint(this);
    }
    
    public void PlayCheckpointEffects()
    {
        VFXManager.Instance?.PlayAnimationTrigger(animator, "Spawn");
        StartCoroutine(VFXManager.Instance?.LerpColorAdjustments(true, 0.5f));
        
        if (!receiveAbilityOnUse) return;
        if (doubleJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.DoubleJump, false);
        if (wallSlide) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallSlide, false);
        if (wallJump) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.WallJump, false);
        if (dash) PlayerController.Instance?.ReceiveAbility(PlayerAbilities.Dash, false);
    }
    

    private void SetCheckpointGfx(StateGfx stateGfx)
    {
        innerSprite.color = stateGfx.innerSpriteColor;
        outerSprite.color = stateGfx.outerSpriteColor;
        light2d.color = stateGfx.lightColor;
        foreach (RotateEffect rotateEffect in rotateEffects)
        {
            rotateEffect.enabled = stateGfx.rotateEffect;
        }
        
    }


#region Unity Editor
#if UNITY_EDITOR
    
        private void OnValidate() {
            SetCheckpointGfx(disabledStateGfx);
        }
        
#endif
#endregion
    


}
