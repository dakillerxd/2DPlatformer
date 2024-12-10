using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class CollectibleItem : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private string collectibleName;
    [SerializeField] private UnityEvent[] eventsAfterTrigger;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro titleText;
    [SerializeField] private GameObject collectibleEffect;
    
    private bool _collected;
    private Color _defaultColor;
    
    
    
    private void Start()
    {
        _defaultColor = spriteRenderer.color;
        
        if (GameManager.Instance.IsCollectibleCollected(SceneManager.GetActiveScene().name))
        {
            SetCollectibleState(true);
        } else {
            SetCollectibleState(false);
        }
        
    }
    
    private void OnValidate()
    {
        if (titleText) titleText.text = collectibleName;
    }
    

    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if (_collected) return;
        if (!collision.CompareTag("Player")) return;
        
        CameraController.Instance?.ShakeCamera(2f, 5f,2,2);
        SoundManager.Instance?.PlaySoundFX("Player Receive Collectible");
        GameManager.Instance?.CollectCollectible(SceneManager.GetActiveScene().name);
        foreach (var e in eventsAfterTrigger)
        {
            e.Invoke();
        }
        SetCollectibleState(true);
        
    }

    
    private void SetCollectibleState(bool collected)
    {
        _collected = collected;
        
        Color semiTransparentColor = _defaultColor;
        semiTransparentColor.a = 0.3f;
        
        if (spriteRenderer) { spriteRenderer.color = _collected ? semiTransparentColor : _defaultColor; }
        
        if (titleText) { titleText.gameObject.SetActive(!_collected); }
        
        if (collectibleEffect) { collectibleEffect.SetActive(!_collected); }
        
        gameObject.SetActive(!_collected);
        
    }
}
