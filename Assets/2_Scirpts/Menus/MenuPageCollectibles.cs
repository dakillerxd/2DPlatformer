using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuPageCollectibles : MenuPage
{
    
    [Header("Collectibles")]
    [SerializeField] private TextMeshProUGUI coinsHeaderText;
    [SerializeField] private TextMeshProUGUI coinsText;
    
    [Header("Unlocks")]
    [SerializeField] private TextMeshProUGUI unlocksHeaderText;
    [SerializeField] private TextMeshProUGUI unlocksText;
    [Space(10f)]
    [SerializeField] GameObject playerGfx;
    [SerializeField] private Toggle googlyEyesToggle;
    [SerializeField] private Toggle propellerHatToggle;
    [SerializeField] private Toggle curlyMustacheToggle;
    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject googlyEyes;
    [SerializeField] private GameObject propellerHat;
    [SerializeField] private GameObject curlyMustache;
    private readonly StringBuilder _collectiblesStringBuilder = new StringBuilder(256);
    private readonly StringBuilder _unlockStringBuilder = new StringBuilder(256);
    
    
    [Header("Other")]
    [SerializeField] private Button buttonBack;
    
    private MenuCategoryMainMenu _menuCategoryMain;

    private void Start()
    {
        _menuCategoryMain = GetComponentInParent<MenuCategoryMainMenu>();
        
        if (buttonBack != null) {
            
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategory.SelectPage(_menuCategoryMain.mainMenuPage));
        }
        
        UpdateCollectibles();
        UpdateUnlocks();
    }

    private void Update()
    {
        if (normalEyes != null) normalEyes.SetActive(!GameManager.Instance.CheckUnlockActive("Googly Eye"));
        if (googlyEyes != null) googlyEyes.SetActive(GameManager.Instance.CheckUnlockActive("Googly Eye"));
        if (propellerHat != null) propellerHat.SetActive(GameManager.Instance.CheckUnlockActive("Propeller Hat"));
        if (curlyMustache != null) curlyMustache.SetActive(GameManager.Instance.CheckUnlockActive("Curly Mustache"));
    }

    private void UpdateUnlocks()
    {
        if (googlyEyesToggle != null) {
            googlyEyesToggle.isOn = GameManager.Instance.CheckUnlockActive("Googly Eye");
            googlyEyesToggle.interactable = GameManager.Instance.CheckUnlockReceived("Googly Eye");
            googlyEyesToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Googly Eye", value));
            googlyEyesToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
        }
        
        if (propellerHatToggle != null) {
            propellerHatToggle.isOn = GameManager.Instance.CheckUnlockActive("Propeller Hat");
            propellerHatToggle.interactable = GameManager.Instance.CheckUnlockReceived("Propeller Hat");
            propellerHatToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Propeller Hat", value));
            propellerHatToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
            
        }
        
        if (curlyMustacheToggle != null) {
            curlyMustacheToggle.isOn = GameManager.Instance.CheckUnlockActive("Curly Mustache");
            curlyMustacheToggle.interactable = GameManager.Instance.CheckUnlockReceived("Curly Mustache");
            curlyMustacheToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Curly Mustache", value));
            curlyMustacheToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
        }

        if (!unlocksText || GameManager.Instance.unlocks != null)
        {
            _unlockStringBuilder.Clear();
            
            foreach (Unlock unlock in GameManager.Instance.unlocks)
            {
                _unlockStringBuilder.AppendLine($"{unlock.unlockedAtCollectible} Coins: {unlock.unlockName} \n");
            }


            unlocksText.text = _unlockStringBuilder.ToString();
        }

    }
    
    
    
    private void UpdateCollectibles() 
    {

        if (coinsHeaderText)
        {
            coinsHeaderText.text = $"Coins: {GameManager.Instance.TotalCollectiblesCollected()} / {GameManager.Instance.TotalCollectiblesAmount()}";
        }
        
        
        if (coinsText == null) return;
        
        _collectiblesStringBuilder.Clear();
        
        if (GameManager.Instance.collectibles != null)
        {
            foreach (Collectible collectible in GameManager.Instance.collectibles)
            {
                if (collectible != null && collectible.countsTowardsUnlocks)
                {
                    _collectiblesStringBuilder.AppendFormat("{0}: {1}\n \n", collectible.connectedLevel?.SceneName ?? "Unknown", collectible.collected ? "<color=green>Collected</color>" : "<color=red>Not Collected</color>");
                }
            }
            
            coinsText.text = _collectiblesStringBuilder.ToString();
        }
    }
    
}
