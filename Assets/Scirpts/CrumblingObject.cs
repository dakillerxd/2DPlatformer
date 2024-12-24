
using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider2D))]
public class CrumblingObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeUntilBroken = 3f;
    [SerializeField] private float breakDuration = 3f;
    [SerializeField] [Range(0f, 1f)] private float brokenAlpha;

    [Header("State")] 
    private bool _isBroken;
    private bool _isShaking;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D coll;
    [SerializeField] private ParticleSystem[] crumbleParticleEffects;

    private void Start()
    {
        if (!coll) { coll = GetComponent<Collider2D>(); }
        if (!spriteRenderer) { spriteRenderer = GetComponent<SpriteRenderer>(); }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (_isBroken || _isShaking) return;
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(CountDownToBreak());
        }
        
    }

    private IEnumerator CountDownToBreak() {
        
        if (animator) { animator.SetTrigger("Shake"); }
        _isShaking = true;

        foreach (var particleEffect in crumbleParticleEffects)
        {
            particleEffect.Play();
        }
        
        
        float time = timeUntilBroken;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        Break();
    }
    
    private IEnumerator CountDownToUnBreak() {
        
        float time = breakDuration;
        
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        UnBreak();
    }
    private void Break() {
        _isBroken = true;
        _isShaking = false;
        coll.enabled = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, brokenAlpha);
        if (animator) { animator.SetTrigger("Idle"); }
        foreach (var particleEffect in crumbleParticleEffects)
        {
            particleEffect.Stop();
        }
        StartCoroutine(CountDownToUnBreak());
    }

    private void UnBreak() {
        _isBroken = false;
        _isShaking = false;
        coll.enabled = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);;
    }

}
