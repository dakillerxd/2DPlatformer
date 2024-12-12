using TMPro;
using UnityEngine;



public class CosmeticItem : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private CosmeticItems cosmeticItem = CosmeticItems.PropellerHat;
    
    [Header("References")]
    [SerializeField] private GameObject googlyEyes;
    [SerializeField] private GameObject propellerHat;
    [SerializeField] private GameObject curlyMustache;
    [SerializeField] private TextMeshPro titleText;
    
    
    
    private void Start()
    {
        titleText.text = System.Text.RegularExpressions.Regex.Replace(cosmeticItem.ToString(), "([A-Z])", " $1").Trim();
        
        
        if (propellerHat) propellerHat.SetActive(false);
        if (googlyEyes) googlyEyes.SetActive(false);
        if (curlyMustache) curlyMustache.SetActive(false);
        
        switch (cosmeticItem)
        { 
                    
            case CosmeticItems.GooglyEye:

                if (googlyEyes) googlyEyes.SetActive(true);
                
                break;
            case CosmeticItems.PropellerHat:
                
                if (propellerHat) propellerHat.SetActive(true);
                
                break;
            case CosmeticItems.CurlyMustache:
                
                if (curlyMustache) curlyMustache.SetActive(true);
                
                break;

        }
    }


    private  void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        PlayerController player = collision.GetComponentInParent<PlayerController>();
        
        
        switch (cosmeticItem)
        {
            case CosmeticItems.GooglyEye:
                player.ToggleCosmetic("Googly Eye", true);
                break;
            case CosmeticItems.PropellerHat:
                player.ToggleCosmetic("Propeller Hat", true);
                break;
            case CosmeticItems.CurlyMustache:
                player.ToggleCosmetic("Curly Mustache", true);
                break;
        }
        
        // CameraController.Instance?.ShakeCamera(2f, 5f,2,2);
        SoundManager.Instance?.PlaySoundFX("Player Receive Collectible");
        Destroy(gameObject);
        
    }

}
