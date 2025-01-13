using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuPageCollectibles : MenuPage
{
    [Header("UI Elements")]
    [SerializeField] private Button buttonBack;
    
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
    


    protected override void Start()
    {
        if (buttonBack != null) {
            
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => _menuCategoryMain.SelectPage(_menuCategoryMain.mainMenuPage));
        }
        
        UpdateCollectibles();
        UpdateUnlocks();
        
        base.Start();
    }
    
    

    private void Update()
    {
        if (normalEyes) normalEyes.SetActive(!GameManager.Instance.CheckUnlockActive("Googly Eye"));
        if (googlyEyes) googlyEyes.SetActive(GameManager.Instance.CheckUnlockActive("Googly Eye"));
        if (propellerHat) propellerHat.SetActive(GameManager.Instance.CheckUnlockActive("Propeller Hat"));
        if (curlyMustache) curlyMustache.SetActive(GameManager.Instance.CheckUnlockActive("Curly Mustache"));
    }

    private void UpdateUnlocks()
    {
        if (googlyEyesToggle) {
            googlyEyesToggle.isOn = GameManager.Instance.CheckUnlockActive("Googly Eye");
            googlyEyesToggle.interactable = GameManager.Instance.CheckUnlockReceived("Googly Eye");
            googlyEyesToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Googly Eye", value));
            googlyEyesToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
            selectables.Add(googlyEyesToggle);
            SetupSelectable(googlyEyesToggle);
            StoreOriginalTransforms(googlyEyesToggle);
            StoreOriginalState(googlyEyesToggle);
        }
        
        if (propellerHatToggle) {
            propellerHatToggle.isOn = GameManager.Instance.CheckUnlockActive("Propeller Hat");
            propellerHatToggle.interactable = GameManager.Instance.CheckUnlockReceived("Propeller Hat");
            propellerHatToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Propeller Hat", value));
            propellerHatToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
            selectables.Add(propellerHatToggle);
            SetupSelectable(propellerHatToggle);
            StoreOriginalTransforms(propellerHatToggle);
            StoreOriginalState(propellerHatToggle);
        }
        
        if (curlyMustacheToggle)
        {
            curlyMustacheToggle.isOn = GameManager.Instance.CheckUnlockActive("Curly Mustache");
            curlyMustacheToggle.interactable = GameManager.Instance.CheckUnlockReceived("Curly Mustache");
            curlyMustacheToggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock("Curly Mustache", value));
            curlyMustacheToggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));
            selectables.Add(curlyMustacheToggle);
            SetupSelectable(curlyMustacheToggle);
            StoreOriginalTransforms(curlyMustacheToggle);
            StoreOriginalState(curlyMustacheToggle);
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
        
        
        if (coinsText)
        {
            _collectiblesStringBuilder.Clear();
        
            if (GameManager.Instance.levels != null)
            {
                foreach (Level level in GameManager.Instance.levels)
                {
                    if (level is { countsTowardsUnlocks: true })
                    {
                        _collectiblesStringBuilder.AppendFormat("{0}: {1}\n \n", level.name ?? "Unknown", level.collectibleCollected ? "<color=green>Collected</color>" : "<color=red>Not Collected</color>");
                    }
                }
            
                coinsText.text = _collectiblesStringBuilder.ToString();
            }
        }
        

    }
    
}
