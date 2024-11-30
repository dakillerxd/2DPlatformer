using UnityEngine;
using System.Collections;
using TMPro;

public class InfoTrigger : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool hideTriggerOnStart = true;
    [SerializeField] private float fadeInTime = 2f;
    [SerializeField] private float fadeOutTime = 3f;
    [SerializeField] private bool fadeOutOnExit = true;
    
    private readonly Color _invisibleColor = new Color(1f, 1f, 1f, 0f);
    private Color _startColor;
    private bool _triggered;
    private Coroutine _fadeCoroutine;

    [Header("References")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private TextMeshPro infoText;
    
    private void Start()
    {
        if(hideTriggerOnStart) {triggerSprite.enabled = false;}
        _startColor = infoText.color;
        infoText.color = _invisibleColor;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.CompareTag("Player")) return;
        
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeText(true, fadeInTime));
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player") || !_triggered || !fadeOutOnExit) return;
        
        _fadeCoroutine = StartCoroutine(FadeText(false, fadeOutTime));
    }

    private IEnumerator FadeText(bool fadIn, float time)
    {
        Color startColor = infoText.color;
        Color targetColor = fadIn ? _startColor : _invisibleColor;
        float elapsedTime = 0f;
        
        _triggered = fadIn;

        while(elapsedTime < time)
        {
            infoText.color = Color.Lerp(startColor, targetColor, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        infoText.color = targetColor;
        _fadeCoroutine = null;
    }
}