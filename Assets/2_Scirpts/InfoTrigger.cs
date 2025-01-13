using UnityEngine;
using TMPro;
using VInspector;
using PrimeTween;

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

    [Header("Text")]
    [SerializeField] private TextType textType = TextType.Custom;
    [EnableIf(nameof(IsInfoText))]
    [SerializeField] private string infoId;
    [EndIf]

    [Header("References")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private TextMeshPro infoText;
    
    private bool IsInfoText => textType == TextType.Info;
    private Tween _currentTween;
    private bool _isFirstEntry = true;
    private bool _isPlayerInTrigger;

    private void Start()
    {
        if (hideTriggerOnStart) { triggerSprite.enabled = false; }
        
        infoText.alpha = 0f;
        _isPlayerInTrigger = false;

        SetupText();
    }

    [OnValueChanged(nameof(infoId))]
    private void SetupText()
    {
        if (!infoText || !IsInfoText) return;
        
        infoText.text = GameManager.Instance?.GetInfoText(infoId);
        
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!infoText || !IsInfoText) return;
        infoText.text = FindFirstObjectByType<GameManager>().GetInfoText(infoId);
    }
    #endif

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (_isPlayerInTrigger) return;
        _isPlayerInTrigger = true;

        _currentTween.Stop();

        float delay = (_isFirstEntry && useDelay) ? fadeInDelay : 0f;
        _isFirstEntry = false;

        _currentTween = Tween.Alpha(infoText, endValue: 1f, duration: fadeInTime, startDelay: delay, ease: Ease.InOutSine);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        _isPlayerInTrigger = false;
        if (!fadeOutOnExit) return;

        _currentTween.Stop();
        _currentTween = Tween.Alpha(infoText, endValue: 0f, duration: fadeOutTime, ease: Ease.InOutSine)
            .OnComplete(() => {
                if (destroyAfterFadeOut)
                {
                    Destroy(gameObject);
                }
            });
    }
}