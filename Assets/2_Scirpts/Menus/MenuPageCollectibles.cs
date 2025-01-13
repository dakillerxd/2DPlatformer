using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuPageCollectibles : MenuPage
{
    [Header("UI Elements")]
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private Button buttonBack;
    [SerializeField] private TextMeshProUGUI coinsHeaderText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject googlyEyes;
    [SerializeField] private GameObject propellerHat;
    [SerializeField] private GameObject curlyMustache;
    [SerializeField] private GameObject unlocksContainer;
    
    
    private readonly StringBuilder _collectiblesStringBuilder = new StringBuilder(256);
    


    protected override void Start()
    {
        if (buttonBack != null) {
            
            buttonBack.onClick.RemoveAllListeners();
            buttonBack.onClick.AddListener(() => SoundManager.Instance?.PlaySoundFX("ButtonClick"));
            buttonBack.onClick.AddListener(() => menuCategoryMain.SelectPage(menuCategoryMain.mainMenuPage));
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
        if (GameManager.Instance.unlocks.Length == 0 || !togglePrefab || !unlocksContainer) return;
    
        foreach (Transform child in unlocksContainer.transform) // Delete all buttons before creating new ones
        {
            Destroy(child.gameObject);
        }
    
        for (int i = 0; i < GameManager.Instance.unlocks.Length; i++) // Using index loop instead of foreach
        {
            Unlock unlock = GameManager.Instance.unlocks[i];
            GameObject toggleObject = Instantiate(togglePrefab.gameObject, unlocksContainer.transform);
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            
            toggleObject.GetComponentInChildren<TextMeshProUGUI>().text = $"{unlock.unlockedAtCollectible} Coins: {unlock.name}";
            toggleObject.name = unlock.name;
            
            toggle.isOn = unlock.state;
            toggle.interactable = unlock.received;
            toggle.onValueChanged.AddListener((value) => GameManager.Instance?.ToggleUnlock(unlock.name, value));
            toggle.onValueChanged.AddListener((value) => SoundManager.Instance?.PlaySoundFX("Toggle"));

            selectables.Add(toggle);
            SetupSelectable(toggle);
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
