using UnityEngine;
using System.Collections;
using TMPro;

public class InfoTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hideTriggerOnStart = true;
    [SerializeField] private bool useDelay = false;  // New toggle for delay
    [SerializeField] [Min(0f)] private float fadeInDelay;
    [SerializeField] [Min(0f)] private float fadeInTime = 2f;
    [SerializeField] [Min(0f)] private float fadeOutTime = 3f;
    [SerializeField] private bool fadeOutOnExit = true;

    private readonly Color _invisibleColor = new Color(1f, 1f, 1f, 0f);
    private Color _startColor;
    private Coroutine _fadeCoroutine;
    private bool _isFirstEntry = true;
    private bool _isCoroutineRunning;
    private bool _isPlayerInTrigger;

    [Header("References")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private TextMeshPro infoText;

    private void Start()
    {
        if (hideTriggerOnStart) { triggerSprite.enabled = false; }
        _startColor = infoText.color;
        infoText.color = _invisibleColor;
        _isCoroutineRunning = false;
        _isPlayerInTrigger = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (_isPlayerInTrigger) return;
        _isPlayerInTrigger = true;

        // Stop any existing fade coroutine
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        // Handle first entry with optional delay
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

        StartCoroutine(CheckPlayerExit());
    }

    private IEnumerator CheckPlayerExit()
    {
        yield return new WaitForSeconds(0.1f);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, GetComponent<Collider2D>().bounds.size, 0);
        bool playerStillInside = false;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                playerStillInside = true;
                break;
            }
        }

        if (!playerStillInside)
        {
            _isPlayerInTrigger = false;
            if (!fadeOutOnExit) yield break;
            
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _fadeCoroutine = StartCoroutine(FadeText(false, fadeOutTime));
        }
    }

    private IEnumerator StartFadeWithDelay(float delay)
    {
        _isCoroutineRunning = true;
        yield return new WaitForSeconds(delay);
        _fadeCoroutine = StartCoroutine(FadeText(true, fadeInTime));
        _isCoroutineRunning = false;
    }

    private IEnumerator FadeText(bool fadeIn, float fadeTime)
    {
        _isCoroutineRunning = true;
        Color startColor = infoText.color;
        Color targetColor = fadeIn ? _startColor : _invisibleColor;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            infoText.color = Color.Lerp(startColor, targetColor, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        infoText.color = targetColor;
        _fadeCoroutine = null;
        _isCoroutineRunning = false;
    }
}