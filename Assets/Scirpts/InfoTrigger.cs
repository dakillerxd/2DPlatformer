using System;
using UnityEngine;
using System.Collections;
using TMPro;
using VInspector;


public class InfoTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private bool hideTriggerOnStart = true;
    [SerializeField] private bool useDelay;
    [EnableIf(nameof(useDelay))]
    [SerializeField] [Min(0f)] private float fadeInDelay = 1f;
    [EndIf]
    [SerializeField] [Min(0f)] private float fadeInTime = 2f;
    [SerializeField] [Min(0f)] private float fadeOutTime = 2f;
    [SerializeField] private bool fadeOutOnExit = true;
    [EnableIf(nameof(fadeOutOnExit))]
    [SerializeField] private bool destroyAfterFadeOut;
    [EndIf]

    [Header("Text Settings")]
    [SerializeField] private TextType textType = TextType.Custom;
    [EnableIf(nameof(IsInfoText))]
    [SerializeField] private string infoId;
    [EndIf]
    

    [Header("References")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private TextMeshPro infoText;
    
    
    private bool IsInfoText => textType == TextType.Info;
    private readonly Color _invisibleColor = new Color(1f, 1f, 1f, 0f);
    private Color _startColor;
    private Coroutine _fadeCoroutine;
    private bool _isFirstEntry = true;
    private bool _isPlayerInTrigger;

    private void Start()
    {
        if (hideTriggerOnStart) { triggerSprite.enabled = false; }
        
        _startColor = infoText.color;
        infoText.color = _invisibleColor;
        
        _isPlayerInTrigger = false;

        SetupText();
    }
    
    private void SetupText()
    {
        if (!infoText || !IsInfoText) return;
        
        infoText.text = GameManager.Instance?.GetInfoText(infoId);
    }
    
    
    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!IsInfoText) return;
            infoText.text = FindFirstObjectByType<GameManager>().GetInfoText(infoId);
        }     
    #endif

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (_isPlayerInTrigger) return;
        _isPlayerInTrigger = true;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        if (_isFirstEntry)
        {
            _isFirstEntry = false;
            if (useDelay && fadeInDelay > 0f)
            {
                _fadeCoroutine = StartCoroutine(StartFadeWithDelay(fadeInDelay));
            }
            else
            {
                _fadeCoroutine = StartCoroutine(FadeText(true, fadeInTime));
            }
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeText(true, fadeInTime));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        _isPlayerInTrigger = false;
        if (!fadeOutOnExit) return;

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        _fadeCoroutine = StartCoroutine(FadeText(false, fadeOutTime));
    }

    private IEnumerator StartFadeWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _fadeCoroutine = StartCoroutine(FadeText(true, fadeInTime));
    }

    private IEnumerator FadeText(bool fadeIn, float fadeTime)
    {
        Color startColor = infoText.color;
        Color targetColor = fadeIn ? this._startColor : _invisibleColor;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            infoText.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        infoText.color = targetColor;
        _fadeCoroutine = null;
        
        if (destroyAfterFadeOut && !fadeIn)
        {
            Destroy(gameObject);
        }
    }
}